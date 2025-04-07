using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.GerenciamentoEscolar.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class AlunoController : BaseController
    {
        private readonly IServico<Aluno> _servico;
        private readonly IServico<Escola> _servicoEscola;

        public AlunoController(
            IServico<Aluno> servico,
            IServico<Escola> servicoEscola)
        {
            _servico = servico;
            _servicoEscola = servicoEscola;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Aluno> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<Aluno> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View("_Tabela", listaPaginada);
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
                var dadosDaSessao = HttpContext.DadosDaSessao();
                if (dadosDaSessao.Perfil != ePerfilDeAcesso.Escola)
                {
                    ViewBag.Escolas = new SelectList(
                        _servicoEscola
                        .ObtenhaLista(c => c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId && c.Ativa)
                        .OrderBy(c => c.Nome),
                        "Id",
                        "Nome");
                }

                return View(new Aluno() { Pessoa = new() { Endereco = new() } });
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

        [HttpPost]
        public IActionResult Salvar(Aluno servidor)
        {
            try
            {
                int id = servidor.Id;
                servidor.RedeDeEnsinoId = this.HttpContext.DadosDaSessao().RedeDeEnsinoId;
                servidor.Pessoa.RedeDeEnsinoId = servidor.RedeDeEnsinoId;

                if (servidor.Id == 0)
                {
                    servidor.DataDeCadastro = DateTime.Now;
                    id = _servico.Adicionar(servidor);
                }
                else
                    _servico.Atualizar(servidor);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Aluno> listaPaginada)
        {
            var dados = _servico.ObtenhaListaPaginada(
                CarregueFiltrosDePesquisa(listaPaginada), 
                listaPaginada.Pagina, 
                listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }

        private Expression<Func<Aluno, bool>> CarregueFiltrosDePesquisa(
            ListaPaginada<Aluno> listaPaginada)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();

            Expression<Func<Aluno, bool>> pesquisa = c =>
                (c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId);

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                var pesquisaToLower = listaPaginada.TermoDeBusca.ToLower().Trim();
                var somenteNumeros = ObterSomenteNumeros(listaPaginada.TermoDeBusca, "----");

                pesquisa = ConcatenadorDeExpressao.Concatenar(
                    pesquisa,
                    c => (
                        c.Pessoa.Nome.ToLower().Contains(pesquisaToLower) ||
                        c.Pessoa.Cpf.Replace(".", "").Replace("-", "").Contains(somenteNumeros)) ||
                        c.Pessoa.Rg.Contains(somenteNumeros) ||
                        c.Pessoa.NomeSocial.ToLower().Contains(pesquisaToLower));
            }

            return pesquisa;
        }

        static string ObterSomenteNumeros(string texto, string returnIfNull)
        {
            if (string.IsNullOrEmpty(texto))
                return returnIfNull;

            var retorno = new string(texto.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(retorno))
                return returnIfNull;

            return retorno;
        }
    }
}
