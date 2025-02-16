using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RegistroController : BaseController
    {
        private readonly IServicoRegistroDePonto _servico;

        public RegistroController(
            IServicoRegistroDePonto servico)
        {
            _servico = servico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<RegistroDePonto> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<RegistroDePonto> listaPaginada)
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

        [HttpPost("registro-equipamento")]
        public async Task<IActionResult> ReceberRegistro(
            [FromBody] RegistroEquipamentoDTO dados)
        {
            try
            {
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<RegistroDePonto> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            var dadosDaSessao = this.HttpContext.DadosDaSessao();
            parametros.OrganizacaoId = dadosDaSessao.OrganizacaoId;

            Expression<Func<RegistroDePonto, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.UsuarioEquipamentoId.Contains(listaPaginada.TermoDeBusca) ||
                    c.EquipamentoDePonto.UnidadeOrganizacional.Nome.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
            {
                filtro = ConcatenadorDeExpressao.Concatenar(filtro,
                    c => dadosDaSessao.UnidadeOrganizacionais.Contains(c.EquipamentoDePonto.UnidadeOrganizacionalId));
            }

            var dados = _servico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}