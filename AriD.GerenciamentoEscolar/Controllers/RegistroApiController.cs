using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.GerenciamentoEscolar.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AriD.GerenciamentoEscolar.Controllers
{
    [Route("api/registro")]
    [ApiController]
    public class RegistroApiController : BaseController
    {
        private readonly IServicoRegistroDePonto _servico;

        public RegistroApiController(
            IServicoRegistroDePonto servico)
        {
            _servico = servico;
        }

        [HttpPost("registro-equipamento")]
        public async Task<IActionResult> ReceberRegistro(
            [FromBody] RegistroEquipamentoDTO dados)
        {
            try
            {
                string userAgent = 
                    HttpContext.Request.Headers?.UserAgent.FirstOrDefault();

                if (string.IsNullOrEmpty(userAgent) || !userAgent.Equals("AIFaceEVO.API-ARID.TECNOLOGIA"))
                {
                    return StatusCode(403);
                }

                await _servico.ReceberRegistroDeEquipamento(dados);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
