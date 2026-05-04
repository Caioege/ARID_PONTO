using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;
        private readonly IServicoDeDashboard _servicoDeDashboard;

        public DashboardController(
            IServico<UnidadeOrganizacional> servicoUnidade, 
            IServicoDeDashboard servicoDeDashboard)
        {
            _servicoUnidade = servicoUnidade;
            _servicoDeDashboard = servicoDeDashboard;
        }


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();

                if (dadosDaSessao.Perfil != ePerfilDeAcesso.UnidadeOrganizacional)
                    ViewBag.Unidades = new SelectList(
                        _servicoUnidade
                            .ObtenhaLista(c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId)
                            .OrderBy(c => c.Nome),
                        "Id", "Nome");

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult CarregarDados(int? unidadeId)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeId = dadosDaSessao.UnidadeOrganizacionais.First();

            var dados = _servicoDeDashboard.ObtenhaDashboardDTO(dadosDaSessao.OrganizacaoId, unidadeId);

            return Json(new { sucesso = true, dados });
        }
    }
}