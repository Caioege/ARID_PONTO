using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class EscalaController : BaseController
    {
        private readonly IServicoDeEscala _servico;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;
        private readonly IServico<VinculoDeTrabalho> _servicoVinculoDeTrabalho;

        public EscalaController(
            IServicoDeEscala servico,
            IServico<UnidadeOrganizacional> servicoUnidade,
            IServico<VinculoDeTrabalho> servicoVinculoDeTrabalho)
        {
            _servico = servico;
            _servicoUnidade = servicoUnidade;
            _servicoVinculoDeTrabalho = servicoVinculoDeTrabalho;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Escala> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Escala> listaPaginada)
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
        public IActionResult Adicionar()
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();
                if (dadosDaSessao.Perfil == ePerfilDeAcesso.Organizacao)
                    ViewBag.Unidades = new SelectList(
                            _servicoUnidade
                            .ObtenhaLista(c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId && c.Ativa)
                            .OrderBy(c => c.Nome),
                        "Id",
                        "Nome");

                return View(new Escala());
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult Alterar(int id)
        {
            try
            {
                return View(_servico.Obtenha(id));
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(Escala escala)
        {
            int id = escala.Id;
            escala.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (escala.Id == 0)
                id = _servico.Adicionar(escala);
            else
                _servico.Atualizar(escala);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public ActionResult RemoverEscala(int id)
        {
            _servico.RemoverEscala(id);
            return Json(new { sucesso = true, mensagem = "A escala foi removida." });
        }

        [HttpGet]
        public async Task<ActionResult> ModalCiclo(int id, int escalaId)
        {
            var ciclo = id > 0 ?
                _servico.ObtenhaCiclo(id) 
                : new();

            if (id == 0)
                ciclo.Ciclo = (_servico.Obtenha(escalaId)
                    .Ciclos
                    .OrderBy(c => c.Ciclo)
                    .LastOrDefault()?.Ciclo ?? 0) + 1;

            var html = await RenderizarComoString("_ModalCiclo", ciclo);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public ActionResult SalvarCiclo(CicloDaEscala cicloDaEscala)
        {
            cicloDaEscala.OrganizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
            _servico.AdicioneOuAltereCiclo(cicloDaEscala);
            return Json(new { sucesso = true, mensagem = "Os dados foram salvos." });
        }

        [HttpPost]
        public ActionResult RemoverCiclo(int id)
        {
            _servico.RemoverCiclo(id);
            return Json(new { sucesso = true, mensagem = "O ciclo foi removido." });
        }

        [HttpGet]
        public async Task<ActionResult> ModalServidorCiclo(int id, int escalaId)
        {
            var servidorEscala = id > 0 ?
                _servico.ObtenhaEscalaDoServidor(id) :
                new() { Data = DateTime.Today };

            if (id == 0)
            {
                var escala = _servico.Obtenha(escalaId);
                var dadosDaSessao = HttpContext.DadosDaSessao();

                Expression<Func<VinculoDeTrabalho, bool>> filtro = 
                    c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId &&
                    c.Lotacoes.Any(d => d.UnidadeOrganizacionalId == escala.UnidadeOrganizacionalId);

                if (dadosDaSessao.UnidadeOrganizacionais.Any())
                    filtro = ConcatenadorDeExpressao.Concatenar(filtro,
                        c => c.Lotacoes.Any(d => dadosDaSessao.UnidadeOrganizacionais.Contains(d.UnidadeOrganizacionalId)));

                ViewBag.Servidores = new SelectList(
                    _servicoVinculoDeTrabalho
                        .ObtenhaLista(filtro)
                        .OrderBy(c => c.Servidor.Nome)
                        .Select(c => new CodigoDescricaoDTO(c.Id, $"[{c.Matricula}] {c.Servidor.Nome}")),
                    "Codigo",
                    "Descricao");
            }

            var html = await RenderizarComoString("_ModalEscalaServidorCiclo", servidorEscala);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public ActionResult SalvarServidorCiclo(EscalaDoServidor escalaDoServidor)
        {
            try
            {
                escalaDoServidor.OrganizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
                _servico.AdicioneOuAltereEscalaDoServidor(escalaDoServidor, true);
                return Json(new { sucesso = true, mensagem = "Os dados foram salvos." });
            }
            catch (Exception ex)
            {
                var duplicateEntryText = "duplicate entry";
                if (ex.Message.ToLower().Contains(duplicateEntryText) || (ex.InnerException != null && ex.InnerException.Message.ToLower().Contains(duplicateEntryText)))
                {
                    return Json(new { sucesso = false, mensagem = "Já existe um registro para esse servidor nesse dia e nessa escala." });
                }

                throw ex;
            }
        }

        [HttpPatch]
        public ActionResult RemoverServidorCiclo(int id)
        {
            _servico.RemoverEscalaServidor(id);
            return Json(new { sucesso = true, mensagem = "O servidor foi removido." });
        }

        [HttpGet]
        public async Task<ActionResult> ModalServidorMensal(int id, int escalaId)
        {
            var escalaServidorMensal = id > 0 ?
                _servico.ObtenhaEscalaDoServidor(id) :
                new() { CicloDaEscala = new(), Data = DateTime.Today };

            if (id == 0)
            {
                var escala = _servico.Obtenha(escalaId);
                var dadosDaSessao = HttpContext.DadosDaSessao();

                Expression<Func<VinculoDeTrabalho, bool>> filtro =
                    c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId &&
                    c.Lotacoes.Any(d => d.UnidadeOrganizacionalId == escala.UnidadeOrganizacionalId);

                if (dadosDaSessao.UnidadeOrganizacionais.Any())
                    filtro = ConcatenadorDeExpressao.Concatenar(filtro,
                        c => c.Lotacoes.Any(d => dadosDaSessao.UnidadeOrganizacionais.Contains(d.UnidadeOrganizacionalId)));

                ViewBag.Servidores = new SelectList(
                    _servicoVinculoDeTrabalho
                        .ObtenhaLista(filtro)
                        .OrderBy(c => c.Servidor.Nome)
                        .Select(c => new CodigoDescricaoDTO(c.Id, $"[{c.Matricula}] {c.Servidor.Nome}")),
                    "Codigo",
                    "Descricao");
            }

            var html = await RenderizarComoString("_ModalServidorMensal", escalaServidorMensal);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public ActionResult SalvarEscalaServidorMensal(EscalaDoServidor escalaDoServidor)
        {
            try
            {
                escalaDoServidor.OrganizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
                escalaDoServidor.CicloDaEscala.OrganizacaoId = escalaDoServidor.OrganizacaoId;
                escalaDoServidor.CicloDaEscala.EscalaId = escalaDoServidor.EscalaId;

                _servico.AdicioneOuAltereEscalaDoServidor(escalaDoServidor, false);
                return Json(new { sucesso = true, mensagem = "Os dados foram salvos." });
            }
            catch (Exception ex)
            {
                var duplicateEntryText = "duplicate entry";
                if (ex.Message.ToLower().Contains(duplicateEntryText) || (ex.InnerException != null && ex.InnerException.Message.ToLower().Contains(duplicateEntryText)))
                {
                    return Json(new { sucesso = false, mensagem = "Já existe um registro para esse servidor nesse dia e nessa escala." });
                }

                throw ex;
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Escala> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            var dadosDaSessao = HttpContext.DadosDaSessao();
            parametros.OrganizacaoId = dadosDaSessao.OrganizacaoId;

            Expression<Func<Escala, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            if (dadosDaSessao.UnidadeOrganizacionais.Any())
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => dadosDaSessao.UnidadeOrganizacionais.Contains(c.UnidadeOrganizacionalId));
            }

            var dados = _servico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}