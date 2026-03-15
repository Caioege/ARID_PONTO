using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.Models;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class HorarioDeTrabalhoController : Controller
    {
        private readonly IServico<HorarioDeTrabalho> _servico;
        private readonly IServico<HorarioDeTrabalhoDia> _servicoDia;
        private readonly IServico<RegraHoraExtra> _servicoRegraHE;
        private readonly IServico<FaixaHoraExtra> _servicoFaixaHE;
        private readonly IServico<HorarioDeTrabalhoVigencia> _servicoVigencia;

        public HorarioDeTrabalhoController(
            IServico<HorarioDeTrabalho> servico,
            IServico<HorarioDeTrabalhoDia> servicoDia,
            IServico<RegraHoraExtra> servicoRegraHE,
            IServico<FaixaHoraExtra> servicoFaixaHE,
            IServico<HorarioDeTrabalhoVigencia> servicoVigencia)
        {
            _servico = servico;
            _servicoDia = servicoDia;
            _servicoRegraHE = servicoRegraHE;
            _servicoFaixaHE = servicoFaixaHE;
            _servicoVigencia = servicoVigencia;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<HorarioDeTrabalho> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<HorarioDeTrabalho> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View("_TabelaPaginada", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Adicionar()
        {
            try
            {
                var orgId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                var vm = new HorarioDeTrabalhoCadastroVM
                {
                    Ativo = true,
                    VigenciaInicio = DateTime.Today.Date,

                    UtilizaCincoPeriodos = false,
                    UtilizaBancoDeHoras = false,
                    InicioBancoDeHoras = null,
                    TipoCargaHoraria = eTipoCargaHoraria.EntradaSaida,
                    CargaHorariaMensalFixa = null,
                    IntervaloAutomatico = eIntervaloAutomatico.NaoUtiliza,
                    ToleranciaDiariaEmMinutos = 0,
                    ColunasVisiveis = eColunasDaFolha.Todas,

                    ConsiderarFacultativoComoFeriadoHoraExtra = false,
                    ToleranciaDsrEmMinutos = 0,

                    Dias = DiasPadrao(orgId),
                    RegrasHoraExtra = MonteRegrasPadraoParaTela(new())
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }


        [HttpGet]
        public IActionResult Alterar(int id, int? vigenciaId)
        {
            try
            {
                var orgId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                var horario = _servico.Obtenha(id);
                if (horario == null || horario.OrganizacaoId != orgId)
                    throw new ApplicationException("Horário inválido.");

                var vigencias = _servicoVigencia.ObtenhaLista(v => v.HorarioDeTrabalhoId == id && v.OrganizacaoId == orgId)
                    .OrderByDescending(v => v.VigenciaInicio)
                    .ToList();

                var vigSelecionada = vigenciaId.HasValue
                    ? vigencias.FirstOrDefault(v => v.Id == vigenciaId.Value)
                    : vigencias.FirstOrDefault();

                if (vigSelecionada == null)
                    throw new ApplicationException("Horário sem vigência cadastrada. Execute o backfill.");

                var dias = _servicoDia.ObtenhaLista(d => d.HorarioDeTrabalhoVigenciaId == vigSelecionada.Id && d.OrganizacaoId == orgId)
                    .OrderBy(d => d.DiaDaSemana)
                    .ToList();
                dias = NormalizarDias(orgId, dias);

                var regrasCarregadas = _servicoRegraHE.ObtenhaLista(r =>
                        r.OrganizacaoId == orgId &&
                        r.HorarioDeTrabalhoVigenciaId == vigSelecionada.Id)
                    .ToList();

                foreach (var r in regrasCarregadas)
                    r.Faixas = _servicoFaixaHE.ObtenhaLista(f => f.RegraHoraExtraId == r.Id).OrderBy(f => f.Ordem).ToList();

                var regrasTela = MonteRegrasPadraoParaTela(regrasCarregadas);

                var vm = MapearParaVM(horario, vigSelecionada, dias, regrasTela);

                ViewBag.Vigencias = vigencias
                    .OrderByDescending(v => v.VigenciaInicio)
                    .Select(v => new { v.Id, Data = v.VigenciaInicio.ToString("yyyy-MM-dd"), Texto = v.VigenciaInicio == DateTime.MinValue ? "Início" : v.VigenciaInicio.ToString("dd/MM/yyyy") })
                    .ToList();

                return View(vm);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult CalculaCargaHorariaDoDia(HorarioDeTrabalhoDia dia)
        {
            var cargaHoraria = dia.CalculeCargaHorariaTotal(false);
            return Json(new { sucesso = true, cargaHoraria = cargaHoraria?.ToString(@"hh\:mm") });
        }

        [HttpPost]
        public IActionResult Salvar(HorarioDeTrabalhoCadastroVM vm)
        {
            try
            {
                var orgId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                if (vm.TipoCargaHoraria == eTipoCargaHoraria.MensalFixa && vm.UtilizaBancoDeHoras)
                    throw new ApplicationException("Carga Horária Mensal Fixa não pode ter Banco de Horas habilitado.");

                HorarioDeTrabalho horario;
                if (vm.Id == 0)
                {
                    horario = new HorarioDeTrabalho
                    {
                        OrganizacaoId = orgId,
                        Sigla = vm.Sigla,
                        Descricao = vm.Descricao,
                        Ativo = vm.Ativo
                    };

                    vm.Id = _servico.Adicionar(horario);
                }
                else
                {
                    horario = _servico.Obtenha(vm.Id);
                    if (horario == null || horario.OrganizacaoId != orgId)
                        throw new ApplicationException("Horário inválido.");

                    horario.Sigla = vm.Sigla;
                    horario.Descricao = vm.Descricao;
                    horario.Ativo = vm.Ativo;

                    _servico.Atualizar(horario);
                }

                HorarioDeTrabalhoVigencia vig;
                if (vm.VigenciaId == 0)
                {
                    var existe = _servicoVigencia.Obtenha(v =>
                        v.OrganizacaoId == orgId &&
                        v.HorarioDeTrabalhoId == vm.Id &&
                        v.VigenciaInicio.Date == vm.VigenciaInicio.Date);

                    if (existe != null)
                        throw new ApplicationException("Já existe uma vigência cadastrada com essa data.");

                    vig = new HorarioDeTrabalhoVigencia
                    {
                        OrganizacaoId = orgId,
                        HorarioDeTrabalhoId = vm.Id,
                        VigenciaInicio = vm.VigenciaInicio.Date
                    };

                    AplicarCamposVigencia(vm, vig);

                    vm.VigenciaId = _servicoVigencia.Adicionar(vig);
                }
                else
                {
                    vig = _servicoVigencia.Obtenha(vm.VigenciaId);
                    if (vig == null || vig.OrganizacaoId != orgId || vig.HorarioDeTrabalhoId != vm.Id)
                        throw new ApplicationException("Vigência inválida.");

                    AplicarCamposVigencia(vm, vig);
                    _servicoVigencia.Atualizar(vig);
                }

                var diasAtuais = _servicoDia.ObtenhaLista(d => d.OrganizacaoId == orgId && d.HorarioDeTrabalhoVigenciaId == vm.VigenciaId);
                foreach (var d in diasAtuais)
                    _servicoDia.Remover(d, false);

                var dias = NormalizarDias(orgId, vm.Dias ?? new());
                foreach (var d in dias)
                {
                    d.Id = 0;
                    d.OrganizacaoId = orgId;
                    d.HorarioDeTrabalhoVigenciaId = vm.VigenciaId;
                    _servicoDia.Adicionar(d);
                }

                var regrasHoraExtra = vm.RegrasHoraExtra ?? new();
                SalvarConfiguracaoHorasExtras(orgId, vm.VigenciaId, regrasHoraExtra);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = vm.Id, vigenciaId = vm.VigenciaId });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Remover(int id)
        {
            try
            {
                var orgId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                var horario = _servico.Obtenha(id);
                if (horario == null || horario.OrganizacaoId != orgId)
                    throw new ApplicationException("Horário inválido.");

                // Remove tudo que depende do horário (vigências -> dias -> regras -> faixas)
                var vigencias = _servicoVigencia.ObtenhaLista(v =>
                    v.OrganizacaoId == orgId &&
                    v.HorarioDeTrabalhoId == id);

                foreach (var vig in vigencias)
                {
                    // Dias da vigência
                    var dias = _servicoDia.ObtenhaLista(d =>
                        d.OrganizacaoId == orgId &&
                        d.HorarioDeTrabalhoVigenciaId == vig.Id);

                    foreach (var d in dias)
                        _servicoDia.Remover(d, false);

                    // Regras HE da vigência
                    var regras = _servicoRegraHE.ObtenhaLista(r =>
                        r.OrganizacaoId == orgId &&
                        r.HorarioDeTrabalhoVigenciaId == vig.Id);

                    foreach (var regra in regras)
                    {
                        // Faixas da regra
                        var faixas = _servicoFaixaHE.ObtenhaLista(f => f.RegraHoraExtraId == regra.Id);
                        foreach (var fx in faixas)
                            _servicoFaixaHE.Remover(fx, false);

                        _servicoRegraHE.Remover(regra, false);
                    }

                    // Remove a própria vigência
                    _servicoVigencia.Remover(vig, false);
                }

                // Remove o mestre
                _servico.Remover(horario);

                return Json(new { sucesso = true, mensagem = "O horário de trabalho foi removido." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ClonarVigencia(int horarioId, int vigenciaBaseId, string vigenciaInicio)
        {
            try
            {
                var orgId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                var horario = _servico.Obtenha(horarioId);
                if (horario == null || horario.OrganizacaoId != orgId)
                    throw new ApplicationException("Horário inválido.");

                var baseVig = _servicoVigencia.Obtenha(vigenciaBaseId);
                if (baseVig == null || baseVig.OrganizacaoId != orgId || baseVig.HorarioDeTrabalhoId != horarioId)
                    throw new ApplicationException("Vigência base inválida.");

                if (!DateTime.TryParseExact(vigenciaInicio, "dd/MM/yyyy", new CultureInfo("pt-BR"), DateTimeStyles.None, out var dt))
                    throw new ApplicationException("Data de vigência inválida. Use dd/MM/yyyy.");

                var existe = _servicoVigencia.Obtenha(v =>
                    v.OrganizacaoId == orgId &&
                    v.HorarioDeTrabalhoId == horarioId &&
                    v.VigenciaInicio.Date == dt.Date);

                if (existe != null)
                    throw new ApplicationException("Já existe uma vigência cadastrada com essa data.");

                var nova = new HorarioDeTrabalhoVigencia
                {
                    OrganizacaoId = orgId,
                    HorarioDeTrabalhoId = horarioId,
                    VigenciaInicio = dt.Date,

                    UtilizaCincoPeriodos = baseVig.UtilizaCincoPeriodos,
                    UtilizaBancoDeHoras = baseVig.UtilizaBancoDeHoras,
                    InicioBancoDeHoras = baseVig.InicioBancoDeHoras,
                    TipoCargaHoraria = baseVig.TipoCargaHoraria,
                    CargaHorariaMensalFixa = baseVig.CargaHorariaMensalFixa,
                    IntervaloAutomatico = baseVig.IntervaloAutomatico,
                    ToleranciaDiariaEmMinutos = baseVig.ToleranciaDiariaEmMinutos,
                    ColunasVisiveis = baseVig.ColunasVisiveis,
                    ConsiderarFacultativoComoFeriadoHoraExtra = baseVig.ConsiderarFacultativoComoFeriadoHoraExtra,
                    ToleranciaDsrEmMinutos = baseVig.ToleranciaDsrEmMinutos
                };

                var novaVigId = _servicoVigencia.Adicionar(nova);

                var diasBase = _servicoDia.ObtenhaLista(d => d.OrganizacaoId == orgId && d.HorarioDeTrabalhoVigenciaId == vigenciaBaseId);
                diasBase = NormalizarDias(orgId, diasBase);

                foreach (var d in diasBase)
                {
                    d.Id = 0;
                    d.OrganizacaoId = orgId;
                    d.HorarioDeTrabalhoVigenciaId = novaVigId;
                    _servicoDia.Adicionar(d);
                }

                var regrasBase = _servicoRegraHE.ObtenhaLista(r => r.OrganizacaoId == orgId && r.HorarioDeTrabalhoVigenciaId == vigenciaBaseId);
                foreach (var rb in regrasBase)
                {
                    var faixasBase = _servicoFaixaHE.ObtenhaLista(f => f.RegraHoraExtraId == rb.Id).OrderBy(f => f.Ordem).ToList();

                    rb.Id = 0;
                    rb.OrganizacaoId = orgId;
                    rb.HorarioDeTrabalhoVigenciaId = novaVigId;
                    rb.Faixas = null;

                    var regraIdNova = _servicoRegraHE.Adicionar(rb);

                    int ordem = 1;
                    foreach (var fx in faixasBase)
                    {
                        fx.Id = 0;
                        fx.OrganizacaoId = orgId;
                        fx.RegraHoraExtraId = regraIdNova;
                        fx.Ordem = ordem++;
                        _servicoFaixaHE.Adicionar(fx);
                    }
                }

                return Json(new { sucesso = true, mensagem = "Vigência criada.", vigenciaId = novaVigId });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<HorarioDeTrabalho> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<HorarioDeTrabalho, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Sigla.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _servico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }

        private static List<HorarioDeTrabalhoDia> DiasPadrao(int orgId)
        {
            return new()
            {
                new() { OrganizacaoId = orgId, DiaDaSemana = eDiaDaSemana.Segunda },
                new() { OrganizacaoId = orgId, DiaDaSemana = eDiaDaSemana.Terca },
                new() { OrganizacaoId = orgId, DiaDaSemana = eDiaDaSemana.Quarta },
                new() { OrganizacaoId = orgId, DiaDaSemana = eDiaDaSemana.Quinta },
                new() { OrganizacaoId = orgId, DiaDaSemana = eDiaDaSemana.Sexta },
                new() { OrganizacaoId = orgId, DiaDaSemana = eDiaDaSemana.Sabado },
                new() { OrganizacaoId = orgId, DiaDaSemana = eDiaDaSemana.Domingo }
            };
        }

        private static List<HorarioDeTrabalhoDia> NormalizarDias(int orgId, System.Collections.Generic.IEnumerable<HorarioDeTrabalhoDia> dias)
        {
            var dict = dias.ToDictionary(x => x.DiaDaSemana, x => x);

            var baseDias = DiasPadrao(orgId);

            var outList = new System.Collections.Generic.List<HorarioDeTrabalhoDia>();
            foreach (var d in baseDias)
            {
                if (dict.TryGetValue(d.DiaDaSemana, out var e))
                {
                    outList.Add(new HorarioDeTrabalhoDia
                    {
                        OrganizacaoId = orgId,
                        DiaDaSemana = d.DiaDaSemana,
                        Entrada1 = e.Entrada1,
                        Saida1 = e.Saida1,
                        Entrada2 = e.Entrada2,
                        Saida2 = e.Saida2,
                        Entrada3 = e.Entrada3,
                        Saida3 = e.Saida3,
                        Entrada4 = e.Entrada4,
                        Saida4 = e.Saida4,
                        Entrada5 = e.Entrada5,
                        Saida5 = e.Saida5,
                        CargaHorariaFixa = e.CargaHorariaFixa
                    });
                }
                else
                {
                    outList.Add(d);
                }
            }

            return outList;
        }

        private HorarioDeTrabalhoCadastroVM MapearParaVM(
            HorarioDeTrabalho horario,
            HorarioDeTrabalhoVigencia vig,
            System.Collections.Generic.List<HorarioDeTrabalhoDia> dias,
            System.Collections.Generic.List<RegraHoraExtra> regrasTela)
        {
            return new HorarioDeTrabalhoCadastroVM
            {
                Id = horario.Id,
                Sigla = horario.Sigla,
                Descricao = horario.Descricao,
                Ativo = horario.Ativo,

                VigenciaId = vig.Id,
                VigenciaInicio = vig.VigenciaInicio.Date,

                UtilizaCincoPeriodos = vig.UtilizaCincoPeriodos,
                UtilizaBancoDeHoras = vig.UtilizaBancoDeHoras,
                InicioBancoDeHoras = vig.InicioBancoDeHoras,

                TipoCargaHoraria = vig.TipoCargaHoraria,
                CargaHorariaMensalFixa = vig.CargaHorariaMensalFixa,

                IntervaloAutomatico = vig.IntervaloAutomatico,
                ToleranciaDiariaEmMinutos = vig.ToleranciaDiariaEmMinutos,
                ColunasVisiveis = vig.ColunasVisiveis,

                ConsiderarFacultativoComoFeriadoHoraExtra = vig.ConsiderarFacultativoComoFeriadoHoraExtra,
                ToleranciaDsrEmMinutos = vig.ToleranciaDsrEmMinutos,

                Dias = dias,
                RegrasHoraExtra = regrasTela,

                BancoDeHorasSomenteHorasExtrasAprovadas = vig.BancoDeHorasSomenteHorasExtrasAprovadas,
                BancoDeHorasPrioridadePercentuais = vig.BancoDeHorasPrioridadePercentuais
            };
        }

        private static void AplicarCamposVigencia(HorarioDeTrabalhoCadastroVM vm, HorarioDeTrabalhoVigencia vig)
        {
            vig.UtilizaCincoPeriodos = vm.UtilizaCincoPeriodos;
            vig.UtilizaBancoDeHoras = vm.UtilizaBancoDeHoras;
            vig.InicioBancoDeHoras = vm.InicioBancoDeHoras;

            vig.TipoCargaHoraria = vm.TipoCargaHoraria;
            vig.CargaHorariaMensalFixa = vm.CargaHorariaMensalFixa;

            vig.IntervaloAutomatico = vm.IntervaloAutomatico;
            vig.ToleranciaDiariaEmMinutos = vm.ToleranciaDiariaEmMinutos;

            vig.ColunasVisiveis = vm.ColunasVisiveis;

            vig.ConsiderarFacultativoComoFeriadoHoraExtra = vm.ConsiderarFacultativoComoFeriadoHoraExtra;
            vig.ToleranciaDsrEmMinutos = vm.ToleranciaDsrEmMinutos;

            vig.BancoDeHorasSomenteHorasExtrasAprovadas = vm.BancoDeHorasSomenteHorasExtrasAprovadas;
            vig.BancoDeHorasPrioridadePercentuais = vm.BancoDeHorasPrioridadePercentuais;
        }

        private List<RegraHoraExtra> MonteRegrasPadraoParaTela(List<RegraHoraExtra> regrasCarregadas)
        {
            var lista = new List<RegraHoraExtra>();

            RegraHoraExtra Get(eTipoDiaHoraExtra tipo) =>
                regrasCarregadas.FirstOrDefault(r => r.TipoDia == tipo)
                ?? new RegraHoraExtra
                {
                    TipoDia = tipo,
                    Ativo = true,
                    GerarHoraExtraSobreBaseDaJornada = (tipo != eTipoDiaHoraExtra.DiaTrabalho),
                    PercentualBase = (tipo != eTipoDiaHoraExtra.DiaTrabalho) ? 100 : 0,
                    Faixas = new List<FaixaHoraExtra>()
                };

            lista.Add(Get(eTipoDiaHoraExtra.DiaTrabalho));
            lista.Add(Get(eTipoDiaHoraExtra.DiaFolga));
            lista.Add(Get(eTipoDiaHoraExtra.Feriado));

            return lista;
        }

        private void SalvarConfiguracaoHorasExtras(int orgId, int vigenciaId, System.Collections.Generic.List<RegraHoraExtra> regrasHoraExtra)
        {
            var antigas = _servicoRegraHE.ObtenhaLista(r =>
                r.OrganizacaoId == orgId &&
                r.HorarioDeTrabalhoVigenciaId == vigenciaId);

            foreach (var regra in antigas)
            {
                var faixasAntigas = _servicoFaixaHE.ObtenhaLista(f => f.RegraHoraExtraId == regra.Id);
                foreach (var fx in faixasAntigas)
                    _servicoFaixaHE.Remover(fx, false);

                _servicoRegraHE.Remover(regra, false);
            }

            if (regrasHoraExtra == null) return;

            foreach (var regraTela in regrasHoraExtra)
            {
                var regra = new RegraHoraExtra
                {
                    OrganizacaoId = orgId,
                    HorarioDeTrabalhoVigenciaId = vigenciaId,
                    TipoDia = regraTela.TipoDia,
                    GerarHoraExtraSobreBaseDaJornada = regraTela.GerarHoraExtraSobreBaseDaJornada,
                    PercentualBase = regraTela.GerarHoraExtraSobreBaseDaJornada ? regraTela.PercentualBase : 0,
                    Ativo = true,
                    AprovarAutomaticamente = regraTela.AprovarAutomaticamente,
                };

                var regraId = _servicoRegraHE.Adicionar(regra);

                var faixas = regraTela.Faixas ?? new();
                int ordem = 1;

                foreach (var fx in faixas
                    .Where(x => x.Percentual > 0)
                    .OrderBy(x => x.Ordem <= 0 ? int.MaxValue : x.Ordem))
                {
                    var faixaNova = new FaixaHoraExtra
                    {
                        OrganizacaoId = orgId,
                        RegraHoraExtraId = regraId,
                        Ordem = ordem++,
                        MinutosAte = fx.MinutosAte,
                        Percentual = fx.Percentual,
                        Ativo = true
                    };

                    _servicoFaixaHE.Adicionar(faixaNova);
                }
            }
        }
    }
}