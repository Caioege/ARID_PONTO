using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class PoliticaDePrivacidadeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}