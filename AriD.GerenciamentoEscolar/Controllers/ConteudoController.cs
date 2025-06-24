using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class ConteudoController : Controller
    {
        
        [HttpGet]
        public IActionResult Index()
        {
            // Dados fictícios simulando uma listagem
            var lista = new List<dynamic>
            {
                new { Id = 1, Titulo = "Frações", Disciplina = "Matemática", Data = "10/06/2025" },
                new { Id = 2, Titulo = "Fotossíntese", Disciplina = "Ciências", Data = "15/06/2025" },
                new { Id = 3, Titulo = "Brasil Colônia", Disciplina = "História", Data = "20/06/2025" }
            };

            return View(lista);
        }
        [HttpGet]
        public IActionResult Adicionar()
        {
            return View();
        }


    }
}