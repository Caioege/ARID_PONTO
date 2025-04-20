using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.GerenciamentoEscolar.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class RegistroDePontoController : BaseController
    {
        private readonly IServicoRegistroDePonto _servico;
        private readonly IServico<Escola> _servicoescola;

        public RegistroDePontoController(
            IServicoRegistroDePonto servico, IServico<Escola> servicoescola)
        {
            _servico = servico;
            _servicoescola = servicoescola;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<RegistroDePontoIndexDTO> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<RegistroDePontoIndexDTO> listaPaginada)
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
        public async Task<IActionResult> AbrirModalExportarCSV()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            int redeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;

            if (dadosDaSessao.Perfil != ePerfilDeAcesso.Escola)
            {
                ViewBag.Escolas = new SelectList(
                    _servicoescola
                        .ObtenhaLista(c => c.RedeDeEnsinoId == redeDeEnsinoId)
                        .OrderBy(c => c.Nome),
                    "Id", "Nome");
            }
            else
            {
                ViewBag.EscolaNome = _servicoescola.Obtenha(dadosDaSessao.EscolaId.Value).Nome;
            }

            var html = await RenderizarComoString("_ModalExportarCSV", null);
            return Json(new { sucesso = true, html });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<RegistroDePontoIndexDTO> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosDeConsultaRegistroDePonto>(listaPaginada.Adicional);

            var dadosDaSessao = this.HttpContext.DadosDaSessao();
            parametros.RedeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;
            parametros.EscolaId = dadosDaSessao.EscolaId;
            parametros.Pesquisa = listaPaginada.TermoDeBusca;
            parametros.TotalPorPagina = listaPaginada.QuantidadeDeItensPorPagina;
            parametros.Pagina = listaPaginada.Pagina;

            var dados = _servico.ObtenhaListaPaginadaDTO(parametros);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}
