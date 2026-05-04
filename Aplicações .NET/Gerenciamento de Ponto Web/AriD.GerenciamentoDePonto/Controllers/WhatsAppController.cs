using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class WhatsAppController : Controller
    {
        private readonly IWhatsappService _whatsappService;
        private readonly IEmailService _emailService;
        private readonly IServico<Servidor> _servicoServidor;

        public WhatsAppController(
            IWhatsappService whatsappService,
            IServico<Servidor> servicoServidor,
            IEmailService emailService)
        {
            _whatsappService = whatsappService;
            _servicoServidor = servicoServidor;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensagemTeste(string numero)
        {
            if (HttpContext.DadosDaSessao().Perfil != ePerfilDeAcesso.AdministradorDeSistema)
                throw new ApplicationException("Sem permissão.");

            var somenteNumeros = ObterSomenteNumeros(numero, null);
            if (string.IsNullOrEmpty(somenteNumeros))
                throw new ApplicationException("Número inválido.");

           await _whatsappService.SendMessageAsync($"+55{somenteNumeros}", $"Recebemos seu registro de ponto: {DateTime.Now.TimeOfDay.ToString(@"hh\:mm")}");
            return Json(new { sucesso = true, mensagem = "Mensagem enviada com sucesso." });
        }

        [HttpPost]
        public async Task<IActionResult> EnviarComprovanteDePonto(
            int servidorId, 
            DateTime dataHora)
        {
            servidorId = 10;
            dataHora = DateTime.Now;

            var servidor = _servicoServidor.Obtenha(servidorId);
            await _whatsappService.EnviarComprovantePontoAsync(servidor, 1000, dataHora);
            await _emailService.EnviarComprovantePontoAsync(servidor, 1000, dataHora);
            return Json(new { sucesso = true, mensagem = "Mensagem enviada com sucesso." });
        }

        static string ObterSomenteNumeros(string texto, string returnIfNull)
        {
            if (string.IsNullOrEmpty(texto))
                return returnIfNull;

            var retorno = new string(texto.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(retorno))
                return returnIfNull;

            return retorno;
        }
    }
}