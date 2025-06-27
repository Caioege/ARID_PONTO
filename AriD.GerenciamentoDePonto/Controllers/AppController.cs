using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : Controller
    {
        private readonly IServicoDeAplicativo _servicoDeAplicativo;
        private readonly IServicoDeFolhaDePonto _servicoDeFolhaDePonto;
        private readonly IServico<VinculoDeTrabalho> _servicoVinculoDeTrabalho;

        public AppController(IServicoDeAplicativo servicoDeAplicativo, 
            IServicoDeFolhaDePonto servicoDeFolhaDePonto, 
            IServico<VinculoDeTrabalho> servicoVinculoDeTrabalho)
        {
            _servicoDeAplicativo = servicoDeAplicativo;
            _servicoDeFolhaDePonto = servicoDeFolhaDePonto;
            _servicoVinculoDeTrabalho = servicoVinculoDeTrabalho;
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

                acesso.FotoBase64 = Convert.ToBase64String(
                    ObterBytesDeFileStream(imageFileStream));

                return Ok(acesso);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("horarios-trabalho/{servidorId}")]
        public IActionResult ObtenhaHorariosDeTrabalho(int servidorId)
        {
            try
            {
                return Ok(_servicoDeAplicativo.ObtenhaHorariosDoServidor(servidorId));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("eventos/{organizacaoId}")]
        public IActionResult ObtenhaEventos(int organizacaoId)
        {
            try
            {
                return Ok(_servicoDeAplicativo.ObtenhaListaDeEventos(organizacaoId));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vinculos/{servidorId}")]
        public IActionResult ObtenhaVinculos(int servidorId)
        {
            try
            {
                return Ok(_servicoDeAplicativo.ObtenhaListaDeVinculos(servidorId));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("unidade/{vinculoId}")]
        public IActionResult ObtenhaUnidades(int vinculoId)
        {
            try
            {
                return Ok(_servicoDeAplicativo.ObtenhaListaDeLotacoes(vinculoId));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("justificativas/{organizacaoId}")]
        public IActionResult ObtenhaJustificativas([FromRoute] int organizacaoId)
        {
            try
            {
                return Ok(_servicoDeAplicativo.ObtenhaListaDeJustificativas(organizacaoId));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("folha-ponto/{vinculoId}/{unidadeId}/{mesDeReferencia}")]
        public IActionResult FolhaDePonto(
            int vinculoId, 
            int unidadeId, 
            string mesDeReferencia)
        {
            var mesAno = new MesAno(mesDeReferencia);

            if (mesAno.Inicio.Date > DateTime.Today)
                throw new ApplicationException("O período não pode ser maior que a data atual.");

            var vinculoDeTrabalho = _servicoVinculoDeTrabalho.Obtenha(vinculoId);

            var organizacaoId = vinculoDeTrabalho.OrganizacaoId;

            var eventos = _servicoDeFolhaDePonto
                .EventosDaFolhaDePonto(organizacaoId, mesAno.Inicio, mesAno.Fim);

            var listaDePonto = _servicoDeFolhaDePonto.CarregueFolhaDePonto(
                organizacaoId,
                vinculoId,
                unidadeId,
            mesAno);

            var relatorio = FolhaDePontoController.RelatorioFolhaDePonto(
                new() { OrganizacaoId = organizacaoId },
                vinculoDeTrabalho,
                mesAno,
                eventos,
                listaDePonto,
                true);

            return File(relatorio, "application/pdf");
        }

        [HttpGet("ultimos-registros-servidor/{servidorId}")]
        public IActionResult ObtenhaUltimosRegistrosDoServidor([FromRoute] int servidorId)
        {
            try
            {
                return Ok(_servicoDeAplicativo.ObtenhaUltimosRegistrosDoServidor(servidorId));
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException)
                    return BadRequest(ex.Message);

                return StatusCode(500, "Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }

        [HttpPost("receptar-ponto")]
        [Consumes("multipart/form-data")]
        public IActionResult ReceptarRegistro([FromForm] PostRegistroDePontoDTO registro)
        {
            try
            {
                _servicoDeAplicativo.ReceptarRegistro(registro);
                return Ok();
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException)
                    return BadRequest(ex.Message);

                return StatusCode(500, "Ocorreu um erro inesperado. Tente novamente mais tarde.");
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