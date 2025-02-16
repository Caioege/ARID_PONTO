using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class JustificativaDeAusenciaController : BaseController
    {
        private readonly IServico<JustificativaDeAusencia> _justificativaServico;
        private readonly IServico<Organizacao> _servicoOrganizacao;

        public JustificativaDeAusenciaController(
            IServico<JustificativaDeAusencia> justificativaServico, 
            IServico<Organizacao> servicoOrganizacao)
        {
            _justificativaServico = justificativaServico;
            _servicoOrganizacao = servicoOrganizacao;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<JustificativaDeAusencia> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<JustificativaDeAusencia> listaPaginada)
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
        public async Task<IActionResult> Modal(int justificativaId)
        {
            try
            {
                var model = justificativaId == 0 ?
                    new JustificativaDeAusencia 
                    {
                        Ativa = true, 
                        LocalDeUso = eLocalDeUsoDeJustificativaDeAusencia.AfastamentoEFolhaDePonto,
                        Abono = true
                    } :
                    _justificativaServico.Obtenha(justificativaId);

                var html = await RenderizarComoString("_Modal", model);

                return Json(new { sucesso = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Salvar(JustificativaDeAusencia justificativa)
        {
            try
            {
                int id = justificativa.Id;
                justificativa.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                if (justificativa.Id == 0)
                    id = _justificativaServico.Adicionar(justificativa);
                else
                    _justificativaServico.Atualizar(justificativa);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Ocorreu um erro." });
            }
        }

        [HttpDelete]
        public IActionResult Remova(int justificativaId)
        {
            try
            {
                var justificativa = _justificativaServico.Obtenha(justificativaId);
                _justificativaServico.Remover(justificativa);

                return Json(new { sucesso = true, mensagem = "O registro foi removido." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Ocorreu um erro." });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<JustificativaDeAusencia> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<JustificativaDeAusencia, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Sigla.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _justificativaServico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}