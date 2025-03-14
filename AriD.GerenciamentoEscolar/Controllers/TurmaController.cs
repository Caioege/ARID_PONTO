using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.GerenciamentoEscolar.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class TurmaController : BaseController
    {
        private readonly IServico<Turma> _servico;
        private readonly IServico<Escola> _servicoEscola;

        public TurmaController(
            IServico<Turma> servico, 
            IServico<Escola> servicoEscola)
        {
            _servico = servico;
            _servicoEscola = servicoEscola;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Turma> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<Turma> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View("_TabelaPaginada", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content("Ocorreu um erro ao carregar os dados.");
            }
        }

        [HttpGet]
        public ActionResult Adicionar()
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();
                if (dadosDaSessao.Perfil == ePerfilDeAcesso.RedeDeEnsino)
                {
                    ViewBag.Escolas = new SelectList(
                        _servicoEscola
                            .ObtenhaLista(c => c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId)
                            .OrderBy(c => c.Nome),
                        "Id",
                        "Nome");
                }

                return View(new Turma
                {
                    AnoLetivo = DateTime.Today.Year,
                    Situacao = eSituacaoTurma.Ativa
                });
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }


        [HttpGet]
        public ActionResult Alterar(int id)
        {
            try
            {
                var turma = _servico.Obtenha(id);
                return View(turma);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(Turma turma)
        {
            int id = turma.Id;
            turma.RedeDeEnsinoId = HttpContext.DadosDaSessao().RedeDeEnsinoId;

            if (turma.Id == 0)
                id = _servico.Adicionar(turma);
            else
                _servico.Atualizar(turma);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public IActionResult Remova(int turmaId)
        {
            var turma = _servico.Obtenha(turmaId);
            _servico.Remover(turma);

            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Turma> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaescolasOrganizacionais>(listaPaginada.Adicional);

            var dadosDaSessao = HttpContext.DadosDaSessao();
            parametros.RedeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;

            Expression<Func<Turma, bool>> filtro =
                c => c.RedeDeEnsinoId == parametros.RedeDeEnsinoId;

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Escola)
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.EscolaId == dadosDaSessao.Escolas.First());
            }

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Descricao.Contains(listaPaginada.TermoDeBusca, StringComparison.CurrentCultureIgnoreCase));
            }

            var dados = _servico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}
