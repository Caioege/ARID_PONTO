using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IServico<Escola> _servicoEscola;

        public DashboardController(
            IServico<Escola> servicoEscola)
        {
            _servicoEscola = servicoEscola;
        }


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();

                if (dadosDaSessao.Perfil != ePerfilDeAcesso.Escola)
                    ViewBag.Escolas = new SelectList(
                        _servicoEscola
                            .ObtenhaLista(c => c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId)
                            .OrderBy(c => c.Nome),
                        "Id", "Nome");

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }
    }
}