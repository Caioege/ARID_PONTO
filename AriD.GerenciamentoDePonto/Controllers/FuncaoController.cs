using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
            try
            {
                var model = funcaoId == 0 ?
                    new Funcao { Ativa = true } :
                    _funcaoServico.Obtenha(funcaoId);

                var html = await RenderizarComoString("_Modal", model);

                return Json(new { sucesso = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Salvar(Funcao funcao)
        {
            try
            {
                int id = funcao.Id;
                funcao.OrganizacaoId = 1;

                if (funcao.Id == 0)
                    id = _funcaoServico.Adicionar(funcao);
                else
                    _funcaoServico.Atualizar(funcao);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = true, mensagem = "Ocorreu um erro." });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Funcao> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = 1;

            var dados = _funcaoServico.ObtenhaListaPaginada(c => c.OrganizacaoId == parametros.OrganizacaoId, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total);
        }
    }
}