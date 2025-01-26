using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RelatorioController : BaseController
    {
        private IServicoDeRelatorios _servicoDeRelatorios;

        public RelatorioController(
            IServicoDeRelatorios servicoDeRelatorios)
        {
            _servicoDeRelatorios = servicoDeRelatorios;
        }

        #region Views

        [HttpGet]
        public IActionResult ServidoresComAfastamento()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarServidoresComAfastamento()
        {
            try
            {
                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ServidoresPorEscala()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarServidoresPorEscala()
        {
            try
            {
                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ServidoresPorHorarioDeTrabalho()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarServidoresPorHorarioDeTrabalho()
        {
            try
            {
                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        #endregion
    }
}