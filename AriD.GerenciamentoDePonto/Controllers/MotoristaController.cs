using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores.Permissao;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Collections.Generic;
using AriD.Servicos.Extensao;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class MotoristaController : BaseController
    {
        private readonly IServico<Motorista> _motoristaServico;
        private readonly IServico<Servidor> _servidorServico;
        private readonly IServico<MotoristaHistoricoSituacao> _motoristaHistoricoServico;

        public MotoristaController(
            IServico<Motorista> motoristaServico,
            IServico<Servidor> servidorServico,
            IServico<MotoristaHistoricoSituacao> motoristaHistoricoServico)
        {
            _motoristaServico = motoristaServico;
            _servidorServico = servidorServico;
            _motoristaHistoricoServico = motoristaHistoricoServico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Motorista> listaPaginada)
        {
            try
            {
                if (!HttpContext.PossuiPermissao(eItemDePermissao_Motorista.Visualizar))
                    return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Motorista> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View("_TabelaPaginada", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Modal(int motoristaId)
        {
            var model = motoristaId == 0 ?
                    new Motorista { Situacao = AriD.BibliotecaDeClasses.Enumeradores.eStatusMotorista.Ativo, EmissaoCNH = DateTime.Today, VencimentoCNH = DateTime.Today.AddYears(5) } :
                    _motoristaServico.Obtenha(motoristaId);

            var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
            var servidores = _servidorServico.ObtenhaLista(s => s.OrganizacaoId == organizacaoId).Select(s => new { Id = s.Id, Nome = s.Pessoa?.Nome ?? $"Servidor {s.Id}" }).OrderBy(s => s.Nome).ToList();
            ViewBag.Servidores = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(servidores, "Id", "Nome", model.ServidorId);

            var html = await RenderizarComoString("_Modal", model);
            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public IActionResult Salvar(Motorista motorista)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Motorista.CadastrarOuAlterar))
                throw new ApplicationException("Você não tem permissão para salvar.");

            int id = motorista.Id;
            motorista.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (motorista.Id == 0)
                id = _motoristaServico.Adicionar(motorista);
            else
            {
                var original = _motoristaServico.Obtenha(motorista.Id);
                var situacaoAnterior = original.Situacao;

                original.ServidorId = motorista.ServidorId;
                original.NumeroCNH = motorista.NumeroCNH;
                original.CategoriaCNH = motorista.CategoriaCNH;
                original.EmissaoCNH = motorista.EmissaoCNH;
                original.VencimentoCNH = motorista.VencimentoCNH;
                original.Situacao = motorista.Situacao;
                original.Observacoes = motorista.Observacoes;
                _motoristaServico.Atualizar(original);

                if (situacaoAnterior != original.Situacao)
                {
                    _motoristaHistoricoServico.Adicionar(new MotoristaHistoricoSituacao
                    {
                        OrganizacaoId = motorista.OrganizacaoId,
                        MotoristaId = motorista.Id,
                        UsuarioId = this.HttpContext.DadosDaSessao().UsuarioId,
                        SituacaoAnterior = situacaoAnterior,
                        SituacaoNova = original.Situacao,
                        DataAlteracao = DateTime.Now
                    });
                }
            }

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpGet]
        public IActionResult ObtenhaHistoricoSituacao(int motoristaId)
        {
            var historico = _motoristaHistoricoServico.ObtenhaLista(h => h.MotoristaId == motoristaId)
                .OrderByDescending(h => h.DataAlteracao)
                .Select(h => new {
                    DataAlteracao = h.DataAlteracao.ToString("dd/MM/yyyy HH:mm"),
                    SituacaoAnterior = h.SituacaoAnterior.DescricaoDoEnumerador(),
                    SituacaoNova = h.SituacaoNova.DescricaoDoEnumerador(),
                    Usuario = h.Usuario?.NomeDaPessoa ?? "-"
                });
            return Json(new { sucesso = true, historico = historico });
        }

        [HttpPost]
        public IActionResult Remova(int motoristaId)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Motorista.Excluir))
                throw new ApplicationException("Você não tem permissão para remover.");

            var item = _motoristaServico.Obtenha(motoristaId);
            _motoristaServico.Remover(item);

            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Motorista> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaMotorista>(listaPaginada.Adicional);
            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<Motorista, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (parametros.Situacao.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => c.Situacao == parametros.Situacao.Value);

            if (parametros.CategoriaCNH.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => c.CategoriaCNH == parametros.CategoriaCNH.Value);

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Servidor.Pessoa.Nome.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.NumeroCNH.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _motoristaServico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}
