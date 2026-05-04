using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    /// <summary>
    /// Controller responsável pelas funcionalidades do aplicativo móvel de Gestão de Ponto (Servidor/Colaborador).
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
    /// as responsabilidades de Frontend Web das APIs de integração.
    /// 
    /// <b>Comunicação:</b>
    /// Este controller comunica-se prioritariamente com o Aplicativo Móvel AriD Ponto (versão Servidor), 
    /// fornecendo dados de autenticação, horários, registro de ponto e consulta de espelho de ponto.
    /// </remarks>
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

        [HttpGet("horarios-trabalho/{servidorId}")]
        public IActionResult ObtenhaHorariosDeTrabalho(int servidorId)
        {
            return Ok(_servicoDeAplicativo.ObtenhaHorariosDoServidor(servidorId));
        }

        [HttpGet("eventos/{organizacaoId}")]
        public IActionResult ObtenhaEventos(int organizacaoId)
        {
            return Ok(_servicoDeAplicativo.ObtenhaListaDeEventos(organizacaoId));
        }

        [HttpGet("vinculos/{servidorId}")]
        public IActionResult ObtenhaVinculos(int servidorId)
        {
            return Ok(_servicoDeAplicativo.ObtenhaListaDeVinculos(servidorId));
        }

        [HttpGet("unidade/{vinculoId}")]
        public IActionResult ObtenhaUnidades(int vinculoId)
        {
            return Ok(_servicoDeAplicativo.ObtenhaListaDeLotacoes(vinculoId));
        }

        [HttpGet("justificativas/{organizacaoId}")]
        public IActionResult ObtenhaJustificativas([FromRoute] int organizacaoId)
        {
            return Ok(_servicoDeAplicativo.ObtenhaListaDeJustificativas(organizacaoId));
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
                null,
                null,
                new List<AriD.BibliotecaDeClasses.Entidades.BonusCalculado>());

            return File(relatorio, "application/pdf");
        }

        [HttpGet("ultimos-registros-servidor/{servidorId}")]
        public IActionResult ObtenhaUltimosRegistrosDoServidor([FromRoute] int servidorId)
        {
            return Ok(_servicoDeAplicativo.ObtenhaUltimosRegistrosDoServidor(servidorId));
        }

        [HttpPost("receptar-ponto")]
        [Consumes("multipart/form-data")]
        public IActionResult ReceptarRegistro([FromForm] PostRegistroDePontoDTO registro)
        {
            _servicoDeAplicativo.ReceptarRegistro(registro, true);
            return Ok();
        }

        [HttpPost("registrar-token")]
        public IActionResult RegistrarToken([FromBody] RegistrarTokenDTO registrarToken)
        {
            _servicoDeAplicativo.RegistrarToken(registrarToken);
            return Ok();
        }

        [HttpPost("alterar-senha")]
        public IActionResult AlterarSenha([FromBody] AlterarSenhaDTO alterarSenha)
        {
            _servicoDeAplicativo.AlterarSenha(alterarSenha.ServidorId, alterarSenha.SenhaAtual, alterarSenha.NovaSenha);
            return Ok(new { sucesso = true, mensagem = "Senha alterada com sucesso." });
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