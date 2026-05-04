using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class PortalServidorController : Controller
    {
        private readonly IServicoDeAplicativo _servicoDeAplicativo;
        private readonly IServico<Servidor> _servicoServidor;
        private readonly IServico<RegistroDePonto> _servicoRegistroDePonto;
        private readonly IServico<RegistroAplicativo> _servicoRegistroAplicativo;
        private readonly IServico<JustificativaDeAusencia> _servicoJustificativa;

        public PortalServidorController(
            IServicoDeAplicativo servicoDeAplicativo,
            IServico<Servidor> servicoServidor,
            IServico<RegistroDePonto> servicoRegistroDePonto,
            IServico<RegistroAplicativo> servicoRegistroAplicativo,
            IServico<JustificativaDeAusencia> servicoJustificativa)
        {
            _servicoDeAplicativo = servicoDeAplicativo;
            _servicoServidor = servicoServidor;
            _servicoRegistroDePonto = servicoRegistroDePonto;
            _servicoRegistroAplicativo = servicoRegistroAplicativo;
            _servicoJustificativa = servicoJustificativa;
        }

        [HttpGet]
        public IActionResult RegistrosDePonto()
        {
            var servidor = _servicoServidor.Obtenha(HttpContext.DadosDaSessao().UsuarioId);
            if (servidor == null)
                throw new ApplicationException("Servidor não encontrado.");

            var dataInicio = DateTime.Today.AddDays(-15);

            List<RegistroDePonto> registrosDePonto = [];
            List<RegistroAplicativo> solicitacoes = [];
            List<CodigoDescricaoDTO> vinculos = [];
            foreach (var vinculo in servidor.VinculosDeTrabalho)
            {
                vinculos.Add(new CodigoDescricaoDTO(vinculo.Id, vinculo.ToString()));

                foreach (var lotacao in vinculo.Lotacoes)
                {
                    registrosDePonto.AddRange(_servicoRegistroDePonto
                        .ObtenhaLista(c => 
                            c.EquipamentoDePonto.UnidadeOrganizacionalId == lotacao.UnidadeOrganizacionalId 
                            && c.UsuarioEquipamentoId == lotacao.MatriculaEquipamento
                            && c.DataHoraRegistro >= dataInicio));
                }

                solicitacoes.AddRange(_servicoRegistroAplicativo
                        .ObtenhaLista(c =>
                            c.Manual
                            && c.VinculoDeTrabalhoId == vinculo.Id
                            && ((c.JustificativaDeAusenciaId.HasValue && !c.DataFinalAtestado.HasValue) || (!c.JustificativaDeAusenciaId.HasValue))));
            }

            ViewBag.RegistrosManuais = solicitacoes;

            ViewBag.Justificativas = _servicoJustificativa
                .ObtenhaLista(c => c.OrganizacaoId == servidor.OrganizacaoId && c.Ativa && c.LocalDeUso != eLocalDeUsoDeJustificativaDeAusencia.Afastamento)
                .Select(c => new CodigoDescricaoDTO(c.Id, c.Descricao))
                .OrderBy(c => c.Descricao)
                .ToList();

            ViewBag.VinculosDeTrabalho = vinculos;

            ViewBag.PodeRegistrarManual = servidor.RegistroManualNoAplicativo;

            return View(registrosDePonto);
        }

        [HttpGet]
        public IActionResult CarregarLotacoes(int vinculoId)
        {
            var servidor = _servicoServidor.Obtenha(HttpContext.DadosDaSessao().UsuarioId);
            if (servidor == null)
                throw new ApplicationException("Servidor não encontrado.");

            var lotacoes = servidor.VinculosDeTrabalho.First(c => c.Id == vinculoId).Lotacoes;
            return Json(new { sucesso = true, lotacoes = lotacoes.Select(c => new CodigoDescricaoDTO(c.UnidadeOrganizacionalId, c.UnidadeOrganizacional.Nome)) });
        }

        [HttpPost]
        public IActionResult SalvarPontoManual(PostRegistroDePontoDTO registro)
        {
            registro.Manual = true;
            _servicoDeAplicativo.ReceptarRegistro(registro, false);
            return Json(new { sucesso = true, mensagem = "O registro foi inserido." });
        }
        
        [HttpGet]
        public IActionResult Atestados()
        {
            var servidor = _servicoServidor.Obtenha(HttpContext.DadosDaSessao().UsuarioId);
            if (servidor == null)
                throw new ApplicationException("Servidor não encontrado.");

            var dataInicio = DateTime.Today.AddMonths(-6);

            List<CodigoDescricaoDTO> vinculos = [];
            List<RegistroAplicativo> registros = [];
            foreach (var vinculo in servidor.VinculosDeTrabalho)
            {
                registros.AddRange(_servicoRegistroAplicativo.ObtenhaLista(c =>
                    c.VinculoDeTrabalhoId == vinculo.Id 
                    && c.DataInicialAtestado >= dataInicio
                    && c.DataFinalAtestado.HasValue
                    && c.JustificativaDeAusenciaId.HasValue));

                vinculos.Add(new CodigoDescricaoDTO(vinculo.Id, vinculo.ToString()));
            }

            ViewBag.Justificativas = _servicoJustificativa
                .ObtenhaLista(c => c.OrganizacaoId == servidor.OrganizacaoId && c.Ativa && c.LocalDeUso != eLocalDeUsoDeJustificativaDeAusencia.Afastamento)
                .Select(c => new CodigoDescricaoDTO(c.Id, c.Descricao))
                .OrderBy(c => c.Descricao)
                .ToList();

            ViewBag.VinculosDeTrabalho = vinculos;

            ViewBag.PodeRegistrarAtestado = servidor.RegistroDeAtestadoNoAplicativo;

            return View(registros);
        }
        
        [HttpGet]
        public IActionResult AlterarSenha()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SalvarAlterarSenha(
            string senhaAtual, 
            string novaSenha, 
            string confirmacaoSenha)
        {
            if (novaSenha != confirmacaoSenha)
                throw new ApplicationException("A senha e a confirmação não são iguais.");

            var servidor = _servicoServidor.Obtenha(HttpContext.DadosDaSessao().UsuarioId);
            if (servidor == null)
                throw new ApplicationException("Servidor não encontrado.");

            if (string.IsNullOrEmpty(servidor.SenhaPersonalizadaDeAcesso))
            {
                if (senhaAtual != servidor.Pessoa.DataDeNascimento.ToString("ddMMyyyy"))
                    throw new ApplicationException("A senha atual está incorreta.");
            }
            else
            {
                if (Criptografia.CriptografarSenha(senhaAtual) != servidor.SenhaPersonalizadaDeAcesso)
                    throw new ApplicationException("A senha atual está incorreta.");
            }

            servidor.SenhaPersonalizadaDeAcesso = Criptografia.CriptografarSenha(novaSenha);
            _servicoServidor.Atualizar(servidor);

            return Json(new { sucesso = true, mensagem = "A senha foi alterada." });
        }

        [HttpGet]
        public IActionResult BaixarComprovante(int registroId)
        {
            var registro = _servicoRegistroDePonto.Obtenha(registroId);

            var servidor = _servicoServidor.Obtenha(HttpContext.DadosDaSessao().UsuarioId);
            if (servidor == null)
                throw new ApplicationException("Servidor não encontrado.");

            var comprovante = 
                ComprovantePdfHelper.GerarComprovantePortaria671(
                    registro.OrganizacaoId,
                    registro.Organizacao.Nome,
                    null,
                    servidor.Nome,
                    servidor.Pessoa.Cpf,
                    registro.Id,
                    registro.DataHoraRegistro);

            return Json(new
            {
                sucesso = true,
                fileName = $"Comprovante.pdf",
                base64 = comprovante.Replace("data:application/pdf;base64,", string.Empty),
                mimeType = GetMimeType("t.pdf")
            });
        }

        private string GetMimeType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}