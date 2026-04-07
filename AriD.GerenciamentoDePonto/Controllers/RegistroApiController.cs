using AriD.BibliotecaDeClasses.DTO;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
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

        [HttpPost("monitoramento-conectividade")]
        public async Task<IActionResult> MonitorarConectividade(
            [FromBody] List<MonitoramentoConectividadeDTO> dados)
        {
            try
            {
                string userAgent =
                    HttpContext.Request.Headers?.UserAgent.FirstOrDefault();

                if (string.IsNullOrEmpty(userAgent) || !userAgent.Equals("AIFaceEVO.API-ARID.TECNOLOGIA"))
                {
                    return StatusCode(403);
                }

                await _servico.ProcessarMonitoramentoConectividade(dados);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}