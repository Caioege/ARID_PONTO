using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    [Route("api/rota-app")]
    [ApiController]
    public class RotaAppController : Controller
    {
        private readonly IServicoDeAplicativo _servicoDeAplicativo;
        private readonly IServico<Rota> _servicoRota;
        private readonly IServico<LocalizacaoRota> _servicoLocalizacao;

        public RotaAppController(
            IServicoDeAplicativo servicoDeAplicativo,
            IServico<Rota> servicoRota,
            IServico<LocalizacaoRota> servicoLocalizacao)
        {
            _servicoDeAplicativo = servicoDeAplicativo;
            _servicoRota = servicoRota;
            _servicoLocalizacao = servicoLocalizacao;
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

        [HttpPost("receber-localizacao")]
        public IActionResult ReceberLocalizacao([FromBody] PostLocalizacaoRotaDTO dto)
        {
            try
            {
                var rota = _servicoRota.Obtenha(dto.RotaId);
                if (rota == null)
                    return BadRequest(new { message = "Rota não encontrada." });

                var localizacao = new LocalizacaoRota
                {
                    RotaId = dto.RotaId,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    DataHora = dto.DataHora,
                    OrganizacaoId = rota.OrganizacaoId
                };

                _servicoLocalizacao.Adicionar(localizacao);
                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
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