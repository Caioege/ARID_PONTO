using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeAlunos : Servico<Aluno>, IServicoDeAlunos
    {
        private readonly IRepositorio<Aluno> _repositorio;
        private readonly IRepositorio<Turma> _repositorioTurma;
        private readonly IRepositorio<AlunoTurma> _repositorioAlunoTurma;

        public ServicoDeAlunos(IRepositorio<Aluno> repositorio,
            IRepositorio<Turma> repositorioTurma,
            IRepositorio<AlunoTurma> repositorioAlunoTurma)
            : base(repositorio)
        {
            _repositorio = repositorio;
            _repositorioTurma = repositorioTurma;
            _repositorioAlunoTurma = repositorioAlunoTurma;
        }

        public List<CodigoDescricaoDTO> ObtenhaListaDeAlunosDisponiveisParaAlocacaoNaTurma(
            int turmaId)
        {
            try
            {
                var turma = _repositorioTurma.Obtenha(turmaId);
                if (turma == null)
                    throw new Exception("Turma não encontrada.");

                if (turma.Situacao != eSituacaoTurma.Ativa)
                    throw new ApplicationException("Não é possível alocar alunos em turmas com situação diferente de ativa.");

                var query =
                    @"select
	                    a.Id as Codigo,
                        concat(p.Nome, ' (', DATE_FORMAT(p.DataDeNascimento,'%d/%m/%Y'), ')') as Descricao
                    from aluno a
                    inner join pessoa p
	                    on p.Id = a.PessoaId
                    where
	                    a.RedeDeEnsinoId = @REDEDEENSINOID
                        and a.ConcluiuOsEstudos = false
                        and a.EscolaId = @ESCOLAID
                        and a.AnoEscolarAtual = @ANOESCOLARATUAL
                        and not exists (select 1 from alunoturma at where at.RedeDeEnsinoId = @REDEDEENSINOID and at.AlunoId = a.Id)
                    order by p.Nome";

                return _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, new
                {
                    @REDEDEENSINOID = turma.RedeDeEnsinoId,
                    @ESCOLAID = turma.EscolaId,
                    @ANOESCOLARATUAL = turma.AnoEscolar
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public (int AnoLetivoMaisAntigo, int AnoLetivoMaisNovo) ObtenhaAnosLetivosDaRede(int redeDeEnsinoId)
        {
            try
            {
                var query = @"select
	                            min(AnoLetivo) as 'Item1',
                                max(AnoLetivo) as 'Item2'
                            from turma
                            where
	                            RedeDeEnsinoId = @REDEDEENSINOID
                            limit 1";

                var resultado = _repositorio.ConsultaDapper<(int?, int?)>(query, new
                {
                    @REDEDEENSINOID = redeDeEnsinoId
                }).FirstOrDefault();

                return (resultado.Item1 ?? resultado.Item2 ?? DateTime.Today.Year, resultado.Item2 ?? DateTime.Today.Year);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AlocarAlunosNaTurma(int turmaId, DateTime entrada, List<int> alunosIds)
        {
            try
            {
                if (alunosIds == null || !alunosIds.Any())
                    throw new ApplicationException("Nenhum aluno informado.");

                var turma = _repositorioTurma.Obtenha(turmaId);
                if (entrada <= turma.InicioDasAulas || entrada >= turma.FimDasAulas)
                    throw new ApplicationException("A data de entrada dos alunos deve estar dentro do período das aulas.");

                if (turma.Situacao != eSituacaoTurma.Ativa)
                    throw new ApplicationException("Não é possível alocar alunos em turmas que não estão ativas.");

                var alunos = _repositorio.ObtenhaLista(c => alunosIds.Contains(c.Id)).ToList();
                foreach (var aluno in alunos)
                {
                    if (aluno.ListaDeVinculosDeTurma.Any(d => d.Situacao == eSituacaoAlunoNaTurma.Cursando))
                    {
                        continue;
                    }

                    _repositorioAlunoTurma.Add(new AlunoTurma
                    {
                        RedeDeEnsinoId = turma.RedeDeEnsinoId,
                        AlunoId = aluno.Id,
                        EntradaNaTurma = entrada,
                        Situacao = eSituacaoAlunoNaTurma.Cursando,
                        TurmaId = turmaId
                    });
                }

                _repositorioAlunoTurma.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RemoverVinculoDeAluno(int alunoTurmaId)
        {
            try
            {
                var alunoTurma = _repositorioAlunoTurma.Obtenha(alunoTurmaId);
                _repositorioAlunoTurma.Remover(alunoTurma);
                _repositorio.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<AlunoDiarioDTO> ListaDeAlunosParaDiario(
            int turmaId,
            DateTime inicio,
            DateTime fim)
        {
            try
            {
                var query = @"select
	                            atu.Id as AlunoTurmaId,
                                pes.Nome as AlunoNome,
                                atu.EntradaNaTurma,
                                atu.SaidaDaTurma
                            from alunoturma atu
                            inner join aluno alu
	                            on alu.ID = atu.AlunoId
                            inner join pessoa pes
	                            on pes.Id = alu.PessoaId
                            where
	                            atu.TurmaId = @TURMAID
                                and atu.EntradaNaTurma >= @INICIO
                            order by pes.Nome";

                var alunos = _repositorio.ConsultaDapper<AlunoDiarioDTO>(query, new
                {
                    @TURMAID = turmaId,
                    @INICIO = inicio
                }).ToList();

                var frequencias =
                    _repositorio.ConsultaDapper<(int, DateTime, bool, bool)>(
                        @"(select
                            freq.AlunoTurmaId as 'Item1',
	                        freq.DataHora as 'Item2',
                            true as 'Item3',
                            freq.EstavaPresente as 'Item4'
                        from frequenciaalunoturma freq
                        where
	                        freq.AlunoTurmaId in (@LISTADEALUNOS)
                            and freq.DataHora between @INICIO and @FIM)
                        union
                        (select
                            atu.Id as 'Item1',
	                        date(reg.DataHoraRegistro) as 'Item2',
                            false as 'Item3',
                            true as 'Item4'
                        from registrodeponto reg
                        inner join equipamentodefrequencia eq
	                        on eq.Id = reg.EquipamentoDePontoId
                        inner join aluno al
	                        on al.EscolaId = eq.EscolaId and al.IdEquipamento = reg.UsuarioEquipamentoId
                        inner join alunoturma atu
	                        on atu.AlunoId = al.Id
                        where
	                        reg.DataHoraRegistro between @INICIO and @FIM
                            and atu.Id in (@LISTADEALUNOS))", new
                        {
                            @LISTADEALUNOS = alunos.Select(c => c.AlunoTurmaId),
                            @INICIO = inicio,
                            @FIM = fim
                        });

                foreach (var aluno in alunos)
                {
                    aluno.Frequencias = frequencias
                        .Where(c => c.Item1 == aluno.AlunoTurmaId)
                        .GroupBy(c => c.Item2.Date)
                        .Select(c => 
                            new KeyValuePair<DateTime, bool?>(c.Key, 
                            c.Any(d => d.Item3) ? 
                                c.OrderByDescending(d => d.Item3).First().Item4 : 
                                c.OrderBy(c => c.Item3).FirstOrDefault().Item4))
                        .ToDictionary(c => c.Key, c => c.Value);
                }

                return alunos;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}