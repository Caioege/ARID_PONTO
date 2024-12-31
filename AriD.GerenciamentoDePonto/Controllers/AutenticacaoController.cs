using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class AutenticacaoController : BaseController
    {
        public AutenticacaoController() { }

        [HttpGet]
        public IActionResult Index()
        {
            if (this.EstaAutenticado())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Entrar([FromBody] CredenciaisDTO credenciais)
        {
            try
            {
                this.Autenticar(new SessaoDTO(
                    0,
                    credenciais.Usuario,
                    credenciais.Usuario == "admin" ? ePerfilDeAcesso.AdministradorDeSistema :
                        credenciais.Usuario == "unidade" ? 
                        ePerfilDeAcesso.UnidadeOrganizacional :
                        ePerfilDeAcesso.Organizacao, 
                    1, 
                    "PREFEITURA DE TESTE",
                    new()));

                return Json(new { sucesso = true, mensagem = "O acesso foi feito com sucesso." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Sair()
        {
            this.HttpContext?.Session?.Clear();
            return RedirectToAction("Index");
        }
    }
}