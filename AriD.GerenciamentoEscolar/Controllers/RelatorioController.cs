using AriD.BibliotecaDeClasses.Entidades;
using AriD.GerenciamentoEscolar.Helpers;
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
using System.Text;
using AriD.BibliotecaDeClasses.DTO;
using iText.Kernel.Geom;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class RelatorioController : BaseController
    {
        private IServicoDeRelatorios _servicoDeRelatorios;
        private readonly IServico<Escola> _servicoescola;
        private readonly IServico<Turma> _servicoTurma;
        private readonly IServicoDeAlunos _servicoDeAlunos;

        public RelatorioController(
            IServicoDeRelatorios servicoDeRelatorios,
            IServico<Escola> servicoescola,
            IServico<Turma> servicoTurma,
            IServicoDeAlunos servicoDeAlunos)
        {
            _servicoDeRelatorios = servicoDeRelatorios;
            _servicoescola = servicoescola;
            _servicoTurma = servicoTurma;
            _servicoDeAlunos = servicoDeAlunos;
        }

        #region Views

        [HttpGet]
        public IActionResult AlunosDaEscola()
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();
                int redeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;

                if (dadosDaSessao.Perfil != ePerfilDeAcesso.Escola)
                {
                    ViewBag.Escolas = new SelectList(
                        _servicoescola
                            .ObtenhaLista(c => c.RedeDeEnsinoId == redeDeEnsinoId)
                            .OrderBy(c => c.Nome),
                        "Id", "Nome");
                }
                else
                {
                    ViewBag.EscolaNome = _servicoescola.Obtenha(dadosDaSessao.EscolaId.Value).Nome;
                }

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarRelatorioAlunosDaEscola(
            int? escolaId)
        {
            var relatorio = ObtenhaRelatorioAlunosDaEscola(escolaId);
            var nomeArquivo = "Alunos da Escola.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult FrequenciasNaData()
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();
                int redeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;

                if (dadosDaSessao.Perfil != ePerfilDeAcesso.Escola)
                {
                    ViewBag.Escolas = new SelectList(
                        _servicoescola
                            .ObtenhaLista(c => c.RedeDeEnsinoId == redeDeEnsinoId)
                            .OrderBy(c => c.Nome),
                        "Id", "Nome");
                }
                else
                {
                    ViewBag.EscolaNome = _servicoescola.Obtenha(dadosDaSessao.EscolaId.Value).Nome;
                }

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarFrequenciasNaData(int? escolaId, DateTime? data)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Escola)
                escolaId = dadosDaSessao.EscolaId;

            if (!escolaId.HasValue)
                throw new ApplicationException("A escola deve ser informada.");

            if (!data.HasValue)
                throw new ApplicationException("A data deve ser informada.");
            else if (data > DateTime.Today)
                throw new ApplicationException("A data não pode ser maior que a data atual.");

            var relatorio = ObtenhaRelatorioFrequenciasNaData(escolaId, data);
            var nomeArquivo = "Frequências na Data.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult EquipamentosDaEscola()
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();
                int redeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;

                if (dadosDaSessao.Perfil != ePerfilDeAcesso.Escola)
                {
                    ViewBag.Escolas = new SelectList(
                        _servicoescola
                            .ObtenhaLista(c => c.RedeDeEnsinoId == redeDeEnsinoId)
                            .OrderBy(c => c.Nome),
                        "Id", "Nome");
                }
                else
                {
                    ViewBag.EscolaNome = _servicoescola.Obtenha(dadosDaSessao.EscolaId.Value).Nome;
                }

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult ProcessarEquipamentosDaEscola(
            int? escolaId)
        {
            var relatorio = ObtenhaRelatorioEquipamentosDaEscola(escolaId);
            var nomeArquivo = "Equipamentos da Escola.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpPost]
        public IActionResult ProcessarRelatorioListaDeEscolas()
        {
            try
            {
                var relatorio = ObtenhaRelatorioListaDeEscolas();
                var nomeArquivo = "Lista de Escolas.pdf";

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

        [HttpPost]
        public IActionResult ProcessarExportarCSVRegistrosDeFrequencia(
            int? escolaId,
            DateTime dataInicio,
            DateTime dataFim,
            string separador)
        {
            var relatorio = ObtenhaExportacaoDeFrequenciasCSV(escolaId, dataInicio, dataFim, separador);
            var nomeArquivo = "Registros de Frequência.csv";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpPost]
        public IActionResult ProcessarExportarDiarioDeClasse(
            int turmaId,
            string anoMes)
        {
            var split = anoMes.Split('-');
            var ano = int.Parse(split[0]);
            var mes = int.Parse(split[1]);

            var turma = _servicoTurma.Obtenha(turmaId);

            var inicio = new DateTime(ano, mes, 01);
            var final = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes));

            if (turma.InicioDasAulas > inicio)
                inicio = turma.InicioDasAulas;

            if (turma.FimDasAulas < final)
                final = turma.FimDasAulas;

            var alunos = _servicoDeAlunos.ListaDeAlunosParaDiario(turmaId, inicio, final);
            var diarioDTO = new DiarioClasseDTO
            {
                DescricaoTurma = turma.DescricaoComTurnoAnoLetivo,
                Ano = ano,
                Mes = mes,
                Alunos = alunos,
                DataInicio = inicio,
                DataFim = final,
                Horarios = turma.ListaDeHorarioDeAula
            };

            var relatorio = ObtenhaExportacaoDiarioDeClasse(diarioDTO);
            var nomeArquivo = $"Diário de Classe {turma.Descricao}.pdf";

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

        private byte[] ObtenhaRelatorioAlunosDaEscola(
            int? escolaId)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Escola)
                escolaId = dadosDaSessao.EscolaId;

            var alunos = _servicoDeRelatorios.ObtenhaAlunosDaEscola(
                dadosDaSessao.RedeDeEnsinoId,
                escolaId);

            if (alunos.Count == 0)
                throw new ApplicationException("Nenhum aluno encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document, 
                dadosDaSessao.RedeDeEnsinoId, 
                dadosDaSessao.RedeDeEnsinoNome);

            document.SetFontSize(10);

            var grupoAluno = alunos.GroupBy(c => c.EscolaNome);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add("Alunos da Escola")));

            foreach (var escola in grupoAluno.OrderBy(c => c.Key))
            {
                var table = new Table(UnitValue.CreatePercentArray(new[] 
                {
                    55f,
                    30f,
                    15f,
                })).UseAllAvailableWidth();

                table
                    .AddCell(new Cell(1, 3)
                    .Add(new Paragraph()
                            .Add(new Text($"Escola: {escola.Key}")))
                            .SetBold()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Aluno"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Turma"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Id Equipamento"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

                foreach (var aluno in escola
                    .OrderBy(c => c.Turma)
                    .ThenBy(d => d.PessoaNome))
                {
                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(aluno.PessoaNome))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(string.IsNullOrEmpty(aluno.Turma) ? "-" : $"{aluno.Turma} - {aluno.TurmaTurno.DescricaoDoEnumerador()}"))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text($"{aluno.IdEquipamento}"))));
                }

                document.Add(new Div().SetMarginBottom(3).Add(table));
            }

            document.Close();
            return stream.ToArray();
        }

        private byte[] ObtenhaRelatorioFrequenciasNaData(
            int? escolaId, 
            DateTime? data)
        {
            var dadosDeSessao = HttpContext.DadosDaSessao();

            var frequencias = _servicoDeRelatorios.ObtenhaFrequenciaNaData(
                dadosDeSessao.RedeDeEnsinoId,
                escolaId.Value,
                data.Value);

            if (!frequencias.Any())
                throw new ApplicationException("Nenhum registro encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDeSessao.RedeDeEnsinoId,
                dadosDeSessao.RedeDeEnsinoNome);

            document.SetFontSize(10);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add("Frequências na Data"))
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add(frequencias.FirstOrDefault().EscolaNome))
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add(data.Value.ToString("dd/MM/yyyy"))));

            var grupoTurma = frequencias
                .GroupBy(c => c.TurmaId)
                .OrderBy(c => c.First().TurmaDescricao);

            foreach (var turma in grupoTurma)
            {
                var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    40f,
                    10f,
                    25f,
                    25f,
                })).UseAllAvailableWidth();

                table
                    .AddCell(new Cell(1, 4)
                    .Add(new Paragraph()
                            .Add(new Text($"Turma: {turma.First().TurmaDescricao} - {turma.First().TurmaTurno.DescricaoDoEnumerador()}")))
                            .SetBold()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Aluno"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Id Equipamento"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Presença Diário de Classe"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Registro Equipamento"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

                foreach (var aluno in turma.OrderBy(c => c.PessoaNome))
                {
                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(aluno.PessoaNome))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text($"{aluno.IdEquipamento}"))));

                    var descricaoDiario = "-";
                    if (aluno.PresencaoDiarioDeClasse.HasValue)
                    {
                        if (aluno.PresencaoDiarioDeClasse.Value)
                            descricaoDiario = "SIM";
                        else
                            descricaoDiario = "NÃO";
                    }
                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(descricaoDiario))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(aluno.PresencaEquipamento ? "SIM" : "NÃO"))));
                }

                document.Add(new Div().SetMarginBottom(3).Add(table));
            }

            document.Close();
            return stream.ToArray();
        }

        private byte[] ObtenhaRelatorioEquipamentosDaEscola(
            int? escolaId)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Escola)
                escolaId = dadosDaSessao.EscolaId;

            var equipamentos = _servicoDeRelatorios.ObtenhaEquipamentosDaEscola(
                dadosDaSessao.RedeDeEnsinoId,
                escolaId);

            if (equipamentos.Count == 0)
                throw new ApplicationException("Nenhum equipamento encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.RedeDeEnsinoId,
                dadosDaSessao.RedeDeEnsinoNome);

            document.SetFontSize(10);

            var grupoEscola = equipamentos.GroupBy(c => c.EscolaNome);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add("Equipamentos da Escola")));

            foreach (var escola in grupoEscola.OrderBy(c => c.Key))
            {
                var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    55f,
                    30f,
                    15f,
                })).UseAllAvailableWidth();

                table
                    .AddCell(new Cell(1, 3)
                    .Add(new Paragraph()
                            .Add(new Text($"Escola: {escola.Key}")))
                            .SetBold()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Equipamento"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Número de Série"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                        .AddCell(new Cell()
                            .Add(new Paragraph()
                            .Add(new Text("Ativo"))
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

                foreach (var equipamento in escola
                    .OrderBy(c => c.EquipamentoDescricao))
                {
                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text(equipamento.EquipamentoDescricao))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text($"{equipamento.EquipamentoNumeroDeSerie}"))));

                    table.AddCell(new Cell()
                        .Add(new Paragraph()
                        .Add(new Text($"{(equipamento.EquipamentoAtivo ? "SIM" : "NÃO")}"))));
                }

                document.Add(new Div().SetMarginBottom(3).Add(table));
            }

            document.Close();
            return stream.ToArray();
        }

        private byte[] ObtenhaRelatorioListaDeEscolas()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            var escolas = _servicoescola
                .ObtenhaLista(c => c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId)
                .ToList();

            if (escolas.Count == 0)
                throw new ApplicationException("Nenhuma escola encontrada.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.RedeDeEnsinoId,
                dadosDaSessao.RedeDeEnsinoNome);

            document.SetFontSize(10);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add("Lista de Escolas")));

            var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    65f,
                    20f,
                    15f,
                })).UseAllAvailableWidth();

            table
                .AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text("Nome"))
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text("INEP"))
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text("Ativa"))
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            foreach (var escola in escolas.OrderBy(c => c.Nome))
            {
                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text(escola.Nome))));

                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text($"{escola.CodigoInep}"))));

                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text($"{(escola.Ativa ? "SIM" : "NÃO")}"))));
            }

            document.Add(new Div().SetMarginBottom(3).Add(table));

            document.Close();
            return stream.ToArray();
        }

        private byte[] ObtenhaExportacaoDeFrequenciasCSV(
            int? escolaId,
            DateTime dataInicio,
            DateTime dataFim,
            string separador)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Escola)
                escolaId = dadosDaSessao.EscolaId;

            if (dataInicio > dataFim)
                throw new ApplicationException("A data inicial não pode ser maior que a final.");
            else if (dataFim.Subtract(dataInicio).Days > 31)
                throw new ApplicationException("O período informado não pode ser maior que 31 dias.");

            var registros = _servicoDeRelatorios.ObtenhaRegistrosDeFrequencia(
                dadosDaSessao.RedeDeEnsinoId,
                escolaId,
                dataInicio,
                dataFim);

            if (!registros.Any())
                throw new ApplicationException("Nenhum registro encontrado.");

            var sb = new StringBuilder();

            sb.AppendLine(string.Join(separador ?? ";", new[]
            {
                "Id", "EquipamentoId", "EquipamentoDescricao", "DataHoraRegistro",
                "DataHoraRecebimento", "IdEquipamento", "PessoaNome", "EscolaId", "EscolaNome"
            }));

            foreach (var r in registros)
            {
                sb.AppendLine(string.Join(separador ?? ";", new[]
                {
                    r.Id.ToString(),
                    r.EquipamentoId.ToString(),
                    r.EquipamentoDescricao,
                    r.DataHoraRegistro.ToString("yyyy-MM-dd HH:mm:ss"),
                    r.DataHoraRecebimento.ToString("yyyy-MM-dd HH:mm:ss"),
                    r.IdEquipamento,
                    r.PessoaNome,
                    r.EscolaId.ToString(),
                    r.EscolaNome
                }));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private byte[] ObtenhaExportacaoDiarioDeClasse(DiarioClasseDTO diarioDTO)
        {
            if (!diarioDTO.Alunos.Any())
                throw new ApplicationException("Nenhum registro para o diário de classe.");

            var dadosDaSessao = HttpContext.DadosDaSessao();

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4.Rotate());

            AdicioneCabecalho(
                document,
                dadosDaSessao.RedeDeEnsinoId,
                dadosDaSessao.RedeDeEnsinoNome);

            document.SetFontSize(8);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add($"Diário de Classe - {diarioDTO.Mes.ToString().PadLeft(2, '0')}/{diarioDTO.Ano}"))
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add($"{diarioDTO.DescricaoTurma}")));

            var diasComAula = ObterDiasComAula(diarioDTO.DataInicio, diarioDTO.DataFim, diarioDTO.Horarios);

            if (!diasComAula.Any())
                throw new ApplicationException("Nenhum dia com aula no período informado.");

            List<float> colunas = [25f];
            var porcentagemColuna = (100f - 25f) / diasComAula.Count();

            foreach (var dia in diasComAula)
                colunas.Add(porcentagemColuna);

            var table = new Table(
                    UnitValue.CreatePercentArray(colunas.ToArray()))
                .UseAllAvailableWidth();

            table
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Nome"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            foreach (var dia in diasComAula)
            {
                table
                    .AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.25f) 
                        .Add(new Paragraph()
                        .Add(new Text($"{((eDiaDaSemana)(int)dia.DayOfWeek).SiglaDaSemanaDoEnumerador()}\n{dia.ToString("dd/MM")}"))
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)));
            }

            foreach (var aluno in diarioDTO.Alunos.OrderBy(c => c.AlunoNome))
            {
                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .Add(new Text(aluno.AlunoNome))));

                foreach (var dia in diasComAula)
                {
                    var cursando = aluno.EntradaNaTurma <= dia && aluno.SaidaDaTurma >= dia;

                    if (cursando)
                    {
                        var frequencia = aluno.Frequencias.ContainsKey(dia) ? aluno.Frequencias[dia] : null;

                        string descricao = "";
                        if (frequencia.HasValue)
                        {
                            if (frequencia.Value)
                                descricao = "P";
                            else
                                descricao = "F";
                        }

                        table.AddCell(new Cell()
                            .Add(new Paragraph()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Text(descricao))));
                    }
                    else
                    {
                        table.AddCell(new Cell()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.15f)
                            .Add(new Paragraph()
                            .Add(new Text(""))));
                    }
                }
            }

            document.Add(new Div().Add(table));

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

        public List<DateTime> ObterDiasComAula(DateTime dataInicio, DateTime dataFim, List<ItemHorarioDeAula> horarios)
        {
            var diasComAula = new List<DateTime>();

            var diasPermitidos = horarios
                .Select(h => h.DiaDaSemana)
                .Distinct()
                .ToHashSet();

            for (var data = dataInicio.Date; data <= dataFim.Date; data = data.AddDays(1))
            {
                var diaSemana = (eDiaDaSemana)(int)data.DayOfWeek;
                if (diasPermitidos.Contains(diaSemana))
                {
                    diasComAula.Add(data);
                }
            }

            return diasComAula;
        }
    }
}
