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
    public class GrupoDePermissaoController : BaseController
    {
        private readonly IServico<GrupoDePermissao> _servico;
        private readonly IServico<ItemDoGrupoDePermissao> _servicoPermissao;

        public GrupoDePermissaoController(
            IServico<GrupoDePermissao> servico, 
            IServico<ItemDoGrupoDePermissao> servicoPermissao)
        {
            _servico = servico;
            _servicoPermissao = servicoPermissao;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<GrupoDePermissao> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<GrupoDePermissao> listaPaginada)
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
        public async Task<IActionResult> Modal()
        {
            var model = new GrupoDePermissao { Ativo = true };
            var html = await RenderizarComoString("_Modal", model);
            return Json(new { sucesso = true, html = html });
        }

        [HttpGet]
        public ActionResult Alterar(int grupoDePermissaoId)
        {
            try
            {
                var grupoDePermissao = _servico.Obtenha(grupoDePermissaoId);
                return View(grupoDePermissao);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(GrupoDePermissao grupoDePermissao)
        {
            int id = grupoDePermissao.Id;
            grupoDePermissao.OrganizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;

            grupoDePermissao.ListaDePermissao
                .ForEach(c => {
                    c.OrganizacaoId = grupoDePermissao.OrganizacaoId;
                    if (c.Id == 0)
                        _servicoPermissao.Adicionar(c);
                    else
                        _servicoPermissao.Atualizar(c);
                });

            if (grupoDePermissao.Id == 0)
                id = _servico.Adicionar(grupoDePermissao);
            else
                _servico.Atualizar(grupoDePermissao);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public IActionResult Remova(int grupoDePermissaoId)
        {
            var grupoDePermissao = _servico.Obtenha(grupoDePermissaoId);

            foreach (var item in grupoDePermissao.ListaDePermissao)
                _servicoPermissao.Remover(item);

            _servico.Remover(grupoDePermissao);

            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<GrupoDePermissao> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<GrupoDePermissao, bool>> filtro =
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