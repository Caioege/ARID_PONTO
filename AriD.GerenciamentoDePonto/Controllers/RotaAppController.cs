using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AriD.GerenciamentoDePonto.Controllers
{
    [Route("api/rota-app")]
    [ApiController]
    public class RotaAppController : Controller
    {
        private readonly IServicoDeAplicativo _servicoDeAplicativo;
        private readonly IServico<Rota> _servicoRota;
        private readonly IServico<LocalizacaoRota> _servicoLocalizacao;
        private readonly IServico<ParadaRota> _servicoParadaRota;
        private readonly IServico<RotaExecucao> _servicoRotaExecucao;
        private readonly IServico<Motorista> _servicoMotorista;
        private readonly IServico<RotaVeiculo> _servicoRotaVeiculo;
        private readonly IMemoryCache _memoryCache;

        public RotaAppController(
            IServicoDeAplicativo servicoDeAplicativo,
            IServico<Rota> servicoRota,
            IServico<LocalizacaoRota> servicoLocalizacao,
            IServico<ParadaRota> servicoParadaRota,
            IServico<RotaExecucao> servicoRotaExecucao,
            IServico<Motorista> servicoMotorista,
            IServico<RotaVeiculo> servicoRotaVeiculo,
            IMemoryCache memoryCache)
        {
            _servicoDeAplicativo = servicoDeAplicativo;
            _servicoRota = servicoRota;
            _servicoLocalizacao = servicoLocalizacao;
            _servicoParadaRota = servicoParadaRota;
            _servicoRotaExecucao = servicoRotaExecucao;
            _servicoMotorista = servicoMotorista;
            _servicoRotaVeiculo = servicoRotaVeiculo;
            _memoryCache = memoryCache;
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

                // Compatibilidade com app Flutter
                return Ok(new { token = acesso.ServidorId.ToString(), usuario = new { id = acesso.ServidorId, nome = acesso.ServidorNome, login = credenciais.Usuario, foto = acesso.FotoBase64 } });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("rotas")]
        public IActionResult Rotas([FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            
            var todas = _servicoRota.ObtenhaLista(r => r.Situacao == BibliotecaDeClasses.Enumeradores.eStatusRota.Ativa && r.MotoristaId == motoristaId).ToList();
            var hoje = DateTime.Now.Date;

            var validas = todas.Where(r => r.Recorrente || (!r.Recorrente && r.DataParaExecucao.HasValue && r.DataParaExecucao.Value.Date == hoje)).ToList();

            var resultado = validas.Select(r => new RotaCheckListDTO {
                Id = r.Id,
                Codigo = $"RT-{r.Id:000}",
                Nome = r.Descricao,
                Descricao = r.Recorrente ? "Rota Recorrente" : $"Planejada para {r.DataParaExecucao:dd/MM/yyyy}"
            }).ToList();

            return Ok(new { data = resultado });
        }

        [HttpGet("veiculos")]
        public IActionResult Veiculos(int rotaId)
        {
            var cacheKey = $"VeiculosApp_Rota_{rotaId}";
            if (_memoryCache.TryGetValue(cacheKey, out List<VeiculoCheckListDTO> cachedVeiculos))
            {
                return Ok(new { data = cachedVeiculos });
            }

            var veiculos = _servicoRotaVeiculo.ObtenhaLista(rv => rv.RotaId == rotaId).Select(rv => rv.Veiculo).ToList();
            var resultado = veiculos.Select(v => new VeiculoCheckListDTO {
                Id = v.Id,
                RotaId = rotaId,
                Nome = v.Modelo,
                Placa = v.Placa,
                Modelo = v.Modelo,
                Cor = v.Cor.ToString(),
                Checklist = new List<CheckListItemDTO>()
            }).ToList();

            _memoryCache.Set(cacheKey, resultado, TimeSpan.FromHours(1));

            return Ok(new { data = resultado });
        }

        [HttpPost("checklist")]
        public IActionResult SalvarChecklist([FromBody] dynamic dto)
        {
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/iniciar")]
        public IActionResult Iniciar([FromBody] IniciarRotaAppDTO dto, [FromHeader(Name = "Authorization")] string auth)
        {
            int motoristaId = ObterMotoristaId(auth);
            var rota = _servicoRota.Obtenha(dto.RotaId);
            if (rota == null) return BadRequest(new { message = "Rota não encontrada" });

            if (_servicoRotaExecucao.ObtenhaLista(re => re.RotaId == rota.Id && re.DataHoraFim == null).Any())
            {
                return BadRequest(new { message = "Esta rota já está em execução no momento." });
            }

            if (!rota.Recorrente && rota.DataParaExecucao.HasValue && rota.DataParaExecucao.Value.Date != DateTime.Now.Date)
            {
                return BadRequest(new { message = "Esta rota não está agendada para hoje." });
            }

            var exec = new RotaExecucao
            {
                OrganizacaoId = rota.OrganizacaoId,
                RotaId = rota.Id,
                VeiculoId = dto.VeiculoId,
                MotoristaId = motoristaId,
                UsuarioIdInicio = motoristaId, // Fallback simplificado
                DataHoraInicio = DateTime.Now
            };

            var id = _servicoRotaExecucao.Adicionar(exec);
            
            var dtoRetorno = new RotaExecucaoDTO
            {
                Id = id,
                RotaId = rota.Id,
                Descricao = rota.Descricao,
                EmAndamento = true,
                Paradas = rota.Paradas.Select(p => new ParadaRotaDTO { Id = p.Id, Endereco = p.Endereco, Latitude = p.Latitude, Longitude = p.Longitude, Link = p.Link }).ToList()
            };

            return Ok(dtoRetorno);
        }

        [HttpPost("rotas/encerrar")]
        public IActionResult Encerrar([FromBody] EncerrarRotaAppDTO dto)
        {
            var exec = _servicoRotaExecucao.Obtenha(dto.RotaExecucaoId);
            if (exec == null) return BadRequest(new { message = "Execução não encontrada" });

            exec.DataHoraFim = DateTime.Now;
            _servicoRotaExecucao.Atualizar(exec);

            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/confirmar-parada")]
        public IActionResult ConfirmarParada([FromBody] ConfirmarParadaAppDTO dto)
        {
            var parada = _servicoParadaRota.Obtenha(dto.ParadaId);
            if (parada != null)
            {
                parada.Entregue = dto.Entregue ?? false;
                parada.Observacao = dto.Observacao;
                parada.ConcluidoEm = DateTime.Now;
                _servicoParadaRota.Atualizar(parada);
            }
            return Ok(new { sucesso = true });
        }

        [HttpPost("rotas/salvar-ponto")]
        public IActionResult SalvarPonto([FromBody] PostLocalizacaoExecucaoDTO dto)
        {
            var exec = _servicoRotaExecucao.Obtenha(dto.RotaExecucaoId);
            if (exec == null) return BadRequest(new { message = "Execução não encontrada" });

            var localizacao = new LocalizacaoRota
            {
                RotaId = exec.RotaId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                DataHora = dto.DataHora,
                OrganizacaoId = exec.OrganizacaoId
            };

            _servicoLocalizacao.Adicionar(localizacao);
            return Ok(new { sucesso = true });
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
                var motorista = _servicoMotorista.Obtenha(m => m.ServidorId == servidorId);
                var motoristaId = motorista?.Id ?? 0;
                
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