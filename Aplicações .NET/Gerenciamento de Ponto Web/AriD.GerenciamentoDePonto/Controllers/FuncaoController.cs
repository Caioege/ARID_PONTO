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
    public class FuncaoController : BaseController
    {
        private readonly IServico<Funcao> _funcaoServico;
        private readonly IServico<Organizacao> _servicoOrganizacao;

        public FuncaoController(
            IServico<Funcao> funcaoServico, 
            IServico<Organizacao> servicoOrganizacao)
        {
            _funcaoServico = funcaoServico;
            _servicoOrganizacao = servicoOrganizacao;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Funcao> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<Funcao> listaPaginada)
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
        public async Task<IActionResult> Modal(int funcaoId)
        {
            var model = funcaoId == 0 ?
                    new Funcao { Ativa = true } :
                    _funcaoServico.Obtenha(funcaoId);

            var html = await RenderizarComoString("_Modal", model);

            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public IActionResult Salvar(Funcao funcao)
        {
            int id = funcao.Id;
            funcao.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (funcao.Id == 0)
                id = _funcaoServico.Adicionar(funcao);
            else
                _funcaoServico.Atualizar(funcao);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public IActionResult Remova(int funcaoId)
        {
            var funcao = _funcaoServico.Obtenha(funcaoId);
            _funcaoServico.Remover(funcao);

            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Funcao> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<Funcao, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Sigla.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.CodigoCBO.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _funcaoServico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}