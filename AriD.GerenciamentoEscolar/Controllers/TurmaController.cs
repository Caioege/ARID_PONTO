using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
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
        private readonly IServico<ItemHorarioDeAula> _servicoHorario;
        private readonly IServico<FrequenciaAlunoTurma> _servicoFrequencia;

        public TurmaController(
            IServico<Turma> servico,
            IServico<Escola> servicoEscola,
            IServicoDeAlunos servicoDeAlunos,
            IServico<AlunoTurma> servicoAlunoTurma,
            IServico<ItemHorarioDeAula> servicoHorario,
            IServico<FrequenciaAlunoTurma> servicoFrequencia)
        {
            _servico = servico;
            _servicoEscola = servicoEscola;
            _servicoDeAlunos = servicoDeAlunos;
            _servicoAlunoTurma = servicoAlunoTurma;
            _servicoHorario = servicoHorario;
            _servicoFrequencia = servicoFrequencia;
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
                ViewBag.MesesDoAno = ObterMesesNoPeriodo(turma.InicioDasAulas, turma.FimDasAulas);
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
            {
                if (turma.Situacao != eSituacaoTurma.Ativa)
                {
                    var turmaPersistida = _servico.Obtenha(id);
                    foreach (var alunoTurma in turmaPersistida.ListaDeAlunos.Where(c => c.Situacao == eSituacaoAlunoNaTurma.Cursando))
                    {
                        alunoTurma.SaidaDaTurma = DateTime.Today;
                        alunoTurma.Situacao = eSituacaoAlunoNaTurma.Concluido;

                        _servicoAlunoTurma.Atualizar(alunoTurma);
                    }
                }

                _servico.Atualizar(turma);
            }

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

            if (alunoTurma.Situacao == eSituacaoAlunoNaTurma.Cursando && alunoTurmaPersistido.Turma.Situacao != eSituacaoTurma.Ativa)
                throw new ApplicationException("Não é possível alterar a situação do aluno para 'Cursando' quando a turma não está ativa.");

            if (alunoTurmaPersistido.SaidaDaTurma < alunoTurmaPersistido.EntradaNaTurma)
                throw new ApplicationException("A data de saída não pode ser menor que a de entrada.");

            if (!alunoTurmaPersistido.SaidaDaTurma.HasValue && alunoTurmaPersistido.Situacao != eSituacaoAlunoNaTurma.Cursando)
                throw new ApplicationException("Se o aluno não estiver cursando é obrigatório informar a data de saída.");

            if (alunoTurmaPersistido.SaidaDaTurma.HasValue && alunoTurmaPersistido.Situacao == eSituacaoAlunoNaTurma.Cursando)
                throw new ApplicationException("Não é possível informar a data de saída quando o aluno está cursando.");

            _servicoAlunoTurma.Atualizar(alunoTurmaPersistido);
            return Json(new { sucesso = true, mensagem = "Os dados do aluno foram alterados." });
        }

        [HttpGet]
        public async Task<ActionResult> ModalHorarioDeAula(
            int turmaId, 
            eDiaDaSemana diaDaSemana)
        {
            var turma = _servico.Obtenha(turmaId);
            ViewBag.DiaDaSemana = diaDaSemana;
            var html = await RenderizarComoString("_ModalHorarioDeAula", turma.ListaDeHorarioDeAula.Where(c => c.DiaDaSemana == diaDaSemana).ToList());

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public ActionResult SalvarHorarioDeAula(
            int turmaId, 
            eDiaDaSemana diaDaSemana, 
            List<ItemHorarioDeAula> horarios)
        {
            var turma = _servico.Obtenha(turmaId);

            var horariosDoDia = turma
                .ListaDeHorarioDeAula
                .Where(c => c.DiaDaSemana == diaDaSemana)
                .ToList();

            if (horarios == null || !horarios.Any())
            {
                foreach (var horario in horariosDoDia)
                    _servicoHorario.Remover(horario, false);
            }
            else
            {
                foreach (var horario in horariosDoDia)
                {
                    var horarioExistente = horarios
                        .FirstOrDefault(c => c.InicioAula == horario.InicioAula && c.FimAula == horario.FimAula);

                    if (horarioExistente == null)
                        _servicoHorario.Remover(horario, false);
                }

                foreach (var horario in horarios)
                {
                    horario.RedeDeEnsinoId = turma.RedeDeEnsinoId;

                    if (horario.Id > 0)
                    {
                        var horarioExistente = horariosDoDia.First(c => c.Id ==  horario.Id);

                        horarioExistente.InicioAula = horario.InicioAula;
                        horarioExistente.FimAula = horario.FimAula;
                        horarioExistente.Intervalo = horario.Intervalo;
                        horarioExistente.Disciplina = horario.Disciplina;

                        _servicoHorario.Atualizar(horarioExistente, false);
                    }
                    else
                        _servicoHorario.Adicionar(horario, false);
                }
            }

            _servicoHorario.Commit();

            return Json(new { sucesso = true, mensagem = "Os horários do dia foram salvos." });
        }

        [HttpGet]
        public async Task<ActionResult> CarregarDiarioDeClasse(
            int turmaId,
            string anoMes)
        {
            var split = anoMes.Split('-');
            var ano = int.Parse(split[0]);
            var mes = int.Parse(split[1]);

            var turma = _servico.Obtenha(turmaId);

            var inicio = new DateTime(ano, mes, 01);
            var final = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes));

            if (turma.InicioDasAulas > inicio)
                inicio = turma.InicioDasAulas;

            if (turma.FimDasAulas < final)
                final = turma.FimDasAulas;

            var alunos = _servicoDeAlunos.ListaDeAlunosParaDiario(turmaId, inicio, final);
            var diarioDTO = new DiarioClasseDTO
            {
                Alunos = alunos,
                DataInicio = inicio,
                DataFim = final,
                Horarios = turma.ListaDeHorarioDeAula
            };

            var html = await RenderizarComoString("_PartialDiarioDeClasse", diarioDTO);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public ActionResult SalvarFrequencia(
            int alunoTurmaId, 
            DateTime dia, 
            bool? frequencia)
        {
            var frequenciaPersistida = _servicoFrequencia
                .Obtenha(c => c.AlunoTurmaId == alunoTurmaId && c.DataHora == dia);

            if (frequenciaPersistida == null && frequencia.HasValue)
                _servicoFrequencia.Adicionar(new FrequenciaAlunoTurma
                {
                    RedeDeEnsinoId = HttpContext.DadosDaSessao().RedeDeEnsinoId,
                    DataHora = dia,
                    AlunoTurmaId = alunoTurmaId,
                    EstavaPresente = frequencia.Value
                });
            else if (frequenciaPersistida != null)
            {
                if (!frequencia.HasValue)
                    _servicoFrequencia.Remover(frequenciaPersistida);
                else
                {
                    frequenciaPersistida.EstavaPresente = frequencia.Value;
                    _servicoFrequencia.Atualizar(frequenciaPersistida);
                }
            }

            return Json(new { sucesso = true, mensagem = "Frequência atualizada com sucesso." });
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

        public static List<SelectListItem> ObterMesesNoPeriodo(
            DateTime dataInicio, 
            DateTime dataFim)
        {
            var lista = new List<SelectListItem>();

            var data = new DateTime(dataInicio.Year, dataInicio.Month, 1);
            var dataLimite = new DateTime(dataFim.Year, dataFim.Month, 1);

            while (data <= dataLimite)
            {
                lista.Add(new SelectListItem
                {
                    Value = data.ToString("yyyy-MM"),
                    Text = $"[{data.Year}] {ObterNomeMes(data.Month)}"
                });

                data = data.AddMonths(1);
            }

            return lista;
        }

        private static string ObterNomeMes(int mes)
        {
            return mes switch
            {
                1 => "Janeiro",
                2 => "Fevereiro",
                3 => "Março",
                4 => "Abril",
                5 => "Maio",
                6 => "Junho",
                7 => "Julho",
                8 => "Agosto",
                9 => "Setembro",
                10 => "Outubro",
                11 => "Novembro",
                12 => "Dezembro",
                _ => ""
            };
        }
    }
}