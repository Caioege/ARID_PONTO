using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class UnidadeOrganizacionalController : Controller
    {
        private readonly IServico<UnidadeOrganizacional> _servicoUnidadeOrganizacional;
        private readonly IServico<Organizacao> _servicoOrganizacao;

        public UnidadeOrganizacionalController(
            IServico<UnidadeOrganizacional> servicoUnidadeOrganizacional, 
            IServico<Organizacao> servicoOrganizacao)
        {
            _servicoUnidadeOrganizacional = servicoUnidadeOrganizacional;
            _servicoOrganizacao = servicoOrganizacao;
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<UnidadeOrganizacional> listaPaginada)
        {
            try
            {
                var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

                var dados = _servicoUnidadeOrganizacional.ObtenhaListaPaginada(c => c.OrganizacaoId == parametros.OrganizacaoId, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

                listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");

                return View("_TabelaPaginada", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Adicionar(int organizacaoId)
        {
            try
            {
                return View(new UnidadeOrganizacional
                {
                    OrganizacaoId = organizacaoId,
                    Organizacao = _servicoOrganizacao.Obtenha(organizacaoId)
                });
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
                return View(_servicoUnidadeOrganizacional.Obtenha(id));
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(UnidadeOrganizacional unidadeOrganizacional)
        {
            try
            {
                int id = unidadeOrganizacional.Id;

                if (unidadeOrganizacional.Id == 0)
                    id = _servicoUnidadeOrganizacional.Adicionar(unidadeOrganizacional);
                else
                    _servicoUnidadeOrganizacional.Atualizar(unidadeOrganizacional);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = true, mensagem = "Ocorreu um erro." });
            }
        }
    }
}