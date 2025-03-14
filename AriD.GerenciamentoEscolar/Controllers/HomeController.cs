using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
