using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AriD.GerenciamentoDePonto.Controllers
{
    /// <summary>
    /// Controller responsável pelas funcionalidades do aplicativo móvel de Rastreio (Motorista/Acompanhante).
    /// </summary>
    /// <remarks>
    /// <b>Arquitetura e Decisão Técnica:</b>
    /// Esta API foi implementada dentro do projeto ASP.NET MVC principal a pedido do proprietário da aplicação.
    /// O objetivo inicial é reduzir custos de hospedagem e manter uma infraestrutura enxuta durante a fase de lançamento,
    /// evitando a necessidade de gerenciar múltiplas instâncias de hospedagem.
    /// 
    /// <b>Evolução Futura:</b>
    /// Existe um planejamento para que, em etapas futuras de escalabilidade, estas funcionalidades de API sejam 
    /// extraídas para um microserviço ou aplicação distinta dentro da mesma solução, separando completamente 
    /// as responsabilidades de Frontend Web das APIs de integração de rastreio e logística.
    /// 
    /// <b>Comunicação:</b>
    /// Este controller comunica-se com o Aplicativo Móvel AriD Rastreio (versões Motorista e Acompanhante),
    /// processando rotas, geolocalização em tempo real, checklists de veículos e eventos de execução de trajeto.
    /// </remarks>
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

        [HttpPost("autentique")]
        public IActionResult Autentique([FromBody] CredenciaisDTO credenciais)
        {
            try
            {
                var acesso = _servicoDeRastreio.AutenticarUsuario(credenciais);
                if (acesso == null)
                    return BadRequest(new { message = "Usuário ou senha incorretos." });

                var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "organizacao", $"{acesso.OrganizacaoId}", $"{acesso.ServidorId}.png");

                if (!Path.Exists(caminhoArquivo))
                    caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "sem-foto.png");

                var bytes = System.IO.File.ReadAllBytes(caminhoArquivo);
                acesso.FotoBase64 = Convert.ToBase64String(bytes);

                return Ok(new { 
                    token = acesso.ServidorId.ToString(), 
                    usuario = new { 
                        id = acesso.ServidorId, 
                        nome = acesso.ServidorNome, 
                        login = credenciais.Usuario, 
                        foto = acesso.FotoBase64, 
                        tipoAcesso = credenciais.TipoAcesso 
                    } 
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("registrar-token")]
        public IActionResult RegistrarToken([FromBody] RegistrarTokenDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            try
            {
                int motoristaId = ObterMotoristaId(auth);
                if (motoristaId <= 0) return Unauthorized();

                int servidorId = ObterServidorId(auth);

                _servicoDeRastreio.RegistrarToken(servidorId, dto.Token, dto.Plataforma);

                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("rotas")]
        public IActionResult Rotas([FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            if (motoristaId == 0) return Unauthorized();

            var resultado = _servicoDeRastreio.ObterRotasMotorista(motoristaId);
            return Ok(new { data = resultado });
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
            if (ultima == null) return NotFound(new { message = "Nenhuma localização encontrada." });

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
            try
            {
                int motoristaId = ObterMotoristaId(auth);
                if (motoristaId == 0) return Unauthorized();

                _servicoDeRastreio.SalvarChecklist(dto, motoristaId);

                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("rotas/iniciar")]
        public IActionResult Iniciar([FromBody] IniciarRotaAppDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            try 
            {
                int motoristaId = ObterMotoristaId(auth);
                if (motoristaId == 0) return Unauthorized();

                var retorno = _servicoDeRastreio.IniciarRota(dto, motoristaId);
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("rotas/encerrar")]
        public IActionResult Encerrar([FromBody] EncerrarRotaAppDTO dto)
        {
            _servicoDeRastreio.EncerrarRota(dto);
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/confirmar-parada")]
        public IActionResult ConfirmarParada([FromBody] ConfirmarParadaAppDTO dto)
        {
            _servicoDeRastreio.ConfirmarParada(dto);
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/salvar-ponto")]
        public IActionResult SalvarPonto([FromBody] PostLocalizacaoExecucaoDTO dto)
        {
            _servicoDeRastreio.SalvarPonto(dto);
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/fazer-pausa")]
        public IActionResult FazerPausa([FromBody] PausaRotaAppDTO dto)
        {
            try
            {
                _servicoDeRastreio.FazerPausa(dto);
                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("rotas/finalizar-pausa")]
        public IActionResult FinalizarPausa([FromBody] PausaRotaAppDTO dto)
        {
            try
            {
                _servicoDeRastreio.FinalizarPausa(dto);
                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("receber-localizacao")]
        public IActionResult ReceberLocalizacao([FromBody] PostLocalizacaoRotaDTO dto)
        {
            try
            {
                _servicoDeRastreio.ReceberLocalizacao(dto);
                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int ObterServidorId(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) return 0;
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (int.TryParse(token, out int servidorId)) return servidorId;
            return 0;
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
