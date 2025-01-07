using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class HorarioDeTrabalhoController : Controller
    {
        private readonly IServico<HorarioDeTrabalho> _servico;

        public HorarioDeTrabalhoController(
            IServico<HorarioDeTrabalho> servico)
        {
            _servico = servico;
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
            try
            {
                var cargaHoraria = dia.CalculeCargaHorariaTotal();
                return Json(new { sucesso = true, cargaHoraria = cargaHoraria?.ToString(@"hh\:mm") });
            }
            catch (Exception ex)
            {
                return Json(new { sucess = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Salvar(HorarioDeTrabalho horarioDeTrabalho)
        {
            try
            {
                int id = horarioDeTrabalho.Id;
                horarioDeTrabalho.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                horarioDeTrabalho.Dias.ForEach(c => c.OrganizacaoId = horarioDeTrabalho.OrganizacaoId);

                if (horarioDeTrabalho.Id == 0)
                    id = _servico.Adicionar(horarioDeTrabalho);
                else
                    _servico.Atualizar(horarioDeTrabalho);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = true, mensagem = "Ocorreu um erro." });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<HorarioDeTrabalho> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            var dados = _servico.ObtenhaListaPaginada(c => c.OrganizacaoId == parametros.OrganizacaoId, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}