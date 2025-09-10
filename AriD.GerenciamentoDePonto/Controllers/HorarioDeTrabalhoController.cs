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

        public HorarioDeTrabalhoController(
            IServico<HorarioDeTrabalho> servico, 
            IServico<HorarioDeTrabalhoDia> servicoDia)
        {
            _servico = servico;
            _servicoDia = servicoDia;
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

            if (horarioDeTrabalho.Id == 0)
                id = _servico.Adicionar(horarioDeTrabalho);
            else
                _servico.Atualizar(horarioDeTrabalho);

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
    }
}