using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RegistroDePontoController : BaseController
    {
        private readonly IServicoRegistroDePonto _servico;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;

        public RegistroDePontoController(
            IServicoRegistroDePonto servico,
            IServico<UnidadeOrganizacional> servicoUnidade)
        {
            _servico = servico;
            _servicoUnidade = servicoUnidade;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<RegistroDePontoIndexDTO> listaPaginada)
        {
            try
            {
                var dadosDaSessao = this.DadosDaSessao();
                int organizacaoId = dadosDaSessao.OrganizacaoId;

                ViewBag.Unidades = new SelectList(
                    _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == organizacaoId)
                    .OrderBy(c => c.Nome),
                    "Id", "Nome");

                ConfigureDadosDaTabelaPaginada(listaPaginada, true);
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

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<RegistroDePontoIndexDTO> listaPaginada, bool requisicaoInicial = false)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosDeConsultaRegistroDePonto>(listaPaginada.Adicional);

            var dadosDaSessao = this.HttpContext.DadosDaSessao();
            parametros.OrganizacaoId = dadosDaSessao.OrganizacaoId;
            parametros.Unidades = dadosDaSessao.UnidadeOrganizacionais;
            parametros.Pesquisa = listaPaginada.TermoDeBusca;
            parametros.TotalPorPagina = listaPaginada.QuantidadeDeItensPorPagina;
            parametros.Pagina = listaPaginada.Pagina;

            if (requisicaoInicial)
            {
                parametros.DataInicio = DateTime.Today;
                parametros.DataFim = DateTime.Today;
            }
            else
            {
                if (parametros.DataInicio.HasValue) parametros.DataInicio = parametros.DataInicio.Value.Date;
                if (parametros.DataFim.HasValue) parametros.DataFim = parametros.DataFim.Value.Date;
            }

            var dados = _servico.ObtenhaListaPaginadaDTO(parametros);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}