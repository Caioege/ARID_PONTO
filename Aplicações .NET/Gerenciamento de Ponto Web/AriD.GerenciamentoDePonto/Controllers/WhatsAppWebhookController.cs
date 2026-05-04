using AriD.Servicos.Servicos;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AriD.GerenciamentoDePonto.Controllers
{
    [ApiController]
    [Route("api/whatsapp-webhook")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly WhatsappService _whatsappService;

        public WhatsAppWebhookController(WhatsappService whatsappService)
        {
            _whatsappService = whatsappService;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] JsonElement body)
        {
            try
            {
                // A estrutura do JSON depende da API escolhida (Z-API, Evolution, etc)
                // Este é um exemplo genérico de extração
                if (body.TryGetProperty("message", out var messageData))
                {
                    string text = messageData.GetProperty("text").GetString()?.ToLower();
                    string sender = messageData.GetProperty("sender").GetString(); // Número do usuário

                    if (text.Contains("ponto") || text.Contains("relatorio"))
                    {
                        var pontos = ObterPontosDoDia(sender);
                        await _whatsappService.SendMessageAsync(sender, pontos);
                    }
                    else
                    {
                        await _whatsappService.SendMessageAsync(sender, "Olá! Digite 'ponto' para ver seus registros de hoje.");
                    }
                }

                return Ok();
            }
            catch
            {
                return Ok();
            }
        }

        private string ObterPontosDoDia(string telefone)
        {
            // Lógica de banco de dados SQL aqui
            return "Seus registros hoje: 08:00, 12:00, 13:00";
        }
    }
}