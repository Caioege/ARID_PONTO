using AriD.BibliotecaDeClasses.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class CepController : Controller
    {
        public CepController() { }

        [HttpGet]
        public async Task<IActionResult> ConsulteCEP(string cep)
        {
            if (string.IsNullOrEmpty(cep))
                throw new ArgumentNullException();

            EnderecoDTO endereco = null;
            using (var client = new HttpClient())
            {
                var responseMessage = await client.GetAsync($"https://viacep.com.br/ws/{cep.Replace("-", string.Empty)}/json/");
                if (responseMessage.IsSuccessStatusCode)
                    endereco = JsonConvert.DeserializeObject<EnderecoDTO>(await responseMessage.Content.ReadAsStringAsync());
            }

            return Json(new { sucesso = true, dados = endereco });
        }
    }
}