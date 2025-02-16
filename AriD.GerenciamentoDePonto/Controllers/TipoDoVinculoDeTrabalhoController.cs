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
    public class TipoDoVinculoDeTrabalhoController : BaseController
    {
        private readonly IServico<TipoDoVinculoDeTrabalho> _servico;

        public TipoDoVinculoDeTrabalhoController(IServico<TipoDoVinculoDeTrabalho> funcaoServico)
        {
            _servico = funcaoServico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<TipoDoVinculoDeTrabalho> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<TipoDoVinculoDeTrabalho> listaPaginada)
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
        public async Task<IActionResult> Modal(int tipoDoVinculoDeTrabalhoId)
        {
            try
            {
                var model = tipoDoVinculoDeTrabalhoId == 0 ?
                    new TipoDoVinculoDeTrabalho { Ativo = true } :
                    _servico.Obtenha(tipoDoVinculoDeTrabalhoId);

                var html = await RenderizarComoString("_Modal", model);

                return Json(new { sucesso = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Salvar(TipoDoVinculoDeTrabalho tipoDoVinculoDeTrabalho)
        {
            try
            {
                int id = tipoDoVinculoDeTrabalho.Id;
                tipoDoVinculoDeTrabalho.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                if (tipoDoVinculoDeTrabalho.Id == 0)
                    id = _servico.Adicionar(tipoDoVinculoDeTrabalho);
                else
                    _servico.Atualizar(tipoDoVinculoDeTrabalho);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = true, mensagem = "Ocorreu um erro." });
            }
        }

        [HttpDelete]
        public IActionResult Remova(int tipoId)
        {
            try
            {
                var tipo = _servico.Obtenha(tipoId);
                _servico.Remover(tipo);

                return Json(new { sucesso = true, mensagem = "O registro foi removido." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Ocorreu um erro." });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<TipoDoVinculoDeTrabalho> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<TipoDoVinculoDeTrabalho, bool>> filtro =
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