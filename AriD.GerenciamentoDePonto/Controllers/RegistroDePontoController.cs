using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RegistroDePontoController : BaseController
    {
        private readonly IServicoRegistroDePonto _servico;

        public RegistroDePontoController(
            IServicoRegistroDePonto servico)
        {
            _servico = servico;
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

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<RegistroDePontoIndexDTO> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosDeConsultaRegistroDePonto>(listaPaginada.Adicional);

            var dadosDaSessao = this.HttpContext.DadosDaSessao();
            parametros.OrganizacaoId = dadosDaSessao.OrganizacaoId;
            parametros.Unidades = dadosDaSessao.UnidadeOrganizacionais;
            parametros.Pesquisa = listaPaginada.TermoDeBusca;
            parametros.TotalPorPagina = listaPaginada.QuantidadeDeItensPorPagina;
            parametros.Pagina = listaPaginada.Pagina;

            var dados = _servico.ObtenhaListaPaginadaDTO(parametros);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}