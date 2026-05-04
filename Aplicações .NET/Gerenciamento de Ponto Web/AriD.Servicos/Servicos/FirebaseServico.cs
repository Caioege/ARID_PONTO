using AriD.Servicos.Servicos.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace AriD.Servicos.Servicos
{
    public class FirebaseServico : IServicoNotificacao
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public FirebaseServico(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> EnviarNotificacaoPush(string pushToken, string titulo, string mensagem, object dados = null)
        {
            if (string.IsNullOrEmpty(pushToken)) return false;
            return await EnviarNotificacaoPush(new List<string> { pushToken }, titulo, mensagem, dados);
        }

        public async Task<bool> EnviarNotificacaoPush(List<string> pushTokens, string titulo, string mensagem, object dados = null)
        {
            try
            {
                var serverKey = _configuration["Firebase:ServerKey"];
                var senderId = _configuration["Firebase:SenderId"];

                if (string.IsNullOrEmpty(serverKey) || serverKey == "SUA_CHAVE_AQUI")
                {
                    // Log or handle missing configuration
                    return false;
                }

                using var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={serverKey}");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Project_id", senderId);

                var payload = new
                {
                    registration_ids = pushTokens,
                    notification = new
                    {
                        title = titulo,
                        body = mensagem,
                        sound = "default"
                    },
                    data = dados
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://fcm.googleapis.com/fcm/send", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Handle exceptions
                return false;
            }
        }
    }
}
