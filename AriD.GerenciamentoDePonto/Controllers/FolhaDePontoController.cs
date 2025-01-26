using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class FolhaDePontoController : BaseController
    {
        private readonly IServicoDeFolhaDePonto _servicoDeFolhaDePonto;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;
        private readonly IServico<JustificativaDeAusencia> _servicoJustificativa;

        public FolhaDePontoController(
            IServicoDeFolhaDePonto servicoDeFolhaDePonto,
            IServico<UnidadeOrganizacional> servicoUnidade,
            IServico<JustificativaDeAusencia> servicoJustificativa)
        {
            _servicoDeFolhaDePonto = servicoDeFolhaDePonto;
            _servicoUnidade = servicoUnidade;
            _servicoJustificativa = servicoJustificativa;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                ContextoPontoDoDia();
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult PontoDoDia()
        {
            try
            {
                ContextoPontoDoDia();
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult FiltrosPontoDoDia(int unidadeId)
        {
            try
            {
                var filtros = _servicoDeFolhaDePonto.ObtenhaFiltrosPontoDia(
                    HttpContext.DadosDaSessao().OrganizacaoId,
                    unidadeId);

                return Json(new
                {
                    sucesso = true,
                    funcoes = filtros.Funcoes,
                    departamentos = filtros.Departamentos,
                    horarios = filtros.Horarios
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CarregarPontoDoDia(
            int unidadeId,
            int horarioId,
            DateTime data,
            int? funcaoId,
            int? departamentoId)
        {
            try
            {
                if (data.Date > DateTime.Today)
                    throw new ApplicationException("Não é possível visualizar ponto do dia para datas futuras.");

                var organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
                var pontos = _servicoDeFolhaDePonto.ObtenhaPontosDoDia(
                    data,
                    organizacaoId,
                    unidadeId,
                    horarioId,
                    funcaoId,
                    departamentoId);

                if (!pontos.Any())
                    throw new ApplicationException("Nenhum ponto disponível no dia.");

                ViewBag.Eventos = _servicoDeFolhaDePonto.EventosDaFolhaDePonto(organizacaoId, data, data);

                var html = await RenderizarComoString("_PartialPontoDoDia", pontos);

                return Json(new { sucesso = true, html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AtualizePontoDia(
            int vinculoDeTrabalhoId,
            DateTime data,
            TimeSpan? valorHora,
            int? justificativaId,
            string acao,
            bool folhaDePonto)
        {
            try
            {
                var organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
                var pontoDoDia = _servicoDeFolhaDePonto.AtualizePontoDoDia(
                    organizacaoId,
                    vinculoDeTrabalhoId,
                    data,
                    valorHora,
                    justificativaId,
                    acao);

                ViewBag.Eventos = _servicoDeFolhaDePonto.EventosDaFolhaDePonto(organizacaoId, data, data);

                ViewBag.ExibirNomeServidor = !folhaDePonto;

                var html = await RenderizarComoString("_LinhaPontoDia", pontoDoDia);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ModalEdicaoPontoDia(
            int vinculoDeTrabalhoId, 
            DateTime data, 
            string acao)
        {
            try
            {
                ViewBag.Acao = acao;
                var pontoDoDia = _servicoDeFolhaDePonto.ObtenhaPontoDoDia(vinculoDeTrabalhoId, data);

                ViewBag.Justificativas = _servicoJustificativa
                    .ObtenhaLista(c => 
                        c.OrganizacaoId == this.DadosDaSessao().OrganizacaoId && 
                        c.LocalDeUso != eLocalDeUsoDeJustificativaDeAusencia.Afastamento && 
                        c.Ativa)
                    .Select(c => new CodigoDescricaoDTO(c.Id, c.SiglaComDescricao))
                    .OrderBy(c => c.Descricao);

                var html = await RenderizarComoString("_ModalPontoDoDia", pontoDoDia);
                return Json(new { sucesso = true, html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ServidoresLotadosNaUnidade(int unidadeId)
        {
            try
            {
                var servidores = _servicoDeFolhaDePonto
                    .ObtenhaServidoresLotadosNaUnidade(this.DadosDaSessao().OrganizacaoId, unidadeId);

                return Json(new { sucesso = true, servidores });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult VinculosDoServidor(int servidorId, int unidadeId)
        {
            try
            {
                var vinculos = _servicoDeFolhaDePonto
                    .ObtenhaVinculosDeTrabalhoDoServido(this.DadosDaSessao().OrganizacaoId, servidorId, unidadeId);

                return Json(new { sucesso = true, vinculos });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CarregarFolhaDePonto(
            int vinculoDeTrabalhoId,
            int unidadeId,
            string mesDeReferencia)
        {
            try
            {
                var mesAno = new MesAno(mesDeReferencia);

                if (mesAno.Inicio.Date > DateTime.Today)
                    throw new ApplicationException("O início do período é maior que a data atual.");

                var organizacaoId = this.DadosDaSessao().OrganizacaoId;

                var listaDePonto = _servicoDeFolhaDePonto.CarregueFolhaDePonto(
                    organizacaoId,
                    vinculoDeTrabalhoId,
                    unidadeId,
                    mesAno);

                ViewBag.Eventos = _servicoDeFolhaDePonto.EventosDaFolhaDePonto(organizacaoId, mesAno.Inicio, mesAno.Fim);

                var html = await RenderizarComoString("_PartialFolhaDePonto", listaDePonto);
                return Json(new 
                { 
                    sucesso = true, html, 
                    exibirAbrir = listaDePonto.All(c => c.PontoFechado),
                    exibirAcoes = !listaDePonto.Any(d => d.DataFutura)
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        private void ContextoPontoDoDia()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            if (dadosDaSessao.Perfil != ePerfilDeAcesso.UnidadeOrganizacional)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId && c.Ativa),
                    "Id",
                    "Nome");
            }
            else
            {
                
            }
        }
    }
}