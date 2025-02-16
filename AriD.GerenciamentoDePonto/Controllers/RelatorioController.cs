using AriD.BibliotecaDeClasses.Entidades;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Mvc.Rendering;
using AriD.BibliotecaDeClasses.Enumeradores;
using iText.IO.Image;
using iText.Layout.Borders;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RelatorioController : BaseController
    {
        private IServicoDeRelatorios _servicoDeRelatorios;
        private readonly IServico<JustificativaDeAusencia> _servicoJustificativa;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;
        private readonly IServico<HorarioDeTrabalho> _servicoHorario;
        private readonly IServico<TipoDoVinculoDeTrabalho> _servicoTipo;

        public RelatorioController(
            IServicoDeRelatorios servicoDeRelatorios,
            IServico<JustificativaDeAusencia> servicoJustificativa,
            IServico<UnidadeOrganizacional> servicoUnidade,
            IServico<HorarioDeTrabalho> servicoHorario,
            IServico<TipoDoVinculoDeTrabalho> servicoTipo)
        {
            _servicoDeRelatorios = servicoDeRelatorios;
            _servicoJustificativa = servicoJustificativa;
            _servicoUnidade = servicoUnidade;
            _servicoHorario = servicoHorario;
            _servicoTipo = servicoTipo;
        }

        #region Views

        [HttpGet]
        public IActionResult ServidoresComAfastamento()
        {
            try
            {
                int organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;

                ViewBag.Justificativas = new SelectList(
                    _servicoJustificativa
                    .ObtenhaLista(c =>
                        c.OrganizacaoId == organizacaoId && c.LocalDeUso != eLocalDeUsoDeJustificativaDeAusencia.FolhaDePonto)
                    .OrderBy(c => c.SiglaComDescricao),
                    "Id", "SiglaComDescricao");

                ViewBag.Unidades = new SelectList(
                    _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == organizacaoId).OrderBy(c => c.Nome),
                    "Id", "Nome");

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarServidoresComAfastamento(
            int? unidadeLotacaoId,
            DateTime? inicio,
            DateTime? fim,
            int? justificativaId)
        {
            try
            {
                var relatorio = ObtenhaRelatorioServidoresComAfastamento(
                    unidadeLotacaoId, 
                    inicio, 
                    fim, 
                    justificativaId);

                var nomeArquivo = "Servidores com Afastamento.pdf";

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

        [HttpGet]
        public IActionResult ServidoresPorEscala()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult ProcessarServidoresPorEscala()
        {
            try
            {
                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ServidoresPorHorarioDeTrabalho()
        {
            try
            {
                var organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;

                ViewBag.Horarios = new SelectList(_servicoHorario
                    .ObtenhaLista(c => c.OrganizacaoId == organizacaoId)
                    .OrderBy(c => c.SiglaComDescricao),
                    "Id",
                    "SiglaComDescricao");

                ViewBag.Tipos = new SelectList(_servicoTipo
                    .ObtenhaLista(c => c.OrganizacaoId == organizacaoId)
                    .OrderBy(c => c.SiglaComDescricao),
                    "Id",
                    "SiglaComDescricao");

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarServidoresPorHorarioDeTrabalho(
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            try
            {
                var relatorio = RelatorioServidoresPorHorario(
                    horarioDeTrabalhoId, 
                    tipoDeVinculoDeTrabalhoId);

                var nomeArquivo = "Servidores por Horário.pdf";

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

        #endregion

        #region Relatórios

        private byte[] ObtenhaRelatorioServidoresComAfastamento(
            int? unidadeLotacaoId,
            DateTime? inicio,
            DateTime? fim,
            int? justificativaId)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            var afastamentos = _servicoDeRelatorios.ObtenhaAfastamentosParaRelatorio(
                dadosDaSessao.OrganizacaoId,
                unidadeLotacaoId,
                inicio,
                fim,
                justificativaId);

            if (afastamentos.Count == 0)
                throw new ApplicationException("Nenhum afastamento encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document, 
                dadosDaSessao.OrganizacaoId, 
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(10);

            var grupoJustificativa = afastamentos.GroupBy(c => c.JustificativaAusencia);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add("Servidores com Afastamento")));

            foreach (var justificativa in grupoJustificativa.OrderBy(c => c.Key))
            {
                var table = new Table(UnitValue.CreatePercentArray(new[] 
                {
                    35f,
                    15f,
                    30f,
                    10f,
                    10f
                })).UseAllAvailableWidth();

                table
                    .AddCell(new Cell(1, 5)
                    .Add(new Paragraph()
                            .Add(new Text($"Justificativa: {justificativa.Key}")))
                            .SetBold()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Servidor"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("CPF"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Contrato"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Início"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Fim"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

                foreach (var afastamento in justificativa
                    .OrderBy(c => c.InicioAfastamento)
                    .ThenBy(d => d.PessoaNome))
                {
                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(afastamento.PessoaNome))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(afastamento.PessoaCpf))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text($"{afastamento.MatriculaContrato} - {afastamento.TipoContrato}"))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text(afastamento.InicioAfastamento.ToShortDateString()))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text(afastamento.FimAfastamento?.ToShortDateString()))));
                }

                document.Add(new Div().SetMarginBottom(3).Add(table));
            }

            document.Close();
            return stream.ToArray();
        }

        private byte[] RelatorioServidoresPorHorario(
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var dadosDeSessao = HttpContext.DadosDaSessao();

            var horarios = _servicoDeRelatorios.ObtenhaServidoresPorHorario(
                dadosDeSessao.OrganizacaoId,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId);

            if (!horarios.Any())
                throw new ApplicationException("Nenhum registro encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDeSessao.OrganizacaoId,
                dadosDeSessao.OrganizacaoNome);

            document.SetFontSize(10);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add("Servidores por Horário de Trabalho")));

            var grupoHorario = horarios.GroupBy(c => c.HorarioDeTrabalho).OrderBy(c => c.Key);

            foreach (var horario in grupoHorario)
            {
                var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    55f,
                    15f,
                    30f,
                })).UseAllAvailableWidth();

                table
                    .AddCell(new Cell(1, 5)
                    .Add(new Paragraph()
                            .Add(new Text($"Horário de Trabalho: {horario.Key}")))
                            .SetBold()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Servidor"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("CPF"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Contrato"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

                foreach (var contrato in horario.OrderBy(c => c.PessoaNome))
                {
                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(contrato.PessoaNome))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(contrato.PessoaCpf))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text($"{contrato.ContratoMatricula} - {contrato.ContratoTipo}"))));
                }

                document.Add(new Div().SetMarginBottom(3).Add(table));
            }

            document.Close();
            return stream.ToArray();
        }

        #endregion

        private string GetMimeType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private void AdicioneCabecalho(
            Document document, 
            int congregacaoId, 
            string congregacaoNome)
        {
            var tableCabecalho = new Table(2)
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER)
                .SetMarginBottom(10f);

            var path = System.IO.Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "img",
                "brasoes",
                $"{congregacaoId}.png");

            bool possuiBrasao = false;
            if (System.IO.File.Exists(path))
            {
                possuiBrasao = true;

                var brasao = ImageDataFactory.Create(path);
                var brasaoItext = new iText.Layout.Element.Image(brasao);
                brasaoItext.SetWidth(40f);
                brasaoItext.SetHeight(40f);
                brasaoItext.SetPaddingLeft(0f);

                tableCabecalho.AddCell(
                    new Cell(2, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingLeft(0f)
                    .SetWidth(45f)
                    .Add(brasaoItext));
            }

            var celulaNome = new Cell(1, possuiBrasao ? 1 : 2)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph()
                        .SetPaddingTop(15f)
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFixedLeading(12f)
                        .SetBold()
                        .Add(new Text($"{congregacaoNome}").SetFontSize(10f)));

            tableCabecalho.AddCell(celulaNome);

            document.Add(new Div().Add(tableCabecalho));
        }
    }
}