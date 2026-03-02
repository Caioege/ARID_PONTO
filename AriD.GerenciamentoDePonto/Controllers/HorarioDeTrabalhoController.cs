using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class HorarioDeTrabalhoController : Controller
    {
        private readonly IServico<HorarioDeTrabalho> _servico;
        private readonly IServico<HorarioDeTrabalhoDia> _servicoDia;
        private readonly IServico<RegraHoraExtra> _servicoRegraHE;
        private readonly IServico<FaixaHoraExtra> _servicoFaixaHE;

        public HorarioDeTrabalhoController(
            IServico<HorarioDeTrabalho> servico,
            IServico<HorarioDeTrabalhoDia> servicoDia,
            IServico<RegraHoraExtra> servicoRegraHE,
            IServico<FaixaHoraExtra> servicoFaixaHE)
        {
            _servico = servico;
            _servicoDia = servicoDia;
            _servicoRegraHE = servicoRegraHE;
            _servicoFaixaHE = servicoFaixaHE;
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
                var horario = new HorarioDeTrabalho { Ativo = true };
                horario.Dias =
                [
                    new() { DiaDaSemana = eDiaDaSemana.Segunda },
                    new() { DiaDaSemana = eDiaDaSemana.Terca },
                    new() { DiaDaSemana = eDiaDaSemana.Quarta },
                    new() { DiaDaSemana = eDiaDaSemana.Quinta },
                    new() { DiaDaSemana = eDiaDaSemana.Sexta },
                    new() { DiaDaSemana = eDiaDaSemana.Sabado },
                    new() { DiaDaSemana = eDiaDaSemana.Domingo }
                ];

                horario.RegrasHoraExtra = new List<RegraHoraExtra>
                {
                    new() { TipoDia = eTipoDiaHoraExtra.DiaTrabalho, Ativo = true, Faixas = new() },
                    new() { TipoDia = eTipoDiaHoraExtra.DiaFolga, Ativo = true, GerarHoraExtraSobreBaseDaJornada = true, PercentualBase = 100, Faixas = new() },
                    new() { TipoDia = eTipoDiaHoraExtra.Feriado, Ativo = true, GerarHoraExtraSobreBaseDaJornada = true, PercentualBase = 100, Faixas = new() }
                };

                return View(horario);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult Alterar(int id)
        {
            try
            {
                var orgId = HttpContext.DadosDaSessao().OrganizacaoId;

                var horario = _servico.Obtenha(id);
                if (horario == null)
                    return View("Error", new Exception("Horário não encontrado."));

                var regras = _servicoRegraHE.ObtenhaLista(r =>
                    r.OrganizacaoId == orgId &&
                    r.HorarioDeTrabalhoId == horario.Id &&
                    r.Ativo);

                foreach (var regra in regras)
                {
                    regra.Faixas = _servicoFaixaHE.ObtenhaLista(f =>
                        f.OrganizacaoId == orgId &&
                        f.RegraHoraExtraId == regra.Id &&
                        f.Ativo)
                        .OrderBy(f => f.Ordem)
                        .ToList();
                }

                horario.RegrasHoraExtra = MonteRegrasPadraoParaTela(regras);

                return View(_servico.Obtenha(id));
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
        public IActionResult Salvar(HorarioDeTrabalho horarioDeTrabalho)
        {
            int id = horarioDeTrabalho.Id;
            horarioDeTrabalho.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            horarioDeTrabalho.Dias.ForEach(c => c.OrganizacaoId = horarioDeTrabalho.OrganizacaoId);

            if (horarioDeTrabalho.TipoCargaHoraria == eTipoCargaHoraria.MensalFixa && horarioDeTrabalho.UtilizaBancoDeHoras)
                throw new ApplicationException("Carga Horária Mensal Fixa não pode ter Banco de Horas habilitado.");

            var regrasHoraExtra = horarioDeTrabalho.RegrasHoraExtra ?? new List<RegraHoraExtra>();
            horarioDeTrabalho.RegrasHoraExtra = null;

            if (horarioDeTrabalho.Id == 0)
                id = _servico.Adicionar(horarioDeTrabalho);
            else
                _servico.Atualizar(horarioDeTrabalho);

            SalvarConfiguracaoHorasExtras(horarioDeTrabalho.OrganizacaoId, id, regrasHoraExtra);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public ActionResult Remover(int id)
        {
            var horario = _servico.Obtenha(id);
            foreach (var dia in new List<HorarioDeTrabalhoDia>(horario.Dias))
                _servicoDia.Remover(dia, false);

            _servico.Remover(horario);

            return Json(new { sucesso = true, mensagem = "O horário de trabalho foi removido." });
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

        private void SalvarConfiguracaoHorasExtras(int orgId, int horarioId, List<RegraHoraExtra> regrasHoraExtra)
        {
            var antigas = _servicoRegraHE.ObtenhaLista(r =>
                r.OrganizacaoId == orgId &&
                r.HorarioDeTrabalhoId == horarioId);

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
                    HorarioDeTrabalhoId = horarioId,
                    TipoDia = regraTela.TipoDia,
                    GerarHoraExtraSobreBaseDaJornada = regraTela.GerarHoraExtraSobreBaseDaJornada,
                    PercentualBase = regraTela.GerarHoraExtraSobreBaseDaJornada ? regraTela.PercentualBase : 0,
                    Ativo = true,
                    AprovarAutomaticamente = regraTela.AprovarAutomaticamente,
                };

                var regraId = _servicoRegraHE.Adicionar(regra);

                var faixas = regraTela.Faixas ?? new List<FaixaHoraExtra>();
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