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
using Microsoft.Win32;
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
        private readonly IServico<MotivoDeDemissao> _servicoMotivoDeDemissao;
        private readonly IServicoDeFolhaDePonto _servicoDeFolhaDePonto;
        private readonly IServico<Servidor> _servicoServidor;



        public RelatorioController(
            IServicoDeRelatorios servicoDeRelatorios,
            IServico<JustificativaDeAusencia> servicoJustificativa,
            IServico<UnidadeOrganizacional> servicoUnidade,
            IServico<HorarioDeTrabalho> servicoHorario,
            IServico<TipoDoVinculoDeTrabalho> servicoTipo,
            IServico<Escala> servicoEscala,
            IServicoDeFolhaDePonto servicoDeFolhaDePonto,
            IServico<MotivoDeDemissao> servicoMotivoDeDemissao,
            IServico<Servidor> servicoServidor)
        {
            _servicoDeRelatorios = servicoDeRelatorios;
            _servicoJustificativa = servicoJustificativa;
            _servicoUnidade = servicoUnidade;
            _servicoHorario = servicoHorario;
            _servicoTipo = servicoTipo;
            _servicoEscala = servicoEscala;
            _servicoDeFolhaDePonto = servicoDeFolhaDePonto;
            _servicoMotivoDeDemissao = servicoMotivoDeDemissao;
            _servicoServidor = servicoServidor;
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
            int? tipoDeVinculoDeTrabalhoId,
            int? listaPersonalizadaId)
        {
            var relatorio = RelatorioListaDeServidores(
                unidadeId,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId,
                listaPersonalizadaId);

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

        [HttpGet]
        public IActionResult ServidoresDemitidos()
        {
            try
            {
                var dadosDaSessao = this.DadosDaSessao();
                int organizacaoId = dadosDaSessao.OrganizacaoId;

                ViewBag.MotivosDeDemissao = new SelectList(
                    _servicoMotivoDeDemissao
                    .ObtenhaLista(c =>
                        c.OrganizacaoId == organizacaoId && c.Ativo)
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
        public IActionResult ProcessarServidoresDemitidos(
            int? unidadeLotacaoId,
            DateTime? inicio,
            DateTime? fim,
            int? motivoDeDemissaoId)
        {
            var relatorio = ObtenhaRelatorioServidoresDemitidos(
                    unidadeLotacaoId,
                    inicio,
                    fim,
                    motivoDeDemissaoId);

            var nomeArquivo = $"{HttpContext.NomenclaturaServidores()} Demitidos.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpPost]
        public IActionResult ProcessarRelatorioFichaDoServidor(int servidorId)
        {
            var relatorio = ObtenhaRelatorioFichaDoServidor(servidorId);
            var nomeArquivo = "Ficha do Servidor.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult ConferenciaDePontoDiario()
        {
            try
            {
                var dadosDaSessao = this.DadosDaSessao();
                int organizacaoId = dadosDaSessao.OrganizacaoId;

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
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public ActionResult ProcessarConferenciaDePontoDiario(
            DateTime? data,
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            if (!data.HasValue || data == DateTime.MinValue)
                throw new ApplicationException("A data deve ser informada.");
            else if (data > DateTime.Today)
                throw new ApplicationException("Não é possível processar a conferência para datas futuras.");

            var sessao = HttpContext.DadosDaSessao();
            if (sessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeId = sessao.UnidadeOrganizacionais.First();

            if (!unidadeId.HasValue)
                throw new ApplicationException("A unidade organizacional deve ser informada.");

            var relatorio = ObtenhaRelatorioConferenciaDePonto(
                unidadeId.Value,
                data.Value,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId);

            var nomeArquivo = $"Conferência de Ponto {data.Value.ToString("dd-MM-yyyy")}.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult ServidoresPorLotacao()
        {
            try
            {
                var dadosDaSessao = this.DadosDaSessao();
                int organizacaoId = dadosDaSessao.OrganizacaoId;

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
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public ActionResult ProcessarServidoresPorLotacao(
            DateTime? entrada,
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var sessao = HttpContext.DadosDaSessao();
            if (sessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeId = sessao.UnidadeOrganizacionais.First();

            var relatorio = ObtenhaRelatorioServidoresPorLotacao(
                unidadeId,
                entrada,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId);

            var nomeArquivo = $"{HttpContext.NomenclaturaServidores()} por Lotação.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult Absenteismo()
        {
            try
            {
                var dadosDaSessao = this.DadosDaSessao();
                int organizacaoId = dadosDaSessao.OrganizacaoId;

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
        public ActionResult ProcessarAbsenteismo(
            DateTime? inicio,
            DateTime? fim,
            int? unidadeId)
        {
            if (!inicio.HasValue || !fim.HasValue)
                throw new ApplicationException("O período inicial e final devem ser informados.");

            if (fim < inicio)
                throw new ApplicationException("A data final não pode ser menor que a inicial.");

            var sessao = HttpContext.DadosDaSessao();
            if (sessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeId = sessao.UnidadeOrganizacionais.First();

            var relatorio = ObtenhaRelatorioAbsenteismo(
                unidadeId,
                inicio.Value,
                fim.Value);

            var nomeArquivo = $"Relatório de Absenteísmo.pdf";

            return Json(new
            {
                sucesso = true,
                fileName = nomeArquivo,
                base64 = Convert.ToBase64String(relatorio),
                mimeType = GetMimeType(nomeArquivo)
            });
        }

        [HttpGet]
        public IActionResult AuditoriaDeAusencias()
        {
            try
            {
                var dadosDaSessao = this.DadosDaSessao();
                int organizacaoId = dadosDaSessao.OrganizacaoId;

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
        public IActionResult ProcessarAuditoriaDeAusencias(
            int? unidadeLotacaoId,
            DateTime? inicio,
            DateTime? fim)
        {
            var relatorio = ObtenhaRelatorioDeAuditoriaDeAusencias(
                    unidadeLotacaoId,
                    inicio,
                    fim);

            var nomeArquivo = "Auditoria de Ausências.pdf";

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

        private byte[] ObtenhaRelatorioServidoresDemitidos(
            int? unidadeLotacaoId,
            DateTime? inicio,
            DateTime? fim,
            int? motivoDeDemissaoId)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeLotacaoId = dadosDaSessao.UnidadeOrganizacionais.First();

            var registros = _servicoDeRelatorios.ObtenhaServidoresDemitidosPorPeriodo(
                dadosDaSessao.OrganizacaoId,
                unidadeLotacaoId,
                inicio,
                fim,
                motivoDeDemissaoId,
                dadosDaSessao.DepartamentoId);

            if (registros.Count == 0)
                throw new ApplicationException("Nenhuma demissão encontrada para os filtros informados.");

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
                    .Add($"{HttpContext.NomenclaturaServidores()} Demitidos")));

            var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    35f,
                    10f,
                    25f,
                    30f
                })).UseAllAvailableWidth();

            table
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text($"{HttpContext.NomenclaturaServidor()}"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Data"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Motivo"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Observações"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            foreach (var registro in registros.OrderBy(c => c.DataDaDemissao).ThenBy(c => c.PessoaNome))
            {
                table.AddCell(new Cell()
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph()
                    .Add(new Text(registro.PessoaNome))));

                table.AddCell(new Cell()
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph()
                    .Add(new Text($"{registro.DataDaDemissao.ToString("dd/MM/yyyy")}"))));

                table.AddCell(new Cell()
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Text($"{registro.MotivoDeDemissaoDescricao}"))));

                table.AddCell(new Cell()
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Text($"{registro.Observacoes}"))));
            }

            document.Add(new Div().Add(table));

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
            int? tipoDeVinculoDeTrabalhoId,
            int? listaPersonalizadaId)
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

                var vigencia = vinculo.HorarioDeTrabalho.ObtenhaVigenciaDoMes(mesAno);

                var cargaHorariaMensalFixa = vigencia.TipoCargaHoraria == eTipoCargaHoraria.MensalFixa;

                var horasTrabalhadas = TimeSpan.FromTicks(listaDePonto.Where(c => !c.DataFutura).Sum(c => (c.HorasTrabalhadas ?? TimeSpan.Zero).Ticks));

                var cargaHoraria = TimeSpan.FromTicks(listaDePonto.Sum(c => (c.CargaHoraria ?? TimeSpan.Zero).Ticks));
                if (cargaHorariaMensalFixa)
                {
                    cargaHoraria = TimeSpan.FromHours(vigencia.CargaHorariaMensalFixa ?? 0);
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

        private byte[] ObtenhaRelatorioFichaDoServidor(int servidorId)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            var servidor = _servicoServidor.Obtenha(servidorId);

            if (servidor == null)
                throw new ApplicationException("Servidor não encontrado.");

            ImageData imageData = ObterImagemServidor(servidor.Id, dadosDaSessao.OrganizacaoId);

            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.OrganizacaoId,
                dadosDaSessao.OrganizacaoNome);

            document.Add(new Paragraph($"Ficha Cadastral do {HttpContext.NomenclaturaServidor()}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15f)
                .SetBold()
                .SetMarginBottom(15));

            var tableInfoGeral = new Table(new float[] { 1f, 1f, 3f })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(10);

            var cellFoto = new Cell(5, 1)
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.TOP);

            if (imageData != null)
            {
                cellFoto.Add(new Image(imageData)
                    .SetWidth(80).SetHeight(80).SetAutoScale(false)
                    .SetMargins(0, 0, 0, 0).SetPadding(0));
            }
            else
            {
                cellFoto.Add(new Paragraph("Sem Foto").SetFontSize(10).SetItalic().SetWidth(80));
            }
            tableInfoGeral.AddCell(cellFoto);

            // Título da Seção (ao lado da foto)
            tableInfoGeral.AddCell(new Cell(1, 2)
                .Add(new Paragraph("Dados de Identificação").SetBold().SetFontSize(10))
                .SetBackgroundColor(ColorConstants.GRAY).SetFontColor(ColorConstants.WHITE)
                .SetBorder(Border.NO_BORDER).SetPadding(2));

            // Linhas de Dados (ao lado da foto)
            tableInfoGeral.AddCell(CriarCelulaLabel("Nome:").SetPadding(2));
            tableInfoGeral.AddCell(CriarCelulaValor(servidor.Pessoa?.Nome).SetPadding(2));

            tableInfoGeral.AddCell(CriarCelulaLabel("Nome Social:").SetPadding(2));
            tableInfoGeral.AddCell(CriarCelulaValor(servidor.Pessoa?.NomeSocial).SetPadding(2));

            tableInfoGeral.AddCell(CriarCelulaLabel("CPF:").SetPadding(2));
            tableInfoGeral.AddCell(CriarCelulaValor(servidor.Pessoa?.Cpf).SetPadding(2));

            tableInfoGeral.AddCell(CriarCelulaLabel("Data Nasc.:").SetPadding(2));
            tableInfoGeral.AddCell(CriarCelulaValor(servidor.Pessoa?.DataDeNascimento.ToString("dd/MM/yyyy")).SetPadding(2));

            document.Add(tableInfoGeral);

            // 7. Seção de Endereço
            var tableEndereco = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 1, 2 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(10);

            tableEndereco.AddCell(CriarCelulaTituloSecao("Endereço", 4));
            tableEndereco.AddCell(CriarCelulaLabel("Endereço:"));
            tableEndereco.AddCell(CriarCelulaValor(servidor.Pessoa?.Endereco?.ToString(), 3));
            document.Add(tableEndereco);

            // 8. Seção de Vínculos (Loop)
            if (servidor.VinculosDeTrabalho == null || !servidor.VinculosDeTrabalho.Any())
            {
                document.Add(new Paragraph("Servidor não possui vínculos de trabalho cadastrados.")
                    .SetItalic().SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10));
            }
            else
            {
                // Itera sobre cada vínculo
                foreach (var vinculo in servidor.VinculosDeTrabalho.OrderBy(v => v.Inicio))
                {
                    var tableVinculo = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 1, 2 }))
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginBottom(10);

                    string tituloVinculo = $"Vínculo: {vinculo.Matricula} ({vinculo.TipoDoVinculoDeTrabalho?.Descricao ?? "N/A"})";
                    tableVinculo.AddCell(CriarCelulaTituloSecao(tituloVinculo, 4));

                    tableVinculo.AddCell(CriarCelulaLabel("Matrícula:"));
                    tableVinculo.AddCell(CriarCelulaValor(vinculo.Matricula));
                    tableVinculo.AddCell(CriarCelulaLabel("Situação:"));
                    tableVinculo.AddCell(CriarCelulaValor(vinculo.Situacao.DescricaoDoEnumerador()));

                    tableVinculo.AddCell(CriarCelulaLabel("Tipo:"));
                    tableVinculo.AddCell(CriarCelulaValor(vinculo.TipoDoVinculoDeTrabalho?.Descricao, 3));

                    tableVinculo.AddCell(CriarCelulaLabel("Função:"));
                    tableVinculo.AddCell(CriarCelulaValor(vinculo.Funcao?.Descricao, 3));

                    tableVinculo.AddCell(CriarCelulaLabel("Departamento:"));
                    tableVinculo.AddCell(CriarCelulaValor(vinculo.Departamento?.Descricao, 3));

                    tableVinculo.AddCell(CriarCelulaLabel("Início:"));
                    tableVinculo.AddCell(CriarCelulaValor(vinculo.Inicio.ToString("dd/MM/yyyy")));
                    tableVinculo.AddCell(CriarCelulaLabel("Fim:"));
                    tableVinculo.AddCell(CriarCelulaValor(vinculo.Fim?.ToString("dd/MM/yyyy")));

                    if (vinculo.Lotacoes != null && vinculo.Lotacoes.Any())
                    {
                        tableVinculo.AddCell(CriarCelulaSubTitulo("Lotações deste Vínculo", 4));

                        foreach (var lotacao in vinculo.Lotacoes.OrderBy(l => l.Entrada))
                        {
                            tableVinculo.AddCell(CriarCelulaLabel("Unidade Organizacional:"));
                            tableVinculo.AddCell(CriarCelulaValor(lotacao.UnidadeOrganizacional?.Nome, 3));

                            tableVinculo.AddCell(CriarCelulaLabel("Entrada:"));
                            tableVinculo.AddCell(CriarCelulaValor(lotacao.Entrada.ToString("dd/MM/yyyy")));
                            tableVinculo.AddCell(CriarCelulaLabel("Saída:"));
                            tableVinculo.AddCell(CriarCelulaValor(lotacao.Saida?.ToString("dd/MM/yyyy")));
                        }
                    }

                    document.Add(tableVinculo);
                }
            }

            if (servidor.ListaDeObservacoes != null && servidor.ListaDeObservacoes.Any(c => c.Ativa))
            {
                var tableObservacoes = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 1, 2 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(10);

                foreach (var observacao in servidor.ListaDeObservacoes.Where(c => c.Ativa).OrderBy(c => c.CadastradaEm))
                {
                    tableObservacoes.AddCell(CriarCelulaTituloSecao("Observações", 4));
                    tableObservacoes.AddCell(CriarCelulaValor(observacao.Texto, 4));
                }
                
                document.Add(tableObservacoes);
            }

            document.Close();
            return stream.ToArray();
        }

        private byte[] ObtenhaRelatorioConferenciaDePonto(
            int unidadeOrganizacionalId,
            DateTime data,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var dadosDaSessao = this.DadosDaSessao();

            var registros = _servicoDeRelatorios.ObtenhaListaDeDadosParaConferenciaDePonto(
                dadosDaSessao.OrganizacaoId,
                unidadeOrganizacionalId,
                data,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId,
                dadosDaSessao.Perfil == ePerfilDeAcesso.Departamento ? dadosDaSessao.DepartamentoId : null);

            if (registros.Count == 0)
                throw new ApplicationException($"Nenhum registro encontrado para os filtros informados.");

            var nomeUnidade = _servicoUnidade.Obtenha(unidadeOrganizacionalId).Nome;

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.OrganizacaoId,
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(8);

            document.Add(
                new Div()
                .SetMarginBottom(3)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add($"Conferência de Ponto: {data.ToString("dd/MM/yyyy")}\n{nomeUnidade}")));

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(6f)
                    .Add($"Consulta efetuada em: {DateTime.Now.ToString("dd/MM/yyyy")} às {DateTime.Now.ToString("HH:mm")}")));

            var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    35f,
                    30f, // ORIGEM
                    5f,  // HORA
                    10f, // SITUACAO
                    20f, // LAT/LONG
                })).UseAllAvailableWidth();

            table
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text(dadosDaSessao.NomenclaturaServidor.NomenclaturaSingular()))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Origem"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Hora"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Situação"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                    .Add(new Paragraph()
                    .Add(new Text("Lat/Long"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

            foreach (var grupoServidor in registros
                .OrderBy(c => c.ServidorNome)
                .GroupBy(c => c.ServidorId))
            {
                var servidor = grupoServidor.First();

                var rowSpan = grupoServidor.Count();

                table.AddCell(new Cell(rowSpan, 1)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph()
                    .Add(new Text($"{servidor.ServidorNome}\nCPF:{servidor.ServidorCpf}"))));

                foreach (var registro in grupoServidor.OrderBy(c => c.DataHora))
                {
                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{registro.Origem}"))));

                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{registro.DataHora.ToString("HH:mm")}"))));

                    var situacaoTexto = registro.Situacao switch
                    {
                        eSituacaoRegistroAplicativo.AguardandoAvaliacao => "AGD\nAVA",
                        eSituacaoRegistroAplicativo.Aprovado => "APR",
                        eSituacaoRegistroAplicativo.Reprovado => "REP",
                        _ => "-"
                    };
                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{situacaoTexto}"))));

                    var textoLatLong = (!string.IsNullOrWhiteSpace(registro.Latitude) && !string.IsNullOrWhiteSpace(registro.Longitude)) ?
                        $"Lat: {servidor.Latitude}\nLong:{servidor.Longitude}" :
                        "";

                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{textoLatLong}"))));
                }
            }

            document.Add(new Div().Add(table));

            document.Close();
            return stream.ToArray();
        }

        private byte[] ObtenhaRelatorioServidoresPorLotacao(
            int? unidadeOrganizacionalId,
            DateTime? entrada,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var dadosDaSessao = this.DadosDaSessao();

            var registros = _servicoDeRelatorios.ObtenhaListaDeDadosPorLotacao(
                dadosDaSessao.OrganizacaoId,
                unidadeOrganizacionalId,
                entrada,
                horarioDeTrabalhoId,
                tipoDeVinculoDeTrabalhoId,
                dadosDaSessao.Perfil == ePerfilDeAcesso.Departamento ? dadosDaSessao.DepartamentoId : null);

            if (registros.Count == 0)
                throw new ApplicationException($"Nenhum registro encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.OrganizacaoId,
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(8);

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .Add($"{HttpContext.NomenclaturaServidores()} por Lotação")));

            var grupoUnidade = registros
                .OrderBy(c => c.UnidadeNome)
                .GroupBy(c => c.UnidadeId);

            var ultimaUnidade = grupoUnidade.Last();
            foreach (var unidade in grupoUnidade)
            {
                var table = new Table(UnitValue.CreatePercentArray(new[]
                {
                    25f, // NOME
                    15f, // CPF
                    10f, // MATRICULA
                    15f, // TIPO
                    15f, // HORARIO
                    10f, // ENTRADA
                    10f  // TEMPO
                })).UseAllAvailableWidth();

                table
                    .AddCell(new Cell(1, 7)
                    .Add(new Paragraph()
                            .Add(new Text($"{unidade.First().UnidadeNome}\nEndereço: {unidade.First().EnderecoCompleto}")))
                            .SetBold()
                            .SetBackgroundColor(ColorConstants.GRAY, 0.5f)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));

                table
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text(dadosDaSessao.NomenclaturaServidor.NomenclaturaSingular()))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("CPF"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Matrícula"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Tipo"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Horário"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Entrada"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)))
                .AddCell(new Cell()
                    .SetBackgroundColor(ColorConstants.GRAY, 0.25f)
                    .Add(new Paragraph()
                    .Add(new Text("Tempo\nServiço"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)));

                foreach (var servidor in unidade
                    .OrderBy(c => c.ServidorNome)
                    .ThenBy(c => c.Entrada))
                {
                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .Add(new Text($"{servidor.ServidorNome}"))));

                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{servidor.ServidorCpf}"))));

                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{servidor.VinculoMatricula}"))));

                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{servidor.TipoVinculo}"))));

                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{servidor.HorarioDeTrabalho}"))));

                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{servidor.Entrada.ToString("dd/MM/yyyy")}"))));

                    table.AddCell(new Cell()
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Text($"{CalcularTempoServico(servidor.Entrada)}"))));
                }

                document.Add(new Div().Add(table));

                if (ultimaUnidade.Key != unidade.Key)
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
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

        /// <summary>
        /// Cria uma célula de "Rótulo" (Label) padronizada para as tabelas.
        /// </summary>
        private Cell CriarCelulaLabel(string texto)
        {
            return new Cell()
                .Add(new Paragraph(texto ?? "")
                    .SetBold()
                    .SetFontSize(8))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY, 0.5f) // Fundo cinza claro
                .SetBorderTop(Border.NO_BORDER)
                .SetBorderLeft(Border.NO_BORDER)
                .SetBorderRight(Border.NO_BORDER)
                .SetPadding(4)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        /// <summary>
        /// Cria uma célula de "Valor" (Value) padronizada para as tabelas.
        /// </summary>
        private Cell CriarCelulaValor(string texto, int colspan = 1)
        {
            return new Cell(1, colspan)
                .Add(new Paragraph(string.IsNullOrWhiteSpace(texto) ? "Não informado" : texto)
                    .SetFontSize(9))
                .SetBorderTop(Border.NO_BORDER)
                .SetBorderLeft(Border.NO_BORDER)
                .SetBorderRight(Border.NO_BORDER)
                .SetPadding(4)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        /// <summary>
        /// Cria uma célula de "Título de Seção" que ocupa várias colunas.
        /// </summary>
        private Cell CriarCelulaTituloSecao(string texto, int colspan)
        {
            return new Cell(1, colspan)
                .Add(new Paragraph(texto)
                    .SetBold()
                    .SetFontSize(10))
                .SetBackgroundColor(ColorConstants.GRAY) // Fundo cinza escuro
                .SetFontColor(ColorConstants.WHITE)     // Texto branco
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .SetMarginTop(8); // Espaçamento entre seções
        }

        private Cell CriarCelulaSubTitulo(string texto, int colspan)
        {
            return new Cell(1, colspan)
                .Add(new Paragraph(texto)
                    .SetBold()
                    .SetFontSize(9))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY, 0.7f)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(3)
                .SetMarginTop(5);
        }

        private ImageData ObterImagemServidor(int servidorId, int organizacaoId)
        {
            var caminhoFoto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "organizacao", $"{organizacaoId}", $"{servidorId}.png");

            if (!System.IO.File.Exists(caminhoFoto))
            {
                caminhoFoto = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "pessoas", "sem-foto.png");
            }

            if (System.IO.File.Exists(caminhoFoto))
            {
                try
                {
                    byte[] fotoBytes = System.IO.File.ReadAllBytes(caminhoFoto);
                    return ImageDataFactory.Create(fotoBytes);
                }
                catch (IOException)
                {
                    return null;
                }
            }

            return null;
        }

        public string CalcularTempoServico(DateTime dataAdmissao)
        {
            var dataAtual = DateTime.Now.Date;

            if (dataAdmissao.Date > dataAtual)
                return string.Empty;

            var anos = dataAtual.Year - dataAdmissao.Year;
            var meses = dataAtual.Month - dataAdmissao.Month;
            var dias = dataAtual.Day - dataAdmissao.Day;

            if (dias < 0)
            {
                meses--;
                var dataMesAnterior = dataAtual.AddMonths(-1);
                dias += DateTime.DaysInMonth(dataMesAnterior.Year, dataMesAnterior.Month);
            }

            if (meses < 0)
            {
                anos--;
                meses += 12;
            }

            if (anos >= 1)
            {
                var strAnos = anos == 1 ? "1 ano" : $"{anos} anos";
                var strMeses = meses == 1 ? "1 mês" : $"{meses} meses";
                return $"{strAnos} e {strMeses}";
            }

            if (meses >= 1)
            {
                var strMeses = meses == 1 ? "1 mês" : $"{meses} meses";
                var strDias = dias == 1 ? "1 dia" : $"{dias} dias";
                return $"{strMeses} e {strDias}";
            }

            return dias == 1 ? "1 dia" : $"{dias} dias";
        }

        private byte[] ObtenhaRelatorioAbsenteismo(
            int? unidadeId,
            DateTime inicio,
            DateTime fim)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            var absenteismos = _servicoDeRelatorios.ObtenhaRelatorioDeAbsenteismo(
                dadosDaSessao.OrganizacaoId,
                unidadeId,
                inicio,
                fim,
                dadosDaSessao.DepartamentoId);

            if (absenteismos.Count == 0)
                throw new ApplicationException("Nenhum registro de absenteísmo encontrado para o período informado.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document, 
                dadosDaSessao.OrganizacaoId, 
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(9);

            document.Add(
                new Div()
                .SetMarginBottom(5)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14f)
                    .SetBold()
                    .Add("Relatório de Absenteísmo e Atrasos")));

            document.Add(
                new Paragraph($"Período: {inicio.ToShortDateString()} a {fim.ToShortDateString()}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10f));

            var table = new Table(UnitValue.CreatePercentArray(new[] { 30f, 15f, 20f, 10f, 15f, 10f })).UseAllAvailableWidth();

            table.AddHeaderCell(new Cell().Add(new Paragraph("Servidor")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Matrícula/Vínculo")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Departamento")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Data")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Classificação")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Horas (DB)")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));

            foreach (var item in absenteismos)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.NomeServidor)));
                table.AddCell(new Cell().Add(new Paragraph(item.Matricula)));
                table.AddCell(new Cell().Add(new Paragraph(item.Departamento ?? "-")));
                table.AddCell(new Cell().Add(new Paragraph(item.Data.ToShortDateString())));
                table.AddCell(new Cell().Add(new Paragraph(item.TipoAusencia)));
                table.AddCell(new Cell().Add(new Paragraph(item.TotalAtrasoOuFalta)).SetTextAlignment(TextAlignment.CENTER));
            }

            document.Add(new Div().SetMarginTop(10).Add(table));

            document.Close();
            return stream.ToArray();
        }

        private byte[] ObtenhaRelatorioDeAuditoriaDeAusencias(
            int? unidadeLotacaoId,
            DateTime? inicio,
            DateTime? fim)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.UnidadeOrganizacional)
                unidadeLotacaoId = dadosDaSessao.UnidadeOrganizacionais.First();

            var registros = _servicoDeRelatorios.ObtenhaRelatorioDeAuditoriaDeAusencias(
                dadosDaSessao.OrganizacaoId,
                inicio,
                fim,
                unidadeLotacaoId);

            if (registros.Count == 0)
                throw new ApplicationException("Nenhum registro de auditoria encontrado para os filtros informados.");

            var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            AdicioneCabecalho(
                document,
                dadosDaSessao.OrganizacaoId,
                dadosDaSessao.OrganizacaoNome);

            document.SetFontSize(9);

            var subTitulo = inicio.HasValue && fim.HasValue
                ? $"Período Analisado: {inicio.Value.ToShortDateString()} a {fim.Value.ToShortDateString()}"
                : "Período Analisado: Completo";

            document.Add(
                new Div()
                .SetMarginBottom(10)
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(15f)
                    .SetBold()
                    .Add("Auditoria de Ausências / Afastamentos"))
                .Add(new Paragraph()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10f)
                    .Add(subTitulo)));

            var table = new Table(UnitValue.CreatePercentArray(new[] { 15f, 15f, 10f, 10f, 15f, 15f, 20f })).UseAllAvailableWidth();

            table.AddHeaderCell(new Cell().Add(new Paragraph("Servidor")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Justificativa")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Início")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fim")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Ação")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Data/Hora (Log)")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Operador")).SetBold().SetBackgroundColor(ColorConstants.LIGHT_GRAY));

            foreach (var item in registros)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.ServidorNome ?? string.Empty)));
                table.AddCell(new Cell().Add(new Paragraph(item.Justificativa ?? string.Empty)));
                table.AddCell(new Cell().Add(new Paragraph(item.InicioAfastamento.ToShortDateString())).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(item.FimAfastamento?.ToShortDateString() ?? "-")).SetTextAlignment(TextAlignment.CENTER));
                
                var descFormatada = string.IsNullOrWhiteSpace(item.Descricao) ? item.Acao : $"{item.Acao} ({item.Descricao})";
                table.AddCell(new Cell().Add(new Paragraph(descFormatada)));
                
                table.AddCell(new Cell().Add(new Paragraph(item.DataHoraAcao.ToString("dd/MM/yyyy HH:mm"))).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(item.OperadorNome ?? string.Empty)));
            }

            document.Add(table);
            document.Close();

            return stream.ToArray();
        }
    }
}