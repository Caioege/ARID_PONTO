using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class ConfiguracoesController : Controller
    {
        public IActionResult ConteudoAplicado()
        {
            return View();
        }
    }
}
