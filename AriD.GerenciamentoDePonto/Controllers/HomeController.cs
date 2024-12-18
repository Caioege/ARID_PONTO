using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
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
