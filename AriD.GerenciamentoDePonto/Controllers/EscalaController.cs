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
    public class EscalaController : BaseController
    {
        private readonly IServicoDeEscala _servico;

        public EscalaController(
            IServicoDeEscala servico)
        {
            _servico = servico;
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

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Escala> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<Escala, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _servico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}