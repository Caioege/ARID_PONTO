using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Extensao;
using AriD.Servicos.Servicos.Interfaces;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
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
        private readonly IServico<PontoDoDia> _servicoPontoDoDia;

        public FolhaDePontoController(
            IServicoDeFolhaDePonto servicoDeFolhaDePonto,
            IServico<UnidadeOrganizacional> servicoUnidade,
            IServico<JustificativaDeAusencia> servicoJustificativa,
            IServico<VinculoDeTrabalho> servicoVinculoDeTrabalho,
            IServico<PontoDoDia> servicoPontoDoDia)
        {
            _servicoDeFolhaDePonto = servicoDeFolhaDePonto;
            _servicoUnidade = servicoUnidade;
            _servicoJustificativa = servicoJustificativa;
            _servicoVinculoDeTrabalho = servicoVinculoDeTrabalho;
            _servicoPontoDoDia = servicoPontoDoDia;
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

        [HttpGet]
        public async Task<IActionResult> CarregarPontoDoDia(
            int unidadeId,
            int horarioId,
            DateTime data,
            int? funcaoId,
            int? departamentoId)
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

        [HttpPost]
        public async Task<IActionResult> AtualizePontoDia(
            int vinculoDeTrabalhoId,
            DateTime data,
            TimeSpan? valorHora,
            int? justificativaId,
            string acao,
            bool folhaDePonto)
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

        [HttpGet]
        public async Task<IActionResult> ModalEdicaoPontoDia(
            int vinculoDeTrabalhoId, 
            DateTime data, 
            string acao)
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

        [HttpGet]
        public IActionResult ServidoresLotadosNaUnidade(int unidadeId)
        {
            var dadosDaSessao = this.DadosDaSessao();
            var servidores = _servicoDeFolhaDePonto.ObtenhaServidoresLotadosNaUnidade(
                dadosDaSessao.OrganizacaoId, 
                unidadeId,
                dadosDaSessao.DepartamentoId);

            return Json(new { sucesso = true, servidores });
        }

        [HttpGet]
        public IActionResult VinculosDoServidor(int servidorId, int unidadeId)
        {
            var dadosDaSessao = this.DadosDaSessao();
            var vinculos = _servicoDeFolhaDePonto.ObtenhaVinculosDeTrabalhoDoServido(
                dadosDaSessao.OrganizacaoId, 
                servidorId, 
                unidadeId,
                dadosDaSessao.DepartamentoId);

            return Json(new { sucesso = true, vinculos });
        }

        [HttpGet]
        public async Task<IActionResult> CarregarFolhaDePonto(
            int vinculoDeTrabalhoId,
            int unidadeId,
            string mesDeReferencia)
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

            ViewBag.HorarioDeTrabalho = _servicoVinculoDeTrabalho.Obtenha(vinculoDeTrabalhoId).HorarioDeTrabalho;
            ViewBag.Eventos = _servicoDeFolhaDePonto.EventosDaFolhaDePonto(organizacaoId, mesAno.Inicio, mesAno.Fim);

            ViewBag.ObservacaoServidor = _servicoDeFolhaDePonto.ObtenhaObservacaoDoServidorNaFolhaDePonto(vinculoDeTrabalhoId);

            var html = await RenderizarComoString("_PartialFolhaDePonto", listaDePonto);
            return Json(new
            {
                sucesso = true,
                html,
                exibirAbrir = listaDePonto.All(c => c.PontoFechado),
                exibirAcoes = !listaDePonto.Any(d => d.DataFutura)
            });
        }

        [HttpPost]
        public ActionResult FecharAbrirFolhaDePonto(
            int vinculoDeTrabalhoId,
            int unidadeId,
            string mesDeReferencia,
            bool fechar)
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

        [HttpPost]
        public ActionResult ImprimirFolha(
            int vinculoDeTrabalhoId,
            int unidadeId,
            string mesDeReferencia)
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

            if (HttpContext.DadosDaSessao().Perfil == ePerfilDeAcesso.Servidor)
            {
                if (!listaDePonto.All(c => c.PontoFechado))
                    throw new ApplicationException("Não é possível exportar essa folha pois ela ainda não foi fechada.");
            }

            var vinculoDeTrabalho = _servicoVinculoDeTrabalho.Obtenha(vinculoDeTrabalhoId);
            var eventos = _servicoDeFolhaDePonto.EventosDaFolhaDePonto(organizacaoId, mesAno.Inicio, mesAno.Fim);

            var relatorio = RelatorioFolhaDePonto(
                HttpContext.DadosDaSessao(),
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

        [HttpPost]
        public ActionResult MovimentarRegistro(int id, string classe, bool avancar)
        {
            _servicoDeFolhaDePonto.MovimentarRegistro(id, classe, avancar);
            return Json(new { sucesso = true, mensagem = "Registro atualizado." });
        }

        [HttpPost]
        public ActionResult ResetarFolha(
            int vinculoDeTrabalhoId,
            int unidadeId,
            string mesDeReferencia)
        {
            var mesAno = new MesAno(mesDeReferencia);

            if (mesAno.Inicio.Date > DateTime.Today)
                throw new ApplicationException("O início do período é maior que a data atual.");

            _servicoDeFolhaDePonto.ResetarFolhaDePonto(
                this.DadosDaSessao().OrganizacaoId,
                vinculoDeTrabalhoId,
                unidadeId,
                mesAno);

            return Json(new { sucesso = true, mensagem = "A folha de ponto foi restaurada para a versão inicial." });
        }

        [HttpGet]
        public async Task<IActionResult> ModalSolicitacoesApp(int vinculoDeTrabalhoId, string mesDeReferencia)
        {
            var mesAno = new MesAno(mesDeReferencia);

            if (mesAno.Inicio.Date > DateTime.Today)
                throw new ApplicationException("O início do período é maior que a data atual.");

            var registros = _servicoDeFolhaDePonto.ObtenhaRegistrosDeAplicativo(vinculoDeTrabalhoId, mesAno);
            if (registros.Count() == 0)
                throw new ApplicationException("Nenhum registro encontrado.");

            var html = await RenderizarComoString("_ModalSolicitacoesApp", registros);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult AprovarRegistroAplicativo(int id, int unidadeId, string mesDeReferencia)
        {
            var mesAno = new MesAno(mesDeReferencia);

            if (mesAno.Inicio.Date > DateTime.Today)
                throw new ApplicationException("O início do período é maior que a data atual.");

            _servicoDeFolhaDePonto.AprovarRegistroAplicativo(id, unidadeId, mesAno);
            return Json(new { sucesso = true, mensagem = "Item aprovado." });
        }

        [HttpPost]
        public IActionResult ReprovarRegistroAplicativo(int id)
        {
            _servicoDeFolhaDePonto.ReprovarRegistroAplicativo(id);
            return Json(new { sucesso = true, mensagem = "Item reprovado." });
        }

        [HttpPost]
        public IActionResult SalvarAjusteBancoHoras(int id, string ajuste)
        {
            try
            {
                var ponto = _servicoPontoDoDia.Obtenha(id);
                if (ponto == null)
                    return Json(new { sucesso = false, mensagem = "Registro não encontrado." });

                if (string.IsNullOrWhiteSpace(ajuste))
                {
                    ponto.BancoDeHorasAjuste = null;
                }
                else
                {
                    if (TimeSpan.TryParse(ajuste, out var valor))
                    {
                        ponto.BancoDeHorasAjuste = valor;
                    }
                    else
                    {
                        return Json(new { sucesso = false, mensagem = "Formato de hora inválido." });
                    }
                }

                _servicoPontoDoDia.Atualizar(ponto);

                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        private void ContextoPontoDoDia()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            switch (dadosDaSessao.Perfil)
            {
                case ePerfilDeAcesso.Organizacao:
                    ViewBag.Unidades = new SelectList(
                        _servicoUnidade.ObtenhaLista(c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId && c.Ativa),
                        "Id",
                        "Nome");
                    break;

                case ePerfilDeAcesso.Departamento:
                    ViewBag.Unidades = new SelectList(
                        _servicoDeFolhaDePonto.ObtenhaListaDeUnidadesLotadasNoDepartamento(dadosDaSessao.OrganizacaoId, dadosDaSessao.DepartamentoId.Value),
                        "Codigo",
                        "Descricao");
                    break;
            }
        }

        public static byte[] RelatorioFolhaDePonto(
            SessaoDTO dadosDaSessao,
            VinculoDeTrabalho vinculoDeTrabalho,
            MesAno mesAno,
            List<EventoAnual> eventos,
            List<PontoDoDia> listaDePonto,
            bool impressaoDoServidor = false)
        {
            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.OrganizacaoId,
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(8f);

            document.Add(
                new Div()
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
                    .Add(new Text($"{dadosDaSessao.NomenclaturaServidor.NomenclaturaSingular()}: ").SetBold())
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

            var totalDeColunas = 7;

            if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasTrabalhadas))
                totalDeColunas++;

            if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.CargaHoraria))
                totalDeColunas++;

            if (vinculoDeTrabalho.HorarioDeTrabalho.TipoCargaHoraria != eTipoCargaHoraria.MensalFixa)
            {
                if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasPositivas))
                    totalDeColunas++;

                if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasNegativas))
                    totalDeColunas++;
            }

            if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras && vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.BHSaldo))
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

            if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasTrabalhadas))
            {
                table
                    .AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                        .SetWidth(larguraColunas)
                        .Add(new Paragraph()
                        .Add(new Text("HOR TRA"))
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)));
            }

            if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.CargaHoraria))
            {
                table
                    .AddCell(new Cell()
                        .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                        .SetWidth(larguraColunas)
                        .Add(new Paragraph()
                        .Add(new Text("CAR HOR"))
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)));
            }


            if (vinculoDeTrabalho.HorarioDeTrabalho.TipoCargaHoraria != eTipoCargaHoraria.MensalFixa)
            {
                if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasPositivas))
                {
                    table
                        .AddCell(new Cell()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetWidth(larguraColunas)
                            .Add(new Paragraph()
                            .Add(new Text("HOR POS"))
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)));
                }

                if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasNegativas))
                {
                    table
                        .AddCell(new Cell()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                            .SetWidth(larguraColunas)
                            .Add(new Paragraph()
                            .Add(new Text("HOR NEG"))
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)));
                }
            }

            if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras && vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.BHSaldo))
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

                if (eventoDoDia != null)
                {
                    descricaoDia += $"[{eventoDoDia.Descricao} ({eventoDoDia.Tipo.DescricaoDoEnumerador()})]";
                }

                table.AddCell(new Cell()
                    .Add(new Paragraph()
                    .SetBold()
                    .Add(new Text(descricaoDia))));

                if (dia.DataFutura) 
                {
                    table.AddCell(new Cell(1, totalDeColunas - 1)
                        .SetBackgroundColor(ColorConstants.GRAY, 0.10f)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text("Data Futura"))));
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

                    if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasTrabalhadas))
                    {
                        table
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text(dia.HorasTrabalhadas?.ToString(@"hh\:mm") ?? string.Empty))));
                    }

                    if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.CargaHoraria))
                    {
                        table
                            .AddCell(new Cell()
                                .Add(new Paragraph()
                                .Add(new Text(dia.CargaHoraria?.ToString(@"hh\:mm") ?? string.Empty))));
                    }

                    if (vinculoDeTrabalho.HorarioDeTrabalho.TipoCargaHoraria != eTipoCargaHoraria.MensalFixa)
                    {
                        if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasPositivas))
                        {
                            table
                                .AddCell(new Cell()
                                    .Add(new Paragraph()
                                    .Add(new Text(dia.HorasPositivas?.ToString(@"hh\:mm") ?? string.Empty))));
                        }

                        if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasNegativas))
                        {
                            table
                                .AddCell(new Cell()
                                    .Add(new Paragraph()
                                    .Add(new Text(dia.HorasNegativas?.ToString(@"hh\:mm") ?? string.Empty))));
                        }
                    }

                    if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras && vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.BHSaldo))
                    {
                        var descricaoBH = string.Empty;

                        if ((dia.BancoDeHorasCredito ?? TimeSpan.Zero) > TimeSpan.Zero)
                        {
                            descricaoBH = $"+{(dia.BancoDeHorasCredito.Value.TotalHours >= 1
                           ? $"{(int)dia.BancoDeHorasCredito.Value.TotalHours}:{dia.BancoDeHorasCredito.Value.Minutes.ToString().PadLeft(2, '0')}"
                           : $"00:{dia.BancoDeHorasCredito.Value.Minutes.ToString().PadLeft(2, '0')}")}";
                        }
                        else if ((dia.BancoDeHorasDebito ?? TimeSpan.Zero) > TimeSpan.Zero) 
                        {
                            descricaoBH = $"-{(dia.BancoDeHorasDebito.Value.TotalHours >= 1
                           ? $"{(int)dia.BancoDeHorasDebito.Value.TotalHours}:{dia.BancoDeHorasDebito.Value.Minutes.ToString().PadLeft(2, '0')}"
                           : $"00:{dia.BancoDeHorasDebito.Value.Minutes.ToString().PadLeft(2, '0')}")}";
                        }

                        table
                            .AddCell(new Cell()
                                .SetWidth(larguraColunas)
                                .Add(new Paragraph()
                                .SetTextAlignment(TextAlignment.CENTER)
                                .Add(new Text(descricaoBH ?? string.Empty))));
                    }
                }
            }

            var cargaHorariaMensalFixa = vinculoDeTrabalho.HorarioDeTrabalho.TipoCargaHoraria == eTipoCargaHoraria.MensalFixa;

            var horasTrabalhadas = TimeSpan.FromTicks(listaDePonto.Where(c => !c.DataFutura).Sum(c => (c.HorasTrabalhadas ?? TimeSpan.Zero).Ticks));

            var cargaHoraria = TimeSpan.FromTicks(listaDePonto.Sum(c => (c.CargaHoraria ?? TimeSpan.Zero).Ticks));
            if (cargaHorariaMensalFixa)
            {
                cargaHoraria = TimeSpan.FromHours(vinculoDeTrabalho.HorarioDeTrabalho.CargaHorariaMensalFixa ?? 0);
            }

            var horasPositivas = cargaHorariaMensalFixa ?
                (horasTrabalhadas > cargaHoraria ? horasTrabalhadas - cargaHoraria : TimeSpan.Zero) : 
                TimeSpan.FromTicks(listaDePonto.Where(c => !c.DataFutura).Sum(c => (c.HorasPositivas ?? TimeSpan.Zero).Ticks));

            var horasNegativas = cargaHorariaMensalFixa ?
                (cargaHoraria > horasTrabalhadas ? cargaHoraria - horasTrabalhadas : TimeSpan.Zero) :
                TimeSpan.FromTicks(listaDePonto.Where(c => !c.DataFutura).Sum(c => (c.HorasNegativas ?? TimeSpan.Zero).Ticks));

            var paragrafoTotal = new Paragraph();

            if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasTrabalhadas))
                paragrafoTotal.Add(new Text("HOR TRA:").SetBold()).Add(new Text($" {FormatarTimeSpan(horasTrabalhadas)}\t\t"));

            if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.CargaHoraria))
                paragrafoTotal.Add(new Text("CAR HOR:").SetBold()).Add(new Text($" {FormatarTimeSpan(cargaHoraria)}\t\t"));

            if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasPositivas))
                paragrafoTotal.Add(new Text("HOR POS:").SetBold()).Add(new Text($" {FormatarTimeSpan(horasPositivas)}\t\t"));

            if (vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.HorasNegativas))
                paragrafoTotal.Add(new Text("HOR NEG:").SetBold()).Add(new Text($" {FormatarTimeSpan(horasNegativas)}\t\t"));

            paragrafoTotal.Add(new Text("MÊS SALDO:").SetBold()).Add(new Text($" {FormatarSaldoMes(horasPositivas, horasNegativas)}"));

            if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras && vinculoDeTrabalho.HorarioDeTrabalho.ColunasVisiveis.HasFlag(eColunasDaFolha.BHSaldo))
            {
                var ultimoRegistroBanco = listaDePonto.OrderBy(c => c.Data).LastOrDefault(c => !c.DataFutura);
                paragrafoTotal.Add(new Text("\t\tBH SALDO:").SetBold()).Add(new Text($" {FormatarBancoDeHoras(ultimoRegistroBanco)}"));
            }

            table.AddCell(
                new Cell(1, totalDeColunas)
                .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                .SetTextAlignment(TextAlignment.CENTER)
                .Add(paragrafoTotal));

            var paragrafoLegenda = new Paragraph()
                .Add(new Text("LEGENDA:\t\t").SetBold())
                .Add(new Text($"{eTipoDeRegistroDePeriodo.RegistroAplicativo.DescricaoTipoDeRegistroDoEnumerador()} - Aplicativo\t\t"))
                .Add(new Text($"{eTipoDeRegistroDePeriodo.RegistroManual.DescricaoTipoDeRegistroDoEnumerador()} - Manual\t\t"))
                .Add(new Text($"{eTipoDeRegistroDePeriodo.Automatico.DescricaoTipoDeRegistroDoEnumerador()} - Intervalo Automático\t\t"));

            table.AddCell(
                new Cell(1, totalDeColunas)
                .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                .SetTextAlignment(TextAlignment.CENTER)
                .Add(paragrafoLegenda));

            document.Add(new Div().Add(table));

            if (!impressaoDoServidor)
            {
                Table tabela = new Table(2);
                tabela.SetWidth(UnitValue.CreatePercentValue(100));

                Cell assinatura1 = new Cell().Add(new Paragraph("__________________________________"));
                assinatura1.SetTextAlignment(TextAlignment.CENTER);
                assinatura1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                Cell assinatura2 = new Cell().Add(new Paragraph("__________________________________"));
                assinatura2.SetTextAlignment(TextAlignment.CENTER);
                assinatura2.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                tabela.AddCell(assinatura1);
                tabela.AddCell(assinatura2);

                Cell nome1 = new Cell().Add(new Paragraph("Organização"));
                nome1.SetTextAlignment(TextAlignment.CENTER);
                nome1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                Cell nome2 = new Cell().Add(new Paragraph(vinculoDeTrabalho.Servidor.Nome));
                nome2.SetTextAlignment(TextAlignment.CENTER);
                nome2.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                tabela.AddCell(nome1);
                tabela.AddCell(nome2);

                document.Add(new Div().SetMarginTop(30f).Add(tabela));
            }

            document.Close();

            return stream.ToArray();
        }

        static string FormatarTimeSpan(TimeSpan valor)
        {
            valor = valor.Duration();

            var horas = (int)valor.TotalHours;
            var minutos = valor.Minutes.ToString().PadLeft(2, '0');

            return $"{horas:D2}:{minutos}";
        }

        static string FormatarBancoDeHoras(PontoDoDia ultimoRegistroBanco)
        {
            if ((ultimoRegistroBanco.BancoDeHorasCredito ?? TimeSpan.Zero) > TimeSpan.Zero)
            {
                return "+" + (ultimoRegistroBanco.BancoDeHorasCredito.Value.TotalHours >= 1
                ? $"{(int)ultimoRegistroBanco.BancoDeHorasCredito.Value.TotalHours}:{ultimoRegistroBanco.BancoDeHorasCredito.Value.Minutes.ToString().PadLeft(2, '0')}"
                : $"00:{ultimoRegistroBanco.BancoDeHorasCredito.Value.Minutes.ToString().PadLeft(2, '0')}");
            }
            else if ((ultimoRegistroBanco.BancoDeHorasDebito ?? TimeSpan.Zero) > TimeSpan.Zero)
            {
                return "-" + (ultimoRegistroBanco.BancoDeHorasDebito.Value.TotalHours >= 1
                ? $"{(int)ultimoRegistroBanco.BancoDeHorasDebito.Value.TotalHours}:{ultimoRegistroBanco.BancoDeHorasDebito.Value.Minutes.ToString().PadLeft(2, '0')}"
                : $"00:{ultimoRegistroBanco.BancoDeHorasDebito.Value.Minutes.ToString().PadLeft(2, '0')}");
            }

            return string.Empty;
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

        private string GetMimeType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private static void AdicioneCabecalho(
            Document document,
            int organizacaoId,
            string organizacaoNome)
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
                $"{organizacaoId}.png");

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
                        .Add(new Text($"{organizacaoNome}").SetFontSize(10f)));

            tableCabecalho.AddCell(celulaNome);

            document.Add(new Div().Add(tableCabecalho));
        }
    }
}