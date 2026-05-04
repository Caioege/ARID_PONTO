using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class EventoAnualController : BaseController
    {
        private readonly IServico<EventoAnual> _servico;

        public EventoAnualController(
            IServico<EventoAnual> servico)
        {
            _servico = servico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<EventoAnual> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<EventoAnual> listaPaginada)
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
        public async Task<IActionResult> Modal(int eventoId)
        {
            var model = eventoId == 0 ? new() : _servico.Obtenha(eventoId);
            var html = await RenderizarComoString("_Modal", model);
            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public IActionResult Salvar(EventoAnual evento)
        {
            int id = evento.Id;
            evento.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (evento.Id == 0)
                id = _servico.Adicionar(evento);
            else
                _servico.Atualizar(evento);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public IActionResult Remova(int eventoId)
        {
            var evento = _servico.Obtenha(eventoId);
            _servico.Remover(evento);

            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<EventoAnual> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<EventoAnual, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _servico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}