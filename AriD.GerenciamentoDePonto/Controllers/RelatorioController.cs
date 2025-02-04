using AriD.BibliotecaDeClasses.Entidades;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using iText.Layout;
using System;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Mvc.Rendering;
using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RelatorioController : BaseController
    {
        private IServicoDeRelatorios _servicoDeRelatorios;
        private readonly IServico<JustificativaDeAusencia> _servicoJustificativa;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;

        public RelatorioController(
            IServicoDeRelatorios servicoDeRelatorios,
            IServico<JustificativaDeAusencia> servicoJustificativa,
            IServico<UnidadeOrganizacional> servicoUnidade)
        {
            _servicoDeRelatorios = servicoDeRelatorios;
            _servicoJustificativa = servicoJustificativa;
            _servicoUnidade = servicoUnidade;
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

        [HttpPost]
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
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarServidoresPorHorarioDeTrabalho()
        {
            try
            {
                return Json(new 
                { 
                    sucesso = true
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
            var afastamentos = _servicoDeRelatorios.ObtenhaAfastamentosParaRelatorio(
                HttpContext.DadosDaSessao().OrganizacaoId,
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
    }
}