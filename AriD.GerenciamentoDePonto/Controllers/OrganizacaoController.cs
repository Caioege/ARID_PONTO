using AriD.BibliotecaDeClasses.Entidades;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class OrganizacaoController : Controller
    {
        private readonly IServico<Organizacao> _servico;

        public OrganizacaoController(IServico<Organizacao> servico)
        {
            _servico = servico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Organizacao> listaPaginada)
        {
            try
            {
                AjusteContextoDePaginacao(listaPaginada);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Organizacao> listaPaginada)
        {
            try
            {
                AjusteContextoDePaginacao(listaPaginada);
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
                return View(new Organizacao { Ativa = true });
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
                return View("Alterar", _servico.Obtenha(id));
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(Organizacao organizacao)
        {
            try
            {
                int id = organizacao.Id;

                if (organizacao.Id == 0)
                    id = _servico.Adicionar(organizacao);
                else 
                    _servico.Atualizar(organizacao);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = true, mensagem = "Ocorreu um erro." });
            }
        }

        private void AjusteContextoDePaginacao(ListaPaginada<Organizacao> listaPaginada)
        {
            var dados = _servico.ObtenhaListaPaginada(
                c => true,
                listaPaginada.Pagina,
                listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total);
        }
    }
}