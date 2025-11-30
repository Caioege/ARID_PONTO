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
using AriD.Servicos.Extensao;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Comum;
using System.Globalization;
using System.Text;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RelatorioController : BaseController
    {
        private IServicoDeRelatorios _servicoDeRelatorios;
        private readonly IServico<JustificativaDeAusencia> _servicoJustificativa;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidade;
        private readonly IServico<HorarioDeTrabalho> _servicoHorario;
        private readonly IServico<TipoDoVinculoDeTrabalho> _servicoTipo;
        private readonly IServico<Escala> _servicoEscala;
        private readonly IServicoDeFolhaDePonto _servicoDeFolhaDePonto;

        public RelatorioController(
            IServicoDeRelatorios servicoDeRelatorios,
            IServico<JustificativaDeAusencia> servicoJustificativa,
            IServico<UnidadeOrganizacional> servicoUnidade,
            IServico<HorarioDeTrabalho> servicoHorario,
            IServico<TipoDoVinculoDeTrabalho> servicoTipo,
            IServico<Escala> servicoEscala,
            IServicoDeFolhaDePonto servicoDeFolhaDePonto)
        {
            _servicoDeRelatorios = servicoDeRelatorios;
            _servicoJustificativa = servicoJustificativa;
            _servicoUnidade = servicoUnidade;
            _servicoHorario = servicoHorario;
            _servicoTipo = servicoTipo;
            _servicoEscala = servicoEscala;
            _servicoDeFolhaDePonto = servicoDeFolhaDePonto;
        }

        #region Views

        [HttpGet]
        public IActionResult ServidoresComAfastamento()
        {
            try
            {
                var dadosDaSessao = this.DadosDaSessao();
                int organizacaoId = dadosDaSessao.OrganizacaoId;

                ViewBag.Justificativas = new SelectList(
                    _servicoJustificativa
                    .ObtenhaLista(c =>
                        c.OrganizacaoId == organizacaoId && c.LocalDeUso != eLocalDeUsoDeJustificativaDeAusencia.FolhaDePonto)
                    .OrderBy(c => c.SiglaComDescricao),
                    "Id", "SiglaComDescricao");

                if (dadosDaSessao.Perfil == ePerfilDeAcesso.Organizacao)
                {
                    ViewBag.Unidades = new SelectList(
                        _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == organizacaoId).OrderBy(c => c.Nome),
                        "Id", "Nome");
                }
                else if (dadosDaSessao.Perfil == ePerfilDeAcesso.Departamento)
                {
                    ViewBag.Unidades = new SelectList(
                        _servicoDeFolhaDePonto.ObtenhaListaDeUnidadesLotadasNoDepartamento(organizacaoId, dadosDaSessao.DepartamentoId.Value),
                        "Id", "Nome");
                }

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
            var relatorio = ObtenhaRelatorioServidoresComAfastamento(
                    unidadeLotacaoId,
                    inicio,
                    fim,
                    justificativaId);

            var nomeArquivo = $"{HttpContext.NomenclaturaServidores()} com Afastamento.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult ServidoresPorEscala()
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();
                var organizacaoId = dadosDaSessao.OrganizacaoId;

                var escalas = _servicoEscala.ObtenhaLista(c => c.OrganizacaoId == organizacaoId);

                if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                {
                    escalas = escalas
                        .Where(c => dadosDaSessao.UnidadeOrganizacionais.Contains(c.UnidadeOrganizacionalId))
                        .ToList();
                }
                else if (dadosDaSessao.Perfil == ePerfilDeAcesso.Departamento)
                {
                    var unidadesComDepartamento = _servicoDeFolhaDePonto.ObtenhaListaDeUnidadesLotadasNoDepartamento(organizacaoId, dadosDaSessao.DepartamentoId.Value);

                    escalas = escalas
                        .Where(c => unidadesComDepartamento.Select(c => c.Codigo).Contains(c.UnidadeOrganizacionalId))
                        .ToList();
                }

                ViewBag.Escalas = new SelectList(
                    escalas
                    .OrderBy(c => c.UnidadeOrganizacional.Nome)
                    .ThenBy(c => c.Descricao)
                    .Select(c => new CodigoDescricaoGrupoDTO(c.Id, c.Descricao, c.UnidadeOrganizacional.Nome)),
                    "Codigo",
                    "Descricao",
                    null,
                    "Grupo");

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarServidoresPorEscala(int? escalaId)
        {
            try
            {
                var relatorio = RelatorioServidoresPorEscala(escalaId);
                var nomeArquivo = $"{HttpContext.NomenclaturaServidores().ToLower()} por Escala.pdf";

                return Json(new
                {
                    sucesso = true,
                    fileName = nomeArquivo,
                    base64 = Convert.ToBase64String(relatorio),
                    mimeType = GetMimeType(nomeArquivo)
                });

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
            var relatorio = RelatorioServidoresPorHorario(
                    horarioDeTrabalhoId,
                    tipoDeVinculoDeTrabalhoId);

            var nomeArquivo = $"{HttpContext.NomenclaturaServidores()} por Horário.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult ListaDeServidores()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            var organizacaoId = dadosDaSessao.OrganizacaoId;

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Organizacao)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == organizacaoId).OrderBy(c => c.Nome),
                    "Id", "Nome");
            }
            else if (dadosDaSessao.Perfil == ePerfilDeAcesso.Departamento)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoDeFolhaDePonto.ObtenhaListaDeUnidadesLotadasNoDepartamento(organizacaoId, dadosDaSessao.DepartamentoId.Value),
                    "Id", "Nome");
            }

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

        [HttpPost]
        public ActionResult ProcessarListaDeServidores(
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var relatorio = RelatorioListaDeServidores(
                unidadeId,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId);

            var nomeArquivo = $"Lista de {HttpContext.NomenclaturaServidores()}.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult HorasPorServidor()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            var organizacaoId = dadosDaSessao.OrganizacaoId;

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Organizacao)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == organizacaoId).OrderBy(c => c.Nome),
                    "Id", "Nome");
            }
            else if (dadosDaSessao.Perfil == ePerfilDeAcesso.Departamento)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoDeFolhaDePonto.ObtenhaListaDeUnidadesLotadasNoDepartamento(organizacaoId, dadosDaSessao.DepartamentoId.Value),
                    "Id", "Nome");
            }

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

        [HttpPost]
        public ActionResult ProcessarHorasPorServidor(
            string mesAno,
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            if (string.IsNullOrEmpty(mesAno))
                throw new ApplicationException("O período deve ser informado.");

            var periodo = new MesAno(mesAno);

            if (periodo.Inicio.Date > DateTime.Today)
                throw new ApplicationException("O início do período é maior que a data atual.");

            var relatorio = RelatorioHorasPorServidor(
                periodo,
                unidadeId,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId);

            var nomeArquivo = $"Horas Positivas e Negativas por {HttpContext.NomenclaturaServidor()}.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult AniversariantesDoPeriodo()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            var organizacaoId = dadosDaSessao.OrganizacaoId;

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Organizacao)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == organizacaoId).OrderBy(c => c.Nome),
                    "Id", "Nome");
            }
            else if (dadosDaSessao.Perfil == ePerfilDeAcesso.Departamento)
            {
                ViewBag.Unidades = new SelectList(
                    _servicoDeFolhaDePonto.ObtenhaListaDeUnidadesLotadasNoDepartamento(organizacaoId, dadosDaSessao.DepartamentoId.Value),
                    "Id", "Nome");
            }

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

        [HttpPost]
        public ActionResult ProcessarAniversariantesDoPeriodo(
            int mes,
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            if (mes == 0)
                throw new ApplicationException("O mês deve ser informado.");

            var relatorio = RelatorioAniversariantesDoPeriodo(
                mes,
                unidadeId,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId);

            var nomeArquivo = $"Aniversariantes do Mês {mes.ToString().PadLeft(2, '0')}.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpPost]
        public ActionResult ExportarEventosAnuais(eTipoDeExportacao tipoDeExportacao)
        {
            string extensao;

            switch (tipoDeExportacao)
            {
                case eTipoDeExportacao.Excel:
                    extensao = ".xlsx";
                    break;
                case eTipoDeExportacao.TXT:
                    extensao = ".txt";
                    break;
                default:
                    extensao = ".pdf";
                    break;
            }

            var relatorio = RelatorioAniversariantesDoPeriodo(tipoDeExportacao);
            var nomeArquivo = $"Eventos Anuais{extensao}";
            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
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

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeLotacaoId = dadosDaSessao.UnidadeOrganizacionais.First();

            var afastamentos = _servicoDeRelatorios.ObtenhaAfastamentosParaRelatorio(
                dadosDaSessao.OrganizacaoId,
                unidadeLotacaoId,
                inicio,
                fim,
                justificativaId,
                dadosDaSessao.DepartamentoId);

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
                    .Add($"{HttpContext.NomenclaturaServidores()} com Afastamento")));

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
                            .Add(new Text($"{HttpContext.NomenclaturaServidor()}"))
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
                tipoDeVinculoDeTrabalhoId,
                dadosDeSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional ? dadosDeSessao.UnidadeOrganizacionais.First() : null,
                dadosDeSessao.DepartamentoId);

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
                    .Add($"{HttpContext.NomenclaturaServidores()} por Horário de Trabalho")));

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
                            .Add(new Text($"{HttpContext.NomenclaturaServidor()}"))
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

        private byte[] RelatorioServidoresPorEscala(int? escalaId)
        {
            var dadosDeSessao = HttpContext.DadosDaSessao();

            var servidores = _servicoDeRelatorios.ObtenhaServidoresPorEscala(
                dadosDeSessao.OrganizacaoId,
                escalaId,
                dadosDeSessao.DepartamentoId);

            if (!servidores.Any())
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
                    .Add($"{HttpContext.NomenclaturaServidores()} por Horário de Trabalho")));

            var grupoUnidade = servidores
                .OrderBy(c => c.UnidadeNome)
                .GroupBy(c => c.UnidadeId)
                .OrderBy(c => c.Key);

            foreach (var unidade in grupoUnidade)
            {
                var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    55f,
                    15f,
                    30f,
                })).UseAllAvailableWidth();

                table
                    .AddCell(new Cell(1, 3)
                    .Add(new Paragraph()
                            .Add(new Text($"{unidade.First().UnidadeNome}")))
                            .SetBold()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));

                foreach (var escala in unidade
                    .OrderBy(c => c.EscalaDescricao)
                    .GroupBy(c => c.EscalaId))
                {
                    table
                        .AddCell(new Cell(1, 5)
                        .Add(new Paragraph()
                                .Add(new Text($"Escala: {escala.First().EscalaDescricao} - {escala.First().EscalaTipo.DescricaoDoEnumerador()}")))
                                .SetBold()
                                .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE))
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text($"{HttpContext.NomenclaturaServidor()}"))
                                .SetBackgroundColor(ColorConstants.GRAY, 0.05f)
                                .SetBold()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text("CPF"))
                                .SetBackgroundColor(ColorConstants.GRAY, 0.05f)
                                .SetBold()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text("Contrato"))
                                .SetBackgroundColor(ColorConstants.GRAY, 0.05f)
                                .SetBold()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

                    foreach (var contrato in escala.OrderBy(c => c.PessoaNome))
                    {
                        table.AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(contrato.PessoaNome))));

                        table.AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text(contrato.PessoaCpf))));

                        table.AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text($"{contrato.MatriculaVinculo} - {contrato.TipoContrato}"))));
                    }

                    document.Add(new Div().SetMarginBottom(3).Add(table));
                }
            }

            document.Close();
            return stream.ToArray();
        }

        private byte[] RelatorioListaDeServidores(
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var dadosDaSessao = this.DadosDaSessao();
            if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeId = dadosDaSessao.UnidadeOrganizacionais.First();

            var servidores = _servicoDeRelatorios.ObtenhaListaDeServidores(
                dadosDaSessao.OrganizacaoId,
                unidadeId,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId,
                dadosDaSessao.DepartamentoId);

            if (servidores.Count == 0)
                throw new ApplicationException($"Nenhum {HttpContext.NomenclaturaServidor().ToLower()} encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.OrganizacaoId,
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(10);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add($"Lista de {HttpContext.NomenclaturaServidores()}")));

            var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    50f,
                    25f,
                    25f,
                })).UseAllAvailableWidth();

            table
                .AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text("Nome"))
                    .SetBackgroundColor(ColorConstants.GRAY, 0.05f)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text("CPF"))
                    .SetBackgroundColor(ColorConstants.GRAY, 0.05f)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text("Data de Nascimento"))
                    .SetBackgroundColor(ColorConstants.GRAY, 0.05f)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            foreach (var servidor in servidores)
            {
                table.AddCell(new Cell()
                .Add(new Paragraph()
                    .Add(new Text(servidor.PessoaNome))));

                table.AddCell(new Cell()
                .Add(new Paragraph()
                    .Add(new Text($"{servidor.PessoaCpf}"))));

                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text(servidor.DataDeNascimento.ToString("dd/MM/yyyy")))));
            }

            document.Add(new Div().Add(table));

            document.Close();
            return stream.ToArray();
        }

        private byte[] RelatorioHorasPorServidor(
            MesAno mesAno,
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var dadosDaSessao = this.DadosDaSessao();
            if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeId = dadosDaSessao.UnidadeOrganizacionais.First();
            else if (!unidadeId.HasValue)
                throw new ApplicationException("A unidade deve ser informada.");

            var listaDeVinculos = _servicoDeRelatorios
                .ObtenhaListaDeVinculos(
                    dadosDaSessao.OrganizacaoId,
                    unidadeId.Value,
                    horarioDeTrabalhoId,
                    tipoDeVinculoDeTrabalhoId,
                    null)
                .OrderBy(c => c.Servidor.Nome);

            if (listaDeVinculos.Count() == 0)
                throw new ApplicationException($"Nenhum {HttpContext.NomenclaturaServidor().ToLower()} encontrado para os filtros informados.");

            Dictionary<int, (TimeSpan?, TimeSpan?)> dicionarioHorasDoServidor = new();

            foreach (var vinculo in listaDeVinculos)
            {
                if (vinculo.Fim.HasValue && vinculo.Fim < mesAno.Inicio)
                    continue;

                if (!vinculo.Lotacoes.Any(d => d.UnidadeOrganizacionalId == unidadeId.Value && (!d.Saida.HasValue || d.Saida > mesAno.Inicio)))
                    continue;

                List<PontoDoDia> listaDePonto = _servicoDeFolhaDePonto.CarregueFolhaDePonto(dadosDaSessao.OrganizacaoId, vinculo.Id, unidadeId.Value, mesAno);

                if (listaDePonto.Count() == 0)
                    continue;

                var cargaHorariaMensalFixa = vinculo.HorarioDeTrabalho.TipoCargaHoraria == eTipoCargaHoraria.MensalFixa;

                var horasTrabalhadas = TimeSpan.FromTicks(listaDePonto.Where(c => !c.DataFutura).Sum(c => (c.HorasTrabalhadas ?? TimeSpan.Zero).Ticks));

                var cargaHoraria = TimeSpan.FromTicks(listaDePonto.Sum(c => (c.CargaHoraria ?? TimeSpan.Zero).Ticks));
                if (cargaHorariaMensalFixa)
                {
                    cargaHoraria = TimeSpan.FromHours(vinculo.HorarioDeTrabalho.CargaHorariaMensalFixa ?? 0);
                }

                var horasPositivas = cargaHorariaMensalFixa ?
                    (horasTrabalhadas > cargaHoraria ? horasTrabalhadas - cargaHoraria : TimeSpan.Zero) :
                    TimeSpan.FromTicks(listaDePonto.Where(c => !c.DataFutura).Sum(c => (c.HorasPositivas ?? TimeSpan.Zero).Ticks));

                var horasNegativas = cargaHorariaMensalFixa ?
                    (cargaHoraria > horasTrabalhadas ? cargaHoraria - horasTrabalhadas : TimeSpan.Zero) :
                    TimeSpan.FromTicks(listaDePonto.Where(c => !c.DataFutura).Sum(c => (c.HorasNegativas ?? TimeSpan.Zero).Ticks));

                dicionarioHorasDoServidor.Add(vinculo.Id, (horasPositivas, horasNegativas));
            }

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.OrganizacaoId,
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(10);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add($"Horas Positivas e Negativas por {HttpContext.NomenclaturaServidor()}\n{mesAno.ToString()}")));

            var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    35f,
                    25f,
                    10f,
                    10f,
                    10f,
                })).UseAllAvailableWidth();

            table
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Nome"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Vínculo de Trabalho"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Horas\nPositivas"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Horas\nNegativas"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Saldo\nMês"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            foreach (var servidor in dicionarioHorasDoServidor)
            {
                var vinculo = listaDeVinculos.First(c => c.Id == servidor.Key);

                table.AddCell(new Cell()
                .Add(new Paragraph()
                    .Add(new Text(vinculo.Servidor.Nome))));

                table.AddCell(new Cell()
                .Add(new Paragraph()
                    .Add(new Text($"{vinculo.ToString()}"))));

                var horasPositivas = dicionarioHorasDoServidor[vinculo.Id].Item1;
                var horasNegativas = dicionarioHorasDoServidor[vinculo.Id].Item2;

                table.AddCell(new Cell()
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Text($"{FormatarTimeSpan(horasPositivas)}"))));

                table.AddCell(new Cell()
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Text($"{FormatarTimeSpan(horasNegativas)}"))));

                table.AddCell(new Cell()
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Text($"{FormatarSaldoMes(horasPositivas, horasNegativas)}"))));
            }

            document.Add(new Div().Add(table));

            document.Close();
            return stream.ToArray();
        }

        private byte[] RelatorioAniversariantesDoPeriodo(
            int mes,
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var dadosDaSessao = this.DadosDaSessao();
            if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeId = dadosDaSessao.UnidadeOrganizacionais.First();
            else if (!unidadeId.HasValue)
                throw new ApplicationException("A unidade deve ser informada.");

            var listaDeVinculos = _servicoDeRelatorios
                .ObtenhaListaDeVinculos(
                    dadosDaSessao.OrganizacaoId,
                    unidadeId.Value,
                    horarioDeTrabalhoId,
                    tipoDeVinculoDeTrabalhoId,
                    null)
                .OrderBy(c => c.Servidor.Nome);

            var servidores = listaDeVinculos
                .Select(c => c.Servidor)
                .DistinctBy(c => c.Id)
                .ToList();

            servidores.RemoveAll(x => x.Pessoa.DataDeNascimento.Month != mes);

            if (servidores.Count() == 0)
                throw new ApplicationException($"Nenhum {HttpContext.NomenclaturaServidor().ToLower()} encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.OrganizacaoId,
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(10);

            var nomeMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add($"Aniversariantes do período: {nomeMes}")));

            var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    50f,
                    25f,
                    25f,
                })).UseAllAvailableWidth();

            table
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Nome"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("CPF"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Data de\nNascimento"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            foreach (var servidor in servidores
                .OrderBy(c => c.Pessoa.DataDeNascimento)
                .ThenBy(c => c.Nome))
            {
                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text(servidor.Nome))));

                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Text($"{servidor.Pessoa.Cpf}"))));

                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Text($"{servidor.Pessoa.DataDeNascimento.ToString("dd/MM/yyyy")}"))));
            }

            document.Add(new Div().Add(table));

            document.Close();
            return stream.ToArray();
        }

        private byte[] RelatorioAniversariantesDoPeriodo(eTipoDeExportacao tipoDeExportacao)
        {
            var dadosDaSessao = this.DadosDaSessao();
            var eventos = _servicoDeRelatorios.ObtenhaListaDeEventosDaOrganizacao(dadosDaSessao.OrganizacaoId);

            if (eventos == null || eventos.Count() == 0)
                throw new ApplicationException("Não há nenhum evento anual cadastrado.");

            switch (tipoDeExportacao)
            {
                case eTipoDeExportacao.Excel:
                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Eventos Anuais");

                        worksheet.Cell(1, 1).Value = "Descrição";
                        worksheet.Cell(1, 2).Value = "Tipo";
                        worksheet.Cell(1, 3).Value = "Data";

                        var header = worksheet.Range("A1:C1");
                        header.Style.Font.Bold = true;
                        header.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

                        int linha = 2;
                        foreach (var evento in eventos)
                        {
                            worksheet.Cell(linha, 1).Value = evento.Descricao;
                            worksheet.Cell(linha, 2).Value = evento.Tipo.DescricaoDoEnumerador();
                            worksheet.Cell(linha, 3).Value = evento.Data;
                            worksheet.Cell(linha, 3).Style.DateFormat.Format = "dd/MM/yyyy";
                            linha++;
                        }

                        worksheet.Columns().AdjustToContents();

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            return stream.ToArray();
                        }
                    }

                case eTipoDeExportacao.TXT:
                    var sb = new StringBuilder();

                    sb.AppendLine("EVENTOS ANUAIS");
                    sb.AppendLine(new string('-', 80));
                    sb.AppendLine($"{"Descrição".PadRight(40)} | {"Tipo".PadRight(20)} | {"Data".PadRight(15)}");
                    sb.AppendLine(new string('-', 80));

                    foreach (var evento in eventos)
                    {
                        var descricao = evento.Descricao.Length > 37 ? evento.Descricao.Substring(0, 37) + "..." : evento.Descricao;
                        var tipo = evento.Tipo.DescricaoDoEnumerador();
                        var data = evento.Data.ToString("dd/MM/yyyy");

                        sb.AppendLine($"{descricao.PadRight(40)} | {tipo.PadRight(20)} | {data.PadRight(15)}");
                    }

                    return Encoding.UTF8.GetBytes(sb.ToString());

                default:
                    using (var stream = new MemoryStream())
                    {
                        var writer = new PdfWriter(stream);
                        var pdf = new PdfDocument(writer);
                        var document = new Document(pdf);

                        AdicioneCabecalho(
                            document,
                            dadosDaSessao.OrganizacaoId,
                            dadosDaSessao.OrganizacaoNome);

                        document.SetFontSize(10);

                        document.Add(
                            new Div()
                            .SetMarginBottom(10)
                            .Add(new Paragraph()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(15f)
                                .Add($"Eventos Anuais")));

                        var table = new Table(UnitValue.CreatePercentArray(new[]
                            {
                    40f,
                    30f,
                    30f,
                })).UseAllAvailableWidth();

                        table
                            .AddCell(new Cell()
                                .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                                .Add(new Paragraph()
                                .Add(new Text("Descrição"))
                                .SetBold()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                            .AddCell(new Cell()
                                .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                                .Add(new Paragraph()
                                .Add(new Text("Tipo"))
                                .SetBold()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                            .AddCell(new Cell()
                                .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                                .Add(new Paragraph()
                                .Add(new Text("Data"))
                                .SetBold()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

                        foreach (var evento in eventos)
                        {
                            table.AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text(evento.Descricao))));

                            table.AddCell(new Cell()
                                .Add(new Paragraph()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Text(evento.Tipo.DescricaoDoEnumerador()))));

                            table.AddCell(new Cell()
                                .Add(new Paragraph()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Text($"{evento.Data.ToString("dd/MM/yyyy")}"))));
                        }

                        document.Add(new Div().Add(table));

                        document.Close();
                        return stream.ToArray();
                    }
            }
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

        static string FormatarTimeSpan(TimeSpan? valor)
        {
            if (!valor.HasValue)
                return string.Empty;

            valor = valor.Value.Duration();

            var horas = (int)valor.Value.TotalHours;
            var minutos = valor.Value.Minutes.ToString().PadLeft(2, '0');

            return $"{horas:D2}:{minutos}";
        }

        static string FormatarSaldoMes(TimeSpan? horasPositivas, TimeSpan? horasNegativas)
        {
            if ((horasPositivas ?? TimeSpan.Zero) > TimeSpan.Zero || (horasNegativas ?? TimeSpan.Zero) > TimeSpan.Zero)
            {
                if (horasPositivas == horasNegativas)
                {
                    return "00:00";
                }

                if (horasPositivas > horasNegativas)
                {
                    var diferencahoras = horasPositivas - horasNegativas;
                    return "+" + (diferencahoras.Value.TotalHours >= 1
                        ? $"{(int)diferencahoras.Value.TotalHours}:{diferencahoras.Value.Minutes.ToString().PadLeft(2, '0')}"
                        : $"00:{diferencahoras.Value.Minutes.ToString().PadLeft(2, '0')}");
                }
                else
                {
                    var diferencahoras = horasNegativas - horasPositivas;
                    return "-" + (diferencahoras.Value.TotalHours >= 1
                        ? $"{(int)diferencahoras.Value.TotalHours}:{diferencahoras.Value.Minutes.ToString().PadLeft(2, '0')}"
                        : $"00:{diferencahoras.Value.Minutes.ToString().PadLeft(2, '0')}");
                }
            }

            return string.Empty;
        }
    }
}