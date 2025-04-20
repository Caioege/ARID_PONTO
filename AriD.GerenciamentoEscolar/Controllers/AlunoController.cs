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
        private readonly IServicoDeAlunos _servicoDeAlunos;

        public AlunoController(
            IServico<Aluno> servico,
            IServico<Escola> servicoEscola,
            IServicoDeAlunos servicoDeAlunos)
        {
            _servico = servico;
            _servicoEscola = servicoEscola;
            _servicoDeAlunos = servicoDeAlunos;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Aluno> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada, true);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Aluno> listaPaginada, bool somenteMatriculados)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada, somenteMatriculados);
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
        public IActionResult Salvar(Aluno aluno)
        {
            int id = aluno.Id;
            aluno.RedeDeEnsinoId = this.HttpContext.DadosDaSessao().RedeDeEnsinoId;
            aluno.Pessoa.RedeDeEnsinoId = aluno.RedeDeEnsinoId;

            if (aluno.Id == 0)
            {
                aluno.DataDeCadastro = DateTime.Now;
                id = _servico.Adicionar(aluno);
            }
            else
            {
                aluno.EscolaId = _servicoDeAlunos.ObtenhaEscolaIdDoAluno(aluno.Id);
                _servico.Atualizar(aluno);
            }

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public IActionResult Remover(int id)
        {
            var aluno = _servico.Obtenha(id);
            if (aluno.ListaDeVinculosDeTurma.Count > 0)
                throw new ApplicationException("Não é possível remover o aluno, pois existem dados vinculados a ele.");

            _servico.Remover(aluno);

            return Json(new { sucesso = true, mensagem = "O aluno foi removido." });
        }

        [HttpPost]
        public IActionResult MatricularNaEscola(
            int alunoId, 
            int? escolaId, 
            string idEquipamento)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Escola)
                escolaId = dadosDaSessao.EscolaId;

            if (!escolaId.HasValue)
                throw new ApplicationException("A escola deve ser informada.");

            var aluno = _servico.Obtenha(alunoId);
            if (aluno.EscolaId.HasValue)
                throw new ApplicationException("O aluno já está matriculado em uma escola.");

            if (string.IsNullOrEmpty(idEquipamento))
                throw new ApplicationException("Informe o id do equipamento.");

            aluno.IdEquipamento = idEquipamento;
            aluno.EscolaId = escolaId;
            _servico.Atualizar(aluno);

            return Json(new { sucesso = true, mensagem = "O aluno foi matriculado na escola." });
        }

        [HttpGet]
        public async Task<IActionResult> ModalMatricularNaEscola()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            ViewBag.Escolas = new SelectList(
                _servicoEscola
                    .ObtenhaLista(c => c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId)
                    .OrderBy(c => c.Nome),
                "Id",
                "Nome");

            var html = await RenderizarComoString("_ModalMatricularNaEscola", null);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult Desalocar(int alunoId)
        {
            var aluno = _servico.Obtenha(alunoId);
            if (!aluno.EscolaId.HasValue)
                throw new ApplicationException("O aluno já está desalocado.");

            aluno.IdEquipamento = null;
            aluno.EscolaId = null;
            _servico.Atualizar(aluno);

            return Json(new { sucesso = true, mensagem = "O aluno foi desalocado." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Aluno> listaPaginada, bool somenteMatriculados)
        {
            var dados = _servico.ObtenhaListaPaginada(
                CarregueFiltrosDePesquisa(listaPaginada, somenteMatriculados), 
                listaPaginada.Pagina, 
                listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }

        private Expression<Func<Aluno, bool>> CarregueFiltrosDePesquisa(
            ListaPaginada<Aluno> listaPaginada,
            bool somenteMatriculados)
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

            if (dadosDaSessao.EscolaId.HasValue)
            {
                pesquisa = ConcatenadorDeExpressao.Concatenar(
                    pesquisa,
                    c => c.EscolaId == dadosDaSessao.EscolaId || !c.EscolaId.HasValue);
            }

            if (somenteMatriculados)
            {
                pesquisa = ConcatenadorDeExpressao.Concatenar(
                    pesquisa,
                    c => c.EscolaId.HasValue);
            }
            else
            {
                pesquisa = ConcatenadorDeExpressao.Concatenar(
                    pesquisa,
                    c => !c.EscolaId.HasValue);
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
