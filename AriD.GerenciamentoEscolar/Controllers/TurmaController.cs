using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.GerenciamentoEscolar.WebGrid;
using AriD.Servicos.Extensao;
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
        private readonly IServicoDeAlunos _servicoDeAlunos;
        private readonly IServico<AlunoTurma> _servicoAlunoTurma;

        public TurmaController(
            IServico<Turma> servico,
            IServico<Escola> servicoEscola,
            IServicoDeAlunos servicoDeAlunos,
            IServico<AlunoTurma> servicoAlunoTurma)
        {
            _servico = servico;
            _servicoEscola = servicoEscola;
            _servicoDeAlunos = servicoDeAlunos;
            _servicoAlunoTurma = servicoAlunoTurma;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Turma> listaPaginada)
        {
            try
            {
                var anosLetivosDaRede = _servicoDeAlunos.ObtenhaAnosLetivosDaRede(HttpContext.DadosDaSessao().RedeDeEnsinoId);

                var anoLetivoAtual = DateTime.Today.Year;
                ViewBag.AnosLetivos = new SelectList(
                    Enumerable.Range(anosLetivosDaRede.AnoLetivoMaisAntigo, anosLetivosDaRede.AnoLetivoMaisNovo - anosLetivosDaRede.AnoLetivoMaisAntigo + 1).ToList(), anoLetivoAtual);

                ConfigureDadosDaTabelaPaginada(listaPaginada, anoLetivoAtual);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Turma> listaPaginada, int anoLetivo)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada, anoLetivo);
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

        [HttpGet]
        public async Task<ActionResult> ModalAlocarAlunos(int turmaId)
        {
            var alunosParaAlocar = _servicoDeAlunos.ObtenhaListaDeAlunosDisponiveisParaAlocacaoNaTurma(turmaId);

            if (!alunosParaAlocar.Any())
                throw new ApplicationException("Não existe nenhum aluno disponível para alocar nessa turma.");

            var html = await RenderizarComoString("_ModalAlocarAlunos", alunosParaAlocar);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public ActionResult AlocarAlunos(int turmaId, DateTime entrada, List<int> alunos)
        {
            _servicoDeAlunos.AlocarAlunosNaTurma(turmaId, entrada, alunos);
            return Json(new { sucesso = true, mensagem = alunos.Count > 1 ? "Os alunos foram alocados." : "O aluno foi alocado." });
        }

        [HttpPost]
        public ActionResult RemoverVinculoDeAluno(int alunoTurmaId)
        {
            _servicoDeAlunos.RemoverVinculoDeAluno(alunoTurmaId);
            return Json(new { sucesso = true, mensagem = "O vínculo do aluno na turma foi removido." });
        }

        [HttpGet]
        public async Task<ActionResult> ModalAlunoTurma(int alunoTurmaId)
        {
            var alunoTurma = _servicoAlunoTurma.Obtenha(alunoTurmaId);
            var html = await RenderizarComoString("_ModalAlunoTurma", alunoTurma);
            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public ActionResult SalvarRegistroAlunoTurma(AlunoTurma alunoTurma)
        {
            var alunoTurmaPersistido = _servicoAlunoTurma.Obtenha(alunoTurma.Id);
            alunoTurmaPersistido.EntradaNaTurma = alunoTurma.EntradaNaTurma;
            alunoTurmaPersistido.SaidaDaTurma = alunoTurma.SaidaDaTurma;
            alunoTurmaPersistido.Situacao = alunoTurma.Situacao;
        }

        private void ConfigureDadosDaTabelaPaginada(
            ListaPaginada<Turma> listaPaginada, 
            int anoLetivo)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaescolasOrganizacionais>(listaPaginada.Adicional);

            var dadosDaSessao = HttpContext.DadosDaSessao();
            parametros.RedeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;

            Expression<Func<Turma, bool>> filtro =
                c => c.RedeDeEnsinoId == parametros.RedeDeEnsinoId &&
                c.AnoLetivo == anoLetivo;

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Escola)
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.EscolaId == dadosDaSessao.Escolas.First());
            }

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                var mapaAnoEscolar = ExtensaoDeEnum.ObterMapaDescricaoEnum<eAnoEscolar>();
                var mapaTurno = ExtensaoDeEnum.ObterMapaDescricaoEnum<eTurno>();

                var turnosEncontrados = mapaTurno
                    .Where(kv => kv.Key.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()))
                    .Select(kv => kv.Value)
                    .ToList();

                var anosEncontrados = mapaAnoEscolar
                    .Where(kv => kv.Key.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()))
                    .Select(kv => kv.Value)
                    .ToList();

                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Descricao.ToUpper().Contains(listaPaginada.TermoDeBusca.ToUpper()) ||
                        (anosEncontrados.Any() ? anosEncontrados.Contains(c.AnoEscolar) : true) ||
                        (turnosEncontrados.Any() ? turnosEncontrados.Contains(c.Turno) : true));
            }

            var dados = _servico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}