using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class MotivoDeDemissaoController : BaseController
    {
        private readonly IServico<MotivoDeDemissao> _motivoDeDemissaoServico;
        private readonly IServico<Organizacao> _servicoOrganizacao;

        public MotivoDeDemissaoController(
            IServico<MotivoDeDemissao> motivoDeDemissaoServico, 
            IServico<Organizacao> servicoOrganizacao)
        {
            _motivoDeDemissaoServico = motivoDeDemissaoServico;
            _servicoOrganizacao = servicoOrganizacao;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<MotivoDeDemissao> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<MotivoDeDemissao> listaPaginada)
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
        public async Task<IActionResult> Modal(int motivoId)
        {
            var model = motivoId == 0 ?
                    new MotivoDeDemissao { Ativo = true } :
                    _motivoDeDemissaoServico.Obtenha(motivoId);

            var html = await RenderizarComoString("_Modal", model);

            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public IActionResult Salvar(MotivoDeDemissao motivoDeDemissao)
        {
            int id = motivoDeDemissao.Id;
            motivoDeDemissao.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (motivoDeDemissao.Id == 0)
                id = _motivoDeDemissaoServico.Adicionar(motivoDeDemissao);
            else
                _motivoDeDemissaoServico.Atualizar(motivoDeDemissao);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public IActionResult Remova(int motivoId)
        {
            var item = _motivoDeDemissaoServico.Obtenha(motivoId);
            _motivoDeDemissaoServico.Remover(item);

            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<MotivoDeDemissao> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<MotivoDeDemissao, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Sigla.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _motivoDeDemissaoServico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}