using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Extensao;
using AriD.Servicos.Servicos.Interfaces;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class FolhaDePontoController : BaseController
    {
        private readonly IServicoDeFolhaDePonto _servicoDeFolhaDePonto;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;
        private readonly IServico<JustificativaDeAusencia> _servicoJustificativa;
        private readonly IServico<VinculoDeTrabalho> _servicoVinculoDeTrabalho;

        public FolhaDePontoController(
            IServicoDeFolhaDePonto servicoDeFolhaDePonto,
            IServico<UnidadeOrganizacional> servicoUnidade,
            IServico<JustificativaDeAusencia> servicoJustificativa,
            IServico<VinculoDeTrabalho> servicoVinculoDeTrabalho)
        {
            _servicoDeFolhaDePonto = servicoDeFolhaDePonto;
            _servicoUnidade = servicoUnidade;
            _servicoJustificativa = servicoJustificativa;
            _servicoVinculoDeTrabalho = servicoVinculoDeTrabalho;
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

                string html = string.Empty;
                if (!folhaDePonto)
                    html = await RenderizarComoString("_LinhaPontoDia", pontoDoDia);

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

        [HttpPost]
        public ActionResult FecharAbrirFolhaDePonto(
            int vinculoDeTrabalhoId,
            int unidadeId,
            string mesDeReferencia,
            bool fechar)
        {
            try
            {
                var mesAno = new MesAno(mesDeReferencia);

                if (mesAno.Inicio.Date > DateTime.Today)
                    throw new ApplicationException("O início do período é maior que a data atual.");

                if (mesAno.Fim.Date > DateTime.Today)
                    throw new ApplicationException("Folhas de ponto com data final futura não podem ser fechadas.");

                _servicoDeFolhaDePonto.FecharOuAbrirFolhaDePonto(
                    HttpContext.DadosDaSessao().OrganizacaoId,
                    vinculoDeTrabalhoId,
                    mesAno,
                    unidadeId,
                    fechar);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ImprimirFolha(
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

                var vinculoDeTrabalho = _servicoVinculoDeTrabalho.Obtenha(vinculoDeTrabalhoId);
                var eventos = _servicoDeFolhaDePonto.EventosDaFolhaDePonto(organizacaoId, mesAno.Inicio, mesAno.Fim);

                var relatorio = RelatorioFolhaDePonto(
                    vinculoDeTrabalho,
                    mesAno,
                    eventos, 
                    listaDePonto);

                var nomeArquivo = $"Folha de Ponto {mesAno.ToString().Replace("/", "-")}.pdf";

                return Json(new
                {
                    sucesso = true,
                    fileName = nomeArquivo,
                    base64 = Convert.ToBase64String(relatorio),
                    mimeType = GetMimeType(nomeArquivo)
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

        private byte[] RelatorioFolhaDePonto(
            VinculoDeTrabalho vinculoDeTrabalho,
            MesAno mesAno,
            List<EventoAnual> eventos,
            List<PontoDoDia> listaDePonto)
        {
            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            document.SetFontSize(9f);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add($"Folha de Ponto - {mesAno.ToString()}")));

            document.Add(
                new Div()
                .SetMarginBottom(5)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFixedLeading(9f)
                    .SetFontSize(10f)
                    .Add(new Text("Servidor: ").SetBold())
                    .Add(new Text(vinculoDeTrabalho.Servidor.Nome)))
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFixedLeading(9f)
                    .SetFontSize(10f)
                    .Add(new Text("CPF: ").SetBold())
                    .Add(new Text(vinculoDeTrabalho.Servidor.Pessoa.Cpf ?? string.Empty)))
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFixedLeading(9f)
                    .SetFontSize(10f)
                    .Add(new Text("Vínculo de Trabalho: ").SetBold())
                    .Add(new Text(vinculoDeTrabalho.ToString())))
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFixedLeading(9f)
                    .SetFontSize(10f)
                    .Add(new Text("Horário de Trabalho: ").SetBold())
                    .Add(new Text(vinculoDeTrabalho.HorarioDeTrabalho.SiglaComDescricao))));

            var totalDeColunas = 11;

            if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras)
                totalDeColunas++;

            if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaCincoPeriodos)
                totalDeColunas += 4;

            var table = new Table(totalDeColunas).UseAllAvailableWidth();
            var larguraColunas = 25f;

            table
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Data"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("ENT 1"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("SAI 1"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("ENT 2"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("SAI 2"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("ENT 3"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("SAI 3"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaCincoPeriodos)
            {
                table
                    .AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                        .SetWidth(larguraColunas)
                        .Add(new Paragraph()
                        .Add(new Text("ENT 4"))
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                    .AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                        .SetWidth(larguraColunas)
                        .Add(new Paragraph()
                        .Add(new Text("SAI 4"))
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                    .AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                        .SetWidth(larguraColunas)
                        .Add(new Paragraph()
                        .Add(new Text("ENT 5"))
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                    .AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                        .SetWidth(larguraColunas)
                        .Add(new Paragraph()
                        .Add(new Text("SAI 5"))
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)));
            }

            table
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("HOR TRA"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("CAR HOR"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("HOR POS"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetWidth(larguraColunas)
                    .Add(new Paragraph()
                    .Add(new Text("HOR NEG"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras)
                table
                    .AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                        .SetWidth(larguraColunas)
                        .Add(new Paragraph()
                        .Add(new Text("BH SALDO"))
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            foreach (var dia in listaDePonto.OrderBy(c => c.Data))
            {
                var eventoDoDia = eventos.FirstOrDefault(c => c.Data.Date == dia.Data.Date);

                var descricaoDia = $"{dia.Data.ToShortDateString()} - {dia.DiaDaSemana.SiglaDaSemanaDoEnumerador()}";

                //if (eventoDoDia != null)
                //{
                //    descricaoDia += $"[{eventoDoDia.Descricao} ({eventoDoDia.Tipo.DescricaoDoEnumerador()})]";
                //}

                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .SetBold()
                    .Add(new Text(descricaoDia))));

                if (dia.DataFutura) 
                {
                    table.AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.15f)
                        .Add(new Paragraph()
                        .Add(new Text("Data Futura")
                        .SetTextAlignment(TextAlignment.CENTER))));
                }
                else
                {
                    table
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.DescricaoEntrada(1)))))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.DescricaoSaida(1)))))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.DescricaoEntrada(2)))))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.DescricaoSaida(2)))))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.DescricaoEntrada(3)))))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.DescricaoSaida(3)))));

                    if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaCincoPeriodos)
                    {
                        table
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text(dia.DescricaoEntrada(4)))))
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text(dia.DescricaoSaida(4)))))
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text(dia.DescricaoEntrada(5)))))
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text(dia.DescricaoSaida(5)))));
                    }

                    table
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.HorasTrabalhadas?.ToString(@"hh\:mm") ?? string.Empty))))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.CargaHoraria?.ToString(@"hh\:mm") ?? string.Empty))))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.HorasPositivas?.ToString(@"hh\:mm") ?? string.Empty))))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(dia.HorasNegativas?.ToString(@"hh\:mm") ?? string.Empty))));

                    if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras)
                    {
                        var descricaoBH = string.Empty;

                        if ((dia.BancoDeHorasCredito ?? TimeSpan.Zero) > TimeSpan.Zero)
                        {
                            descricaoBH = $"+{dia.BancoDeHorasCredito?.ToString(@"hh\:mm")}";
                        }
                        else if ((dia.BancoDeHorasDebito ?? TimeSpan.Zero) > TimeSpan.Zero) 
                        {
                            descricaoBH = $"-{dia.BancoDeHorasDebito?.ToString(@"hh\:mm")}";
                        }

                        table
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text(descricaoBH ?? string.Empty))));
                    }
                }
            }

            document.Add(new Div().Add(table));
            document.Close();

            return stream.ToArray();
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