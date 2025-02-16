using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class DepartamentoController : BaseController
    {
        private readonly IServico<Departamento> _servicoDepartamento;
        private readonly IServico<Organizacao> _servicoOrganizacao;

        public DepartamentoController(
            IServico<Departamento> servicoDepartamento, 
            IServico<Organizacao> servicoOrganizacao)
        {
            _servicoDepartamento = servicoDepartamento;
            _servicoOrganizacao = servicoOrganizacao;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Departamento> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<Departamento> listaPaginada)
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
        public async Task<IActionResult> Modal(int departamentoId)
        {
            try
            {
                var model = departamentoId == 0 ?
                    new Departamento { Ativo = true } :
                    _servicoDepartamento.Obtenha(departamentoId);

                var html = await RenderizarComoString("_Modal", model);

                return Json(new { sucesso = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Salvar(Departamento departamento)
        {
            try
            {
                int id = departamento.Id;
                departamento.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                if (departamento.Id == 0)
                    id = _servicoDepartamento.Adicionar(departamento);
                else
                    _servicoDepartamento.Atualizar(departamento);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = true, mensagem = "Ocorreu um erro." });
            }
        }

        [HttpDelete]
        public IActionResult Remova(int departamentoId)
        {
            try
            {
                var departamento = _servicoDepartamento.Obtenha(departamentoId);
                _servicoDepartamento.Remover(departamento);

                return Json(new { sucesso = true, mensagem = "O registro foi removido." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Ocorreu um erro." });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Departamento> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<Departamento, bool>> filtro = 
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Sigla.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _servicoDepartamento.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}