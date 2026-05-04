using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.Enumeradores.Permissao;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Extensao;
using AriD.Servicos.Servicos.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;
using System.Text.Json;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class ExportacaoFolhaPagamentoController : BaseController
    {
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;
        private readonly IServicoDeFolhaDePonto _servicoFolha;
        private readonly IServicoDeExportacaoFolhaPagamento _servicoExport;

        public ExportacaoFolhaPagamentoController(
            IServico<UnidadeOrganizacional> servicoUnidade,
            IServicoDeFolhaDePonto servicoFolha,
            IServicoDeExportacaoFolhaPagamento servicoExport)
        {
            _servicoUnidade = servicoUnidade;
            _servicoFolha = servicoFolha;
            _servicoExport = servicoExport;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_ExportacaoFolhaPagamento.Visualizar)) return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            var s = HttpContext.DadosDaSessao();
            var orgId = s.OrganizacaoId;

            // Unidades (mesma lógica das telas)
            if (s.Perfil == ePerfilDeAcesso.Organizacao)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == orgId).OrderBy(c => c.Nome),
                    "Id", "Nome");
            }
            else if (s.Perfil == ePerfilDeAcesso.Departamento)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoFolha.ObtenhaListaDeUnidadesLotadasNoDepartamento(orgId, s.DepartamentoId!.Value),
                    "Id", "Nome");
            }
            else if (s.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == orgId && s.UnidadeOrganizacionais.Contains(c.Id)).OrderBy(c => c.Nome),
                    "Id", "Nome");
            }
            else
            {
                ViewBag.Unidades = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            ViewBag.Layouts = new SelectList(_servicoExport.ObtenhaLayouts(orgId), "Codigo", "Descricao");

            return View();
        }

        [HttpPost]
        public IActionResult Exportar(int unidadeId, string mesDeReferencia, int layoutId, int formatoArquivo, bool somenteServidoresHabilitados = true)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_ExportacaoFolhaPagamento.Gerenciar)) 
                return Json(new { sucesso = false, mensagem = "Sem permissão para exportar." });

            var mesAno = new MesAno(mesDeReferencia);
            var orgId = HttpContext.DadosDaSessao().OrganizacaoId;

            var res = _servicoExport.GerarDadosPacote(
                orgId,
                unidadeId,
                mesAno,
                layoutId,
                (eFormatoArquivoExportacao)formatoArquivo,
                agruparPorMatricula: true,
                somenteServidoresHabilitados);

            var pdfBytes = GerarPdfExportacao(res, unidadeId, mesAno, (eFormatoArquivoExportacao)formatoArquivo);

            var nomePdf = $"Relatorio_Exportacao_Folha_{mesAno.ToString().Replace("/", "-")}.pdf";
            var zipName = $"Exportacao_Folha_{mesAno.ToString().Replace("/", "-")}.zip";

            byte[] zipBytes;
            using (var ms = new MemoryStream())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var entry1 = zip.CreateEntry(res.NomeArquivoExportacao);
                    using (var s = entry1.Open())
                        s.Write(res.BytesExportacao, 0, res.BytesExportacao.Length);

                    var entry2 = zip.CreateEntry(nomePdf);
                    using (var s = entry2.Open())
                        s.Write(pdfBytes, 0, pdfBytes.Length);
                }
                zipBytes = ms.ToArray();
            }

            return Json(new
            {
                sucesso = true,
                fileName = zipName,
                base64 = Convert.ToBase64String(zipBytes),
                mimeType = "application/zip",
                resumo = $"Exportáveis: {res.TotalExportaveis} | Exportados com eventos: {res.TotalExportadosComEventos} | Ignorados: {res.TotalIgnorados}"
            });
        }

        // ---------- Layout ----------
        [HttpGet]
        public async Task<IActionResult> ModalLayout(int id = 0)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_ExportacaoFolhaPagamento.Gerenciar)) 
                return Json(new { sucesso = false, mensagem = "Sem permissão para gerenciar layouts." });

            var orgId = HttpContext.DadosDaSessao().OrganizacaoId;

            LayoutExportacaoFolhaPagamento layout;
            if (id == 0)
            {
                layout = new LayoutExportacaoFolhaPagamento
                {
                    Nome = "Novo Layout",
                    Delimitador = ";",
                    UsarCabecalho = true,
                    FormatoQuantidade = eFormatoQuantidadeExportacao.HHMM,
                    CasasDecimais = 2,
                    UsarBOM = true,
                    Ativo = true
                };
                layout.Campos = new List<LayoutExportacaoFolhaPagamentoCampo>();
            }
            else
            {
                layout = _servicoExport.ObtenhaLayoutCompleto(orgId, id);
            }

            ViewBag.CamposEnum = Enum.GetValues(typeof(eCampoExportacaoFolhaPagamento))
                .Cast<eCampoExportacaoFolhaPagamento>()
                .Select(e => new CodigoDescricaoDTO((int)e, e.DescricaoDoEnumerador()))
                .ToList();

            ViewBag.Formatos = Enum.GetValues(typeof(eFormatoQuantidadeExportacao))
                .Cast<eFormatoQuantidadeExportacao>()
                .Select(e => new CodigoDescricaoDTO((int)e, e.DescricaoDoEnumerador()))
                .ToList();

            var html = await RenderizarComoString("_ModalLayout", layout);
            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult SalvarLayout(LayoutExportacaoFolhaPagamento layout, string camposJson)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_ExportacaoFolhaPagamento.Gerenciar)) 
                return Json(new { sucesso = false, mensagem = "Sem permissão." });

            var orgId = HttpContext.DadosDaSessao().OrganizacaoId;

            var campos = string.IsNullOrWhiteSpace(camposJson)
                ? new List<LayoutExportacaoFolhaPagamentoCampo>()
                : JsonSerializer.Deserialize<List<LayoutExportacaoFolhaPagamentoCampo>>(camposJson) ?? new();

            var id = _servicoExport.SalvarLayout(orgId, layout, campos);

            return Json(new { sucesso = true, mensagem = "Layout salvo.", id });
        }

        // ---------- Códigos ----------
        [HttpGet]
        public async Task<IActionResult> ModalCodigos()
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_ExportacaoFolhaPagamento.Gerenciar)) 
                return Json(new { sucesso = false, mensagem = "Sem permissão." });

            var orgId = HttpContext.DadosDaSessao().OrganizacaoId;

            var lista = _servicoExport.ObtenhaMapeamentos(orgId);

            // Se não tem nada, cria “model” padrão (não salva)
            if (lista.Count == 0)
            {
                lista = Enum.GetValues(typeof(eTipoEventoFolhaPagamento))
                    .Cast<eTipoEventoFolhaPagamento>()
                    .Select(t => new MapeamentoEventoFolhaPagamento { TipoEvento = t, Codigo = "", Descricao = t.DescricaoDoEnumerador() })
                    .ToList();
            }

            ViewBag.TiposEvento = Enum.GetValues(typeof(eTipoEventoFolhaPagamento))
                .Cast<eTipoEventoFolhaPagamento>()
                .Select(e => new CodigoDescricaoDTO((int)e, e.DescricaoDoEnumerador()))
                .ToList();

            var html = await RenderizarComoString("_ModalCodigos", lista);
            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult SalvarCodigos(string mapeamentosJson)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_ExportacaoFolhaPagamento.Gerenciar)) 
                return Json(new { sucesso = false, mensagem = "Sem permissão." });

            var orgId = HttpContext.DadosDaSessao().OrganizacaoId;

            var mapeamentos = string.IsNullOrWhiteSpace(mapeamentosJson)
                ? new List<MapeamentoEventoFolhaPagamento>()
                : JsonSerializer.Deserialize<List<MapeamentoEventoFolhaPagamento>>(mapeamentosJson) ?? new();

            _servicoExport.SalvarMapeamentos(orgId, mapeamentos);

            return Json(new { sucesso = true, mensagem = "Códigos salvos." });
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

        private byte[] GerarPdfExportacao(ResultadoExportacaoFolhaPagamentoDTO res, int unidadeId, MesAno mesAno, eFormatoArquivoExportacao formato)
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph("Relatório de Exportação - Folha de Pagamento")
                .SetTextAlignment(TextAlignment.CENTER).SetBold().SetFontSize(14));

            doc.Add(new Paragraph($"Competência: {mesAno} | Formato: {formato}")
                .SetFontSize(10));
            doc.Add(new Paragraph($"Considerados: {res.TotalVinculosConsiderados} | Exportáveis: {res.TotalExportaveis} | Exportados (com eventos): {res.TotalExportadosComEventos} | Ignorados: {res.TotalIgnorados}")
                .SetFontSize(10).SetBold());

            doc.Add(new Paragraph(" ").SetFontSize(6));

            doc.Add(new Paragraph("Colaboradores exportados (resumo por código)").SetBold());

            var tblExp = new Table(new float[] { 2, 6, 8 }).UseAllAvailableWidth();
            tblExp.AddHeaderCell(new Cell().Add(new Paragraph("Matrícula").SetBold()));
            tblExp.AddHeaderCell(new Cell().Add(new Paragraph("Nome").SetBold()));
            tblExp.AddHeaderCell(new Cell().Add(new Paragraph("Resumo").SetBold()));

            foreach (var e in res.ExportadosResumo.OrderBy(x => x.NomeServidor))
            {
                var resumo = e.ResumoPorCodigo.Count == 0
                    ? "Sem eventos exportáveis (ou sem códigos mapeados)."
                    : string.Join(" | ", e.ResumoPorCodigo.Select(x => $"{x.Codigo}={FmtHHMM(x.Minutos)}"));

                if (e.EventosSemCodigo.Count > 0)
                    resumo += "\nSem código (não exportado): " + string.Join(", ", e.EventosSemCodigo);

                tblExp.AddCell(new Cell().Add(new Paragraph(e.Matricula ?? "")));
                tblExp.AddCell(new Cell().Add(new Paragraph(e.NomeServidor ?? "")));
                tblExp.AddCell(new Cell().Add(new Paragraph(resumo)));
            }
            doc.Add(tblExp);

            doc.Add(new Paragraph(" ").SetFontSize(6));

            doc.Add(new Paragraph("Colaboradores ignorados (motivo)").SetBold());

            var tblIgn = new Table(new float[] { 2, 6, 8 }).UseAllAvailableWidth();
            tblIgn.AddHeaderCell(new Cell().Add(new Paragraph("Matrícula").SetBold()));
            tblIgn.AddHeaderCell(new Cell().Add(new Paragraph("Nome").SetBold()));
            tblIgn.AddHeaderCell(new Cell().Add(new Paragraph("Motivo").SetBold()));

            foreach (var i in res.Ignorados.OrderBy(x => x.NomeServidor))
            {
                tblIgn.AddCell(new Cell().Add(new Paragraph(i.Matricula ?? "")));
                tblIgn.AddCell(new Cell().Add(new Paragraph(i.NomeServidor ?? "")));
                tblIgn.AddCell(new Cell().Add(new Paragraph(i.Motivo ?? "")));
            }

            doc.Add(tblIgn);

            var sessao = HttpContext.DadosDaSessao();
            AdicioneAssinaturaPadrao(doc, sessao);

            doc.Close();
            return ms.ToArray();
        }

        private static void AdicioneAssinaturaPadrao(Document document, SessaoDTO sessao)
        {
            Table tabela = new Table(2);
            tabela.SetWidth(UnitValue.CreatePercentValue(100));
            tabela.SetMarginTop(30f);

            Cell assinatura1 = new Cell().Add(new Paragraph("__________________________________"));
            assinatura1.SetTextAlignment(TextAlignment.CENTER);
            assinatura1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            Cell assinatura2 = new Cell().Add(new Paragraph("__________________________________"));
            assinatura2.SetTextAlignment(TextAlignment.CENTER);
            assinatura2.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            tabela.AddCell(assinatura1);
            tabela.AddCell(assinatura2);

            Cell nome1 = new Cell().Add(new Paragraph("Operador: " + sessao.UsuarioNome));
            nome1.SetTextAlignment(TextAlignment.CENTER);
            nome1.SetFontSize(8f);
            nome1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            Cell nome2 = new Cell().Add(new Paragraph("Responsável / Direção"));
            nome2.SetTextAlignment(TextAlignment.CENTER);
            nome2.SetFontSize(8f);
            nome2.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            tabela.AddCell(nome1);
            tabela.AddCell(nome2);

            document.Add(tabela);
        }

        private static string FmtHHMM(int minutos)
        {
            var h = minutos / 60;
            var m = minutos % 60;
            return $"{h:00}:{m:00}";
        }
    }
}
