using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeRelatorios : IServicoDeRelatorios
    {
        private readonly IRepositorio<Aluno> _repositorio;

        public ServicoDeRelatorios(IRepositorio<Aluno> repositorio)
        {
            _repositorio = repositorio;
        }

        public void Dispose()
        {
        }

        public List<RelatorioAlunosDaEscolaDTO> ObtenhaAlunosDaEscola(
            int redeDeEnsinoId,
            int? escolaId)
        {
            try
            {
                var query = @"select
	                        e.Id as EscolaId,
                            e.Nome as EscolaNome,
                            p.Nome as PessoaNome,
                            a.IdEquipamento as IdEquipamento,
                            t.Descricao as Turma,
                            t.Turno as TurmaTurno
                        from aluno a
                        inner join pessoa p on p.Id = a.PessoaId
                        inner join escola e on e.Id = a.EscolaId
                        left join alunoturma at on at.AlunoId = a.Id and at.Situacao = @SITUACAOCURSANDO
                        left join turma t on t.Id = at.TurmaId and t.EscolaId = e.Id
                        where
	                        a.RedeDeEnsinoId = @REDEDEENSINOID";

                if (escolaId.HasValue)
                    query += " and e.Id = @ESCOLAID ";

                query += " order by e.Nome, t.Descricao, p.Nome ";

                return _repositorio.ConsultaDapper<RelatorioAlunosDaEscolaDTO>(query, new 
                {
                    REDEDEENSINOID = redeDeEnsinoId,
                    @ESCOLAID = escolaId,
                    @SITUACAOCURSANDO = (int)eSituacaoAlunoNaTurma.Cursando
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RelatorioFrequenciaNaDataDTO> ObtenhaFrequenciaNaData(
            int redeDeEnsinoId,
            int escolaId,
            DateTime data)
        {
            var query = @"select
	                        e.Id as EscolaId,
                            e.Nome as EscolaNome,
                            p.Nome as PessoaNome,
                            a.IdEquipamento as IdEquipamento,
                            t.Descricao as Turma,
                            t.Turno as TurmaTurno,
                            fa.EstavaPresente as PresencaoDiarioDeClasse,
                            if(r.DataHoraRegistro is not null, true, false) as PresencaEquipamento
                        from aluno a
                        inner join pessoa p on p.Id = a.PessoaId
                        inner join escola e on e.Id = a.EscolaId
                        inner join alunoturma at on at.AlunoId = a.Id and at.Situacao = @SITUACAOCURSANDO
                        inner join turma t on t.Id = at.TurmaId and t.EscolaId = e.Id
                        left join frequenciaalunoturma fa on fa.AlunoTurmaId = a.Id and date(fa.DataHora) = @DATAFILTRO
                        left join 
	                        (select
		                        eq.EscolaId,
		                        re.DataHoraRegistro,
		                        re.UsuarioEquipamentoId
	                        from registrodeponto re
	                        inner join equipamentodefrequencia eq on eq.Id = re.EquipamentoDeFrequenciaId
	                        where
		                        eq.EscolaId = @ESCOLAID
		                        and date(re.DataHoraRegistro) = @DATAFILTRO) r on r.EscolaId = e.Id and r.UsuarioEquipamentoId = a.IdEquipamento
                        where
	                        a.RedeDeEnsinoId = @REDEDEENSINOID
                            and e.Id = @ESCOLAID
                            and @DATAFILTRO between date(at.EntradaNaTurma) and coalesce(at.SaidaDaTurma, now())
                        order by e.Nome, t.Descricao, p.Nome;";

            return _repositorio.ConsultaDapper<RelatorioFrequenciaNaDataDTO>(query, new
            {
                @REDEDEENSINOID = redeDeEnsinoId,
                @ESCOLAID = escolaId,
                @DATAFILTRO = data.Date
            });
        }

        public List<RelatorioEquipamentoDaEscolaDTO> ObtenhaEquipamentosDaEscola(
            int redeDeEnsinoId,
            int? escolaId)
        {
            try
            {
                var query = @"select
	                            e.Id as EscolaId,
                                e.Nome as EscolaNome,
                                eq.Descricao as EquipamentoDescricao,
                                eq.Ativo as EquipamentoAtivo,
                                eq.NumeroDeSerie as EquipamentoNumeroDeSerie
                            from equipamentodefrequencia eq
                            inner join escola e
	                            on e.Id = eq.EscolaId
                            where
	                            eq.RedeDeEnsinoId = @REDEDEENSINOID";

                if (escolaId.HasValue)
                    query += " and eq.EscolaId = @ESCOLAID ";

                query += " order by e.Nome, eq.Descricao, eq.Ativo desc ";

                return _repositorio.ConsultaDapper<RelatorioEquipamentoDaEscolaDTO>(query, new
                {
                    REDEDEENSINOID = redeDeEnsinoId,
                    @ESCOLAID = escolaId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RegistroDePontoIndexDTO> ObtenhaRegistrosDeFrequencia(
            int redeDeEnsinoId,
            int? escolaId,
            DateTime dataInicio,
            DateTime dataFim)
        {
            try
            {
                var fromAndWhere = @"
                            from registrodeponto reg
                            inner join EquipamentoDeFrequencia eq
	                            on eq.Id = reg.EquipamentoDeFrequenciaId
                            inner join escola uni
	                            on uni.Id = eq.EscolaId
                            left join aluno alu
	                            on alu.EscolaId = uni.Id and alu.IdEquipamento = reg.UsuarioEquipamentoId
                            left join pessoa pes
	                            on pes.Id = alu.PessoaId
                            where
	                            reg.RedeDeEnsinoId = @REDEDEENSINOID
                                and date(reg.DataHoraRegistro) between @INICIO and @FIM";

                if (escolaId.HasValue)
                    fromAndWhere += " and uni.Id = @ESCOLAID";

                var select = $@"select
	                            reg.Id as Id,
                                eq.Id as EquipamentoId,
                                eq.Descricao as EquipamentoDescricao,
                                reg.DataHoraRegistro as DataHoraRegistro,
                                reg.DataHoraRecebimento as DataHoraRecebimento,
                                reg.UsuarioEquipamentoId as IdEquipamento,
                                pes.Nome as PessoaNome,
                                uni.Id as EscolaId,
                                uni.Nome as EscolaNome
                            {fromAndWhere}
                            order by reg.DataHoraRegistro, reg.DataHoraRecebimento, pes.Nome";

                var itens = _repositorio.ConsultaDapper<RegistroDePontoIndexDTO>(select, new
                {
                    @REDEDEENSINOID = redeDeEnsinoId,
                    @ESCOLAID = escolaId,
                    @INICIO = dataInicio,
                    @FIM = dataFim
                });

                return itens;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}