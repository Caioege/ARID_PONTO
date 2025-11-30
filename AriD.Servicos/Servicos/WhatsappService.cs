using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Extensao;
using AriD.Servicos.Helpers;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AriD.Servicos.Servicos
{
    public class WhatsappService : IWhatsappService
    {
        private readonly HttpClient _httpClient;
        private readonly string _instanceUrl = "https://api.z-api.io/instances/3EAFB64698AC2118A5D816EAB20D013B/token/A24BA67F636D60D06E1A8DD3";
        private readonly string _token = "Fb38308df844b431f85bcfae00a8be788S";

        public WhatsappService(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task SendMessageAsync(string phone, string message)
        {
            var payload = new
            {
                phone = phone,
                message = message
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Client-Token", _token);

            var response = await _httpClient.PostAsync($"{_instanceUrl.TrimEnd('/')}/send-text", content);
            var bodyResponse = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }

        public async Task EnviarComprovantePontoAsync(
            Servidor servidor,
            int nsr,
            DateTime dataHora)
        {
            if (servidor == null)
                throw new ApplicationException("Servidor não encontrado.");

            if (!servidor.Organizacao.EnvioDeMensagemWhatsAppExperimental)
                throw new ApplicationException("O envio de comprovante está desabilitado para essa organização.");

            if (string.IsNullOrEmpty(servidor.TelefoneDeContato))
                throw new ApplicationException("O telefone de contato do servidor não foi informado.");

            var sbMensagem = new StringBuilder();
            sbMensagem.Append("*COMPROVANTE DE PONTO*\n\n");
            sbMensagem.AppendLine($"Olá, *{servidor.Nome.ToUpper()}*!");
            sbMensagem.AppendLine($"Recebemos seu registro de ponto.\n");
            sbMensagem.AppendLine($"*Data:* {dataHora.Date.ToString("dd/MM/yyyy")}");
            sbMensagem.AppendLine($"*Hora:* {dataHora.TimeOfDay.ToString(@"hh\:mm")}\n");
            sbMensagem.AppendLine($"O comprovante oficial está anexo a esta mensagem.\n");
            sbMensagem.AppendLine($"*{servidor.Organizacao.Nome}*");
            
            string endpoint = $"{_instanceUrl}/send-document/pdf";

            var payload = new
            {
                phone = ObterSomenteNumeros(servidor.TelefoneDeContato, null),
                document = ComprovantePdfHelper.GerarComprovantePortaria671(
                    servidor.OrganizacaoId,
                    servidor.Organizacao.Nome,
                    string.Empty,
                    servidor.Nome,
                    servidor.Pessoa.Cpf,
                    nsr,
                    dataHora),
                caption = sbMensagem.ToString(),
                fileName = $"Comprovante_Ponto__{dataHora.ToString("dd-MM-yyyy")}_{dataHora.TimeOfDay.ToString(@"hh\-mm")}"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Client-Token", _token);

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
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