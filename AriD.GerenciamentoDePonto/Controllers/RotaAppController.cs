using AriD.BibliotecaDeClasses.DTO;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    [Route("api/rota-app")]
    [ApiController]
    public class RotaAppController : Controller
    {
        private readonly IServicoDeAplicativo _servicoDeAplicativo;

        public RotaAppController(IServicoDeAplicativo servicoDeAplicativo)
        {
            _servicoDeAplicativo = servicoDeAplicativo;
        }

        [HttpPost("autentique")]
        public IActionResult Autentique([FromBody] CredenciaisDTO credenciais)
        {
            try
            {
                var acesso = _servicoDeAplicativo.AutenticarUsuario(credenciais);
                if (acesso == null)
                    throw new ApplicationException("Usuário ou senha incorretos.");

                var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "organizacao", $"{acesso.OrganizacaoId}", $"{acesso.ServidorId}.png");

                FileStream imageFileStream;

                if (!Path.Exists(caminhoArquivo))
                    imageFileStream = System.IO.File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "sem-foto.png"));
                else
                    imageFileStream = System.IO.File.OpenRead(caminhoArquivo);

                acesso.FotoBase64 = Convert.ToBase64String(ObterBytesDeFileStream(imageFileStream));

                return Ok(acesso);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private byte[] ObterBytesDeFileStream(FileStream fileStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}