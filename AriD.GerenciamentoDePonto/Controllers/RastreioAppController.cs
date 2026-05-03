using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AriD.GerenciamentoDePonto.Controllers
{
    [Route("api/rastreio-app")]
    [ApiController]
    public class RastreioAppController : Controller
    {
        private readonly IServicoDeAplicativoDeRastreio _servicoDeRastreio;
        private readonly IMemoryCache _memoryCache;

        public RastreioAppController(
            IServicoDeAplicativoDeRastreio servicoDeRastreio,
            IMemoryCache memoryCache)
        {
            _servicoDeRastreio = servicoDeRastreio;
            _memoryCache = memoryCache;
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpPost("autentique")]
        public IActionResult Autentique([FromBody] CredenciaisDTO credenciais)
        {
            var acesso = _servicoDeRastreio.AutenticarUsuario(credenciais);
            if (acesso == null)
                throw new ApplicationException("Usuario ou senha incorretos.");

            var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "organizacao", $"{acesso.OrganizacaoId}", $"{acesso.ServidorId}.png");

            if (!Path.Exists(caminhoArquivo))
                caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "sem-foto.png");

            var bytes = System.IO.File.ReadAllBytes(caminhoArquivo);
            acesso.FotoBase64 = Convert.ToBase64String(bytes);

            return Ok(new
            {
                token = acesso.ServidorId.ToString(),
                usuario = new
                {
                    id = acesso.ServidorId,
                    nome = acesso.ServidorNome,
                    login = credenciais.Usuario,
                    foto = acesso.FotoBase64,
                    tipoAcesso = credenciais.TipoAcesso,
                    cpf = acesso.Cpf,
                    dataNascimento = acesso.DataDeNascimento?.ToString("yyyy-MM-dd"),
                    email = acesso.Email,
                    numeroCnh = acesso.NumeroCNH,
                    categoriaCnh = acesso.CategoriaCNH?.ToString(),
                    emissaoCnh = acesso.EmissaoCNH?.ToString("yyyy-MM-dd"),
                    validadeCnh = acesso.ValidadeCNH?.ToString("yyyy-MM-dd")
                }
            });
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpGet("conectividade")]
        public IActionResult Conectividade()
        {
            //return StatusCode(500);

            return Ok(new
            {
                sucesso = true,
                dataHoraServidor = DateTime.Now
            });
        }

        [HttpPost("registrar-token")]
        public IActionResult RegistrarToken([FromBody] RegistrarTokenDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int servidorId = ObterServidorId(auth);
            if (servidorId <= 0) return Unauthorized();

            _servicoDeRastreio.RegistrarToken(servidorId, dto.Token, dto.Plataforma);

            return Ok(new { sucesso = true });
        }

        [HttpGet("rotas")]
        public IActionResult Rotas([FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            var resultado = _servicoDeRastreio.ObterRotasMotorista(motoristaId);
            return Ok(new { data = resultado });
        }

        [HttpGet("offline/pacote")]
        public IActionResult PacoteOffline([FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            var resultado = _servicoDeRastreio.ObterPacoteOfflineMotorista(motoristaId);
            return Ok(resultado);
        }

        [HttpPost("offline/sincronizar")]
        public IActionResult SincronizarOffline([FromBody] SincronizarRotaOfflineDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            var resultado = _servicoDeRastreio.SincronizarRotaOffline(dto, motoristaId);
            return Ok(resultado);
        }

        [HttpGet("rotas-acompanhante")]
        public IActionResult RotasAcompanhante([FromHeader(Name = "Authorization")] string auth)
        {
            int servidorId = ObterServidorId(auth);
            if (servidorId == 0) return Unauthorized();

            var resultado = _servicoDeRastreio.ObterRotasAcompanhante(servidorId);
            return Ok(new { data = resultado });
        }

        [HttpGet("rotas/ultima-localizacao")]
        public IActionResult UltimaLocalizacao(int rotaId)
        {
            var ultima = _servicoDeRastreio.ObterUltimaLocalizacao(rotaId);
            if (ultima == null) return NotFound(new { message = "Nenhuma localizacao encontrada." });

            return Ok(ultima);
        }

        [HttpGet("rotas/trajeto")]
        public IActionResult Trajeto(int rotaId, DateTime data)
        {
            var pontos = _servicoDeRastreio.ObterTrajeto(rotaId, data);
            return Ok(new { data = pontos });
        }

        [HttpGet("veiculos")]
        public IActionResult Veiculos(int rotaId)
        {
            var resultado = _servicoDeRastreio.ObterVeiculosChecklist(rotaId);
            return Ok(new { data = resultado });
        }

        [HttpPost("checklist")]
        public IActionResult SalvarChecklist([FromBody] ChecklistPostDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            int idExec = _servicoDeRastreio.SalvarChecklist(dto, motoristaId);

            return Ok(new { sucesso = true, data = idExec });
        }

        [HttpPost("rotas/iniciar")]
        public IActionResult Iniciar([FromBody] IniciarRotaAppDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            var retorno = _servicoDeRastreio.IniciarRota(dto, motoristaId);
            return Ok(retorno);
        }

        [HttpGet("rotas/em-andamento")]
        public IActionResult ObterEmAndamento([FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            var retorno = _servicoDeRastreio.ObterRotaEmAndamento(motoristaId);
            return Ok(retorno);
        }

        [HttpPost("rotas/encerrar")]
        public IActionResult Encerrar([FromBody] EncerrarRotaAppDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            _servicoDeRastreio.EncerrarRota(dto, motoristaId);
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/confirmar-parada")]
        public IActionResult ConfirmarParada([FromBody] ConfirmarParadaAppDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            _servicoDeRastreio.ConfirmarParada(dto, motoristaId);
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/salvar-ponto")]
        public IActionResult SalvarPonto([FromBody] PostLocalizacaoExecucaoDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            _servicoDeRastreio.SalvarPonto(dto, motoristaId);
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/fazer-pausa")]
        public IActionResult FazerPausa([FromBody] PausaRotaAppDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            _servicoDeRastreio.FazerPausa(dto, motoristaId);
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/finalizar-pausa")]
        public IActionResult FinalizarPausa([FromBody] PausaRotaAppDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            _servicoDeRastreio.FinalizarPausa(dto, motoristaId);
            return Ok(new { sucesso = true });
        }

        [HttpPost("receber-localizacao")]
        public IActionResult ReceberLocalizacao([FromBody] PostLocalizacaoRotaDTO dto)
        {
            _servicoDeRastreio.ReceberLocalizacao(dto);
            return Ok(new { sucesso = true });
        }

        private int ObterServidorId(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) return 0;
            var token = authHeader.Substring("Bearer ".Length).Trim();
            return int.TryParse(token, out int servidorId) ? servidorId : 0;
        }

        private int ObterMotoristaId(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) return 0;
            var token = authHeader.Substring("Bearer ".Length).Trim();

            var cacheKey = $"MotoristaId_TokenApp_{token}";
            if (_memoryCache.TryGetValue(cacheKey, out int cachedId))
            {
                return cachedId;
            }

            if (int.TryParse(token, out int servidorId))
            {
                var motoristaId = _servicoDeRastreio.ObterMotoristaIdPorServidor(servidorId);

                if (motoristaId > 0)
                {
                    _memoryCache.Set(cacheKey, motoristaId, TimeSpan.FromHours(3));
                }

                return motoristaId;
            }
            return 0;
        }
    }
}
