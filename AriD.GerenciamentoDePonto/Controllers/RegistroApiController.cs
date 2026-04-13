using AriD.BibliotecaDeClasses.DTO;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    /// <summary>
    /// Controller responsável por recepcionar registros de ponto vindos de equipamentos externos (Terminais de Reconhecimento Facial).
    /// </summary>
    /// <remarks>
    /// <b>Arquitetura e Decisão Técnica:</b>
    /// Esta API foi implementada dentro do projeto ASP.NET MVC principal a pedido do proprietário da aplicação.
    /// O objetivo inicial é reduzir custos de hospedagem e manter uma infraestrutura enxuta durante a fase de lançamento,
    /// evitando a necessidade de gerenciar múltiplas instâncias de hospedagem para gateways de integração.
    /// 
    /// <b>Evolução Futura:</b>
    /// Existe um planejamento para que estes endpoints de integração com hardware sejam movidos para um serviço 
    /// de gateway de borda (edge gateway) ou uma API de integração dedicada, isolando o tráfego de dispositivos 
    /// do tráfego de usuários web.
    /// 
    /// <b>Comunicação:</b>
    /// Este controller comunica-se com equipamentos de reconhecimento facial e sistemas de hardware (ex: AIFaceEVO),
    /// validando a origem via User-Agent específico e persistindo logs de presença e monitoramento de conectividade.
    /// </remarks>
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