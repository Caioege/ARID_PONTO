using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeRelatorios : IServicoDeRelatorios
    {
        private readonly IRepositorio<Servidor> _repositorio;

        public ServicoDeRelatorios(IRepositorio<Servidor> repositorio)
        {
            _repositorio = repositorio;
        }

        public void Dispose()
        {
        }

        public List<RelatorioAfastamentODTO> ObtenhaAfastamentosParaRelatorio(
            int organizacaoId,
            int? unidadeId,
            DateTime? inicio,
            DateTime? fim,
            int? justificativaId)
        {
            try
            {
                var query = @"select
	                            p.Nome as PessoaNome,
                                v.Matricula as MatriculaContrato,
                                concat('[', t.Sigla, '] ', t.Descricao) as TipoContrato,
                                v.Situacao as SituacaoContrato,
                                p.Cpf as PessoaCpf,
                                a.Inicio as InicioAfastamento,
                                a.Fim as FimAfastamento,
                                concat('[', j.Sigla, '] ', j.Descricao, ' (', if(j.Abono, 'Abono', 'Sem Abono'), ')') as JustificativaAusencia
                            from afastamento a
                            inner join vinculodetrabalho v
	                            on v.Id = a.VinculoDeTrabalhoId
                            inner join servidor s
	                            on s.Id = v.ServidorId
                            inner join pessoa p
	                            on p.Id = s.PessoaId
                            inner join tipodovinculodetrabalho t
	                            on t.Id = v.TipoDoVinculoDeTrabalhoId
                            inner join justificativadeausencia j
	                            on j.Id = a.JustificativaDeAusenciaId
                            where
	                            a.OrganizacaoId = @ORGANIZACAOID";

                if (inicio.HasValue)
                    query += " and a.Inicio >= @INICIO";

                if (fim.HasValue)
                    query += " and a.Fim <= @FIM";

                if (justificativaId.HasValue)
                    query += " and j.Id = @JUSTIFICATIVAID";

                if (unidadeId.HasValue)
                    query += " and exists (select 1 from lotacaounidadeorganizacional l where l.VinculoDeTrabalhoId = v.Id and l.UnidadeOrganizacionalId = @UNIDADEID)";

                return _repositorio.ConsultaDapper<RelatorioAfastamentODTO>(query, new 
                {
                    @ORGANIZACAOID = organizacaoId,
                    @INICIO = inicio,
                    @FIM = fim,
                    @JUSTIFICATIVAID = justificativaId,
                    @UNIDADEID = unidadeId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<ItemRelatorioServidorPorHorarioDTO> ObtenhaServidoresPorHorario(
            int organizacaoId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId)
        {
            var query = @"select
	                        p.Id as PessoaId,
                            p.Nome as PessoaNome,
                            p.Cpf as PessoaCpf,
                            v.Matricula as ContratoMatricula,
                            v.Situacao as ContratoSituacao,
                            concat('[', t.Sigla, '] ', t.Descricao) as ContratoTipo,
                            concat('[', h.Sigla, '] ', h.Descricao) as HorarioDeTrabalho
                        from vinculodetrabalho v
                        inner join servidor s
	                        on s.Id = v.ServidorId
                        inner join pessoa p
	                        on p.Id = s.PessoaId
                        inner join horariodetrabalho h
	                        on h.Id = v.HorarioDeTrabalhoId
                        inner join tipodovinculodetrabalho t
	                        on t.Id = v.TipoDoVinculoDeTrabalhoId
                        where
	                        v.OrganizacaoId = @ORGANIZACAOID";

            if (horarioDeTrabalhoId.HasValue)
                query += " and h.Id = @HORARIODETRABALHOID";

            if (tipoDeVinculoDeTrabalhoId.HasValue)
                query += " and t.Id = @TIPODEVINCULOID";

            return _repositorio.ConsultaDapper<ItemRelatorioServidorPorHorarioDTO>(query, new
            {
                @ORGANIZACAOID = organizacaoId,
                @HORARIODETRABALHOID = horarioDeTrabalhoId,
                @TIPODEVINCULOID = tipoDeVinculoDeTrabalhoId
            });
        }

        public List<ItemRelatorioServidorPorEscalaDTO> ObtenhaServidoresPorEscala(
            int organizacaoId,
            int? escalaId)
        {
            try
            {
                var query =
                    @"select
	                    s.Id as ServidorId,
                        p.Nome as PessoaNome,
                        p.Cpf as PessoaCpf,
                        v.Matricula as MatriculaVinculo,
                        concat('[', t.Sigla, '] ', t.Descricao) as TipoContrato,
                        e.Descricao as EscalaDescricao,
                        e.Id as EscalaId,
                        e.Tipo as EscalaTipo,
                        u.Id as UnidadeNome,
                        u.Nome as UnidadeNome
                    from escaladoservidor es
                    inner join vinculodetrabalho v
	                    on v.Id = es.VinculoDeTrabalhoId
                    inner join tipodovinculodetrabalho t
	                    on t.Id = v.TipoDoVinculoDeTrabalhoId
                    inner join servidor s
	                    on s.Id = v.ServidorId
                    inner join pessoa p
	                    on p.Id = s.PessoaId
                    inner join escala e
	                    on e.Id = es.EscalaId
                    inner join unidadeorganizacional u
	                    on u.Id = e.UnidadeOrganizacionalId
                    where
	                    es.OrganizacaoId = @ORGANIZACAOID";

                if (escalaId.HasValue)
                    query += " and e.Id = @ESCALAID";

                return _repositorio.ConsultaDapper<ItemRelatorioServidorPorEscalaDTO>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @ESCALAID = escalaId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}