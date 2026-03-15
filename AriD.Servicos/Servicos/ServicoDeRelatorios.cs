using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeRelatorios : IServicoDeRelatorios
    {
        private readonly IRepositorio<Servidor> _repositorio;
        private readonly IRepositorio<VinculoDeTrabalho> _repositorioVinculo;

        public ServicoDeRelatorios(IRepositorio<Servidor> repositorio, IRepositorio<VinculoDeTrabalho> repositorioVinculo)
        {
            _repositorio = repositorio;
            _repositorioVinculo = repositorioVinculo;
        }

        public void Dispose()
        {
        }

        public List<RelatorioAfastamentODTO> ObtenhaAfastamentosParaRelatorio(
            int organizacaoId,
            int? unidadeId,
            DateTime? inicio,
            DateTime? fim,
            int? justificativaId,
            int? departamentoId)
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

                if (departamentoId.HasValue)
                    query += " and v.DepartamentoId = @DEPARTAMENTOID ";

                return _repositorio.ConsultaDapper<RelatorioAfastamentODTO>(query, new 
                {
                    @ORGANIZACAOID = organizacaoId,
                    @INICIO = inicio,
                    @FIM = fim,
                    @JUSTIFICATIVAID = justificativaId,
                    @UNIDADEID = unidadeId,
                    @DEPARTAMENTOID = departamentoId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RelatorioDemitidosDTO> ObtenhaServidoresDemitidosPorPeriodo(
            int organizacaoId,
            int? unidadeId,
            DateTime? inicio,
            DateTime? fim,
            int? motivoDeDemissaoId,
            int? departamentoId)
        {
            try
            {
                var query = @"select
		                        p.Nome as PessoaNome,
		                        v.Matricula as MatriculaContrato,
		                        concat('[', t.Sigla, '] ', t.Descricao) as TipoContrato,
		                        v.Situacao as SituacaoContrato,
		                        p.Cpf as PessoaCpf,
                                m.Id as MotivoDeDemissaoId,
                                m.Descricao as MotivoDeDemissaoDescricao,
                                v.DataDemissao as DataDaDemissao,
                                v.ObservacoesDaDemissao as Observacoes
	                        from vinculodetrabalho v
	                        inner join servidor s
		                        on s.Id = v.ServidorId
	                        inner join pessoa p
		                        on p.Id = s.PessoaId
	                        inner join tipodovinculodetrabalho t
		                        on t.Id = v.TipoDoVinculoDeTrabalhoId
	                        inner join motivodedemissao m
		                        on m.Id = v.MotivoDeDemissaoId
	                        where
		                        v.OrganizacaoId = @ORGANIZACAOID";

                if (inicio.HasValue)
                    query += " and v.DataDemissao >= @INICIO";

                if (fim.HasValue)
                    query += " and v.DataDemissao <= @FIM";

                if (motivoDeDemissaoId.HasValue)
                    query += " and m.Id = @MOTIVOID";

                if (unidadeId.HasValue)
                    query += " and exists (select 1 from lotacaounidadeorganizacional l where l.VinculoDeTrabalhoId = v.Id and l.UnidadeOrganizacionalId = @UNIDADEID)";

                if (departamentoId.HasValue)
                    query += " and v.DepartamentoId = @DEPARTAMENTOID ";

                return _repositorio.ConsultaDapper<RelatorioDemitidosDTO>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @INICIO = inicio,
                    @FIM = fim,
                    @MOTIVOID = motivoDeDemissaoId,
                    @UNIDADEID = unidadeId,
                    @DEPARTAMENTOID = departamentoId
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
            int? tipoDeVinculoDeTrabalhoId,
            int? unidadeId,
            int? departamentoId)
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

            if (unidadeId.HasValue)
                query += " and exists (select 1 from lotacaounidadeorganizacional l where l.VinculoDeTrabalhoId = v.Id and l.UnidadeOrganizacionalId = @UNIDADEID)";

            if (departamentoId.HasValue)
                query += " and v.DepartamentoId = @DEPARTAMENTOID ";

            return _repositorio.ConsultaDapper<ItemRelatorioServidorPorHorarioDTO>(query, new
            {
                @ORGANIZACAOID = organizacaoId,
                @HORARIODETRABALHOID = horarioDeTrabalhoId,
                @TIPODEVINCULOID = tipoDeVinculoDeTrabalhoId,
                @DEPARTAMENTOID = departamentoId
            });
        }

        public List<ItemRelatorioServidorPorEscalaDTO> ObtenhaServidoresPorEscala(
            int organizacaoId,
            int? escalaId,
            int? departamentoId)
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

                if (departamentoId.HasValue)
                    query += " and v.DepartamentoId = @DEPARTAMENTOID ";

                return _repositorio.ConsultaDapper<ItemRelatorioServidorPorEscalaDTO>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @ESCALAID = escalaId,
                    @DEPARTAMENTOID = departamentoId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RelatorioItemListaServidorDTO> ObtenhaListaDeServidores(
            int organizacaoId,
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? departamentoId)
        {
            try
            {
                var query =
                    @"select
                        s.Id as ServidorId,
                        p.Nome as PessoaNome,
                        p.Cpf as PessoaCpf,
                        p.DataDeNascimento as DataDeNascimento
                    from pessoa p
                    inner join servidor s
                        on s.PessoaId = p.Id
                    where
                        p.OrganizacaoId = @ORGANIZACAOID ";

                if (unidadeId.HasValue)
                    query += 
                            @" and exists (SELECT 
                                1
                            FROM
                                lotacaounidadeorganizacional l
                            inner join vinculodetrabalho v
	                            on v.Id = l.VinculoDeTrabalhoId
                            WHERE
                                v.ServidorId = s.Id
	                            AND l.UnidadeOrganizacionalId = @UNIDADEID)";

                if (departamentoId.HasValue)
                    query +=
                            @" and exists (SELECT 
                                1
                            FROM
                                vinculodetrabalho v
                            WHERE
                                v.ServidorId = s.Id
	                            AND v.DepartamentoId = @DEPARTAMENTOID)";

                if (horarioDeTrabalhoId.HasValue)
                    query +=
                            @" and exists (SELECT 
                                1
                            FROM
                                vinculodetrabalho v
                            WHERE
                                v.ServidorId = s.Id
	                            AND v.HorarioDeTrabalhoId = @HORARIOID)";

                if (tipoDeVinculoDeTrabalhoId.HasValue)
                    query +=
                            @" and exists (SELECT 
                                1
                            FROM
                                vinculodetrabalho v
                            WHERE
                                v.ServidorId = s.Id
	                            AND v.TipoDoVinculoDeTrabalhoId = @TIPOID)";

                query += " order by p.Nome ";

                return _repositorio.ConsultaDapper<RelatorioItemListaServidorDTO>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @TIPOID = tipoDeVinculoDeTrabalhoId,
                    @HORARIOID = horarioDeTrabalhoId,
                    @DEPARTAMENTOID = departamentoId,
                    @UNIDADEID = unidadeId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<VinculoDeTrabalho> ObtenhaListaDeVinculos(
            int organizacaoId,
            int unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? departamentoId)
        {
            try
            {
                var query = $@"select
	                            v.Id
                            from vinculodetrabalho v
                            where
	                             v.OrganizacaoId = @ORGANIZACAOID
                                {(horarioDeTrabalhoId.HasValue ? "and v.HorarioDeTrabalhoId = @HORARIOID" : string.Empty)}
                                {(departamentoId.HasValue ? "and v.DepartamentoId = @DEPARTAMENTOID" : string.Empty)}
                                {(tipoDeVinculoDeTrabalhoId.HasValue ? "and v.TipoDoVinculoDeTrabalhoId = @TIPOID" : string.Empty)}
                                and exists (select 1 from lotacaounidadeorganizacional lt where lt.VinculoDeTrabalhoId = v.Id and lt.UnidadeOrganizacionalId = @UNIDADEID)";

                var lista = _repositorio.ConsultaDapper<int>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @HORARIOID = horarioDeTrabalhoId,
                    @DEPARTAMENTOID = departamentoId,
                    @TIPOID = tipoDeVinculoDeTrabalhoId,
                    @UNIDADEID = unidadeId
                });

                if (!lista.Any())
                    return new List<VinculoDeTrabalho>();

                return _repositorioVinculo.ObtenhaLista(c => lista.Contains(c.Id));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<EventoAnual> ObtenhaListaDeEventosDaOrganizacao(int organizacaoId)
        {
            try
            {
                var query =
                    @"select
	                        *
                        from eventoanual
                        where
	                        OrganizacaoId = @ORGANIZACAOID
                        order by Data";

                return _repositorio.ConsultaDapper<EventoAnual>(query, new { @ORGANIZACAOID = organizacaoId });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RelatorioConferenciaDePontoDTO> ObtenhaListaDeDadosParaConferenciaDePonto(
            int organizacaoId,
            int unidadeOrganizacionalId,
            DateTime data,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? departamentoId)
        {
            try
            {
                return _repositorio.ConsultaDapper<RelatorioConferenciaDePontoDTO>(
                    $@"(select
	                    ser.Id as ServidorId,
                        pes.Nome as ServidorNome,
                        pes.Cpf as ServidorCpf,
                        reg.DataHoraRegistro as DataHora,
                        'Aplicativo' as Origem,
                        ra.Latitude,
                        ra.Longitude,
                        ra.ForaDaCerca,
                        ra.Situacao
                    from registrodeponto reg
                    inner join registroaplicativo ra
	                    on ra.Id = reg.RegistroAplicativoId
                    inner join vinculodetrabalho vin
	                    on vin.Id = ra.VinculoDeTrabalhoId
                    inner join servidor ser
	                    on ser.Id = vin.ServidorId
                    inner join pessoa pes
	                    on pes.Id = ser.PessoaId
                    where
	                    reg.OrganizacaoId = @ORGANIZACAOID
	                    and date(reg.DataHoraRegistro) = @DATA
                        and ra.Manual = false
                        and ra.JustificativaDeAusenciaId = false
                        {(tipoDeVinculoDeTrabalhoId.HasValue ? "and vin.TipoDoVinculoDeTrabalhoId = @TIPOID" : string.Empty)}
                        {(departamentoId.HasValue ? "and vin.DepartamentoId = @DEPARTAMENTOID" : string.Empty)}
                        and exists (select 1 from lotacaounidadeorganizacional l where l.VinculoDeTrabalhoId = vin.Id and l.UnidadeOrganizacionalId = @UNIDADEID)
                        {(horarioDeTrabalhoId.HasValue ? "and vin.HorarioDeTrabalhoId = @HORARIOID" : string.Empty)})
                    union
                    (select
	                    ser.Id as ServidorId,
                        pes.Nome as ServidorNome,
                        pes.Cpf as ServidorCpf,
                        reg.DataHoraRegistro as DataHora,
                        eq.Descricao as Origem,
                        null as Latitude,
                        null as Longitude,
                        false as ForaDaCerca,
                        @SITUACAOAPROVADO as Situacao
                    from registrodeponto reg
                    inner join equipamentodeponto eq
	                    on eq.Id = reg.EquipamentoDePontoId
                    inner join lotacaounidadeorganizacional lot
	                    on lot.MatriculaEquipamento = reg.UsuarioEquipamentoId
                        and lot.UnidadeOrganizacionalId = eq.UnidadeOrganizacionalId
                        and reg.DataHoraRegistro between lot.Entrada and coalesce(lot.Saida, reg.DataHoraRegistro)
                    inner join vinculodetrabalho vin
	                    on vin.Id = lot.VinculoDeTrabalhoId
                    inner join servidor ser
	                    on ser.Id = vin.ServidorId
                    inner join pessoa pes
	                    on pes.Id = ser.PessoaId
                    where
	                    reg.OrganizacaoId = @ORGANIZACAOID
                        and date(reg.DataHoraRegistro) = @DATA
                        {(horarioDeTrabalhoId.HasValue ? "and vin.HorarioDeTrabalhoId = @HORARIOID" : string.Empty)}
                        and eq.UnidadeOrganizacionalId = @UNIDADEID
                        {(departamentoId.HasValue ? "and vin.DepartamentoId = @DEPARTAMENTOID" : string.Empty)}
                        {(tipoDeVinculoDeTrabalhoId.HasValue ? "and vin.TipoDoVinculoDeTrabalhoId = @TIPOID" : string.Empty)})",
                    new
                    {
                        @ORGANIZACAOID = organizacaoId,
                        @UNIDADEID = unidadeOrganizacionalId,
                        @DATA = data.Date,
                        @HORARIOID = horarioDeTrabalhoId,
                        @TIPOID = tipoDeVinculoDeTrabalhoId,
                        @DEPARTAMENTOID = departamentoId,
                        @SITUACAOAPROVADO = eSituacaoRegistroAplicativo.Aprovado
                    });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RelatorioServidorPorLotacaoDTO> ObtenhaListaDeDadosPorLotacao(
            int organizacaoId,
            int? unidadeOrganizacionalId,
            DateTime? entrada,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? departamentoId)
        {
            try
            {
                return _repositorio.ConsultaDapper<RelatorioServidorPorLotacaoDTO>(
                    $@"select
	                    ser.Id as ServidorId,
                        pes.Nome as ServidorNome,
                        pes.Cpf as ServidorCpf,
                        uni.Id as UnidadeId,
                        uni.Nome as UnidadeNome,
                        en.Logradouro,
                        en.Bairro,
                        en.Cep,
                        en.Cidade,
                        en.UF,
                        lot.Entrada,
                        vin.Matricula as VinculoMatricula,
                        hor.Descricao as HorarioDeTrabalho,
                        tip.Descricao as TipoVinculo
                    from servidor ser
                    inner join vinculodetrabalho vin
	                    on vin.ServidorId = ser.Id
                    inner join lotacaounidadeorganizacional lot
	                    on lot.VinculoDeTrabalhoId = vin.Id
                    inner join unidadeorganizacional uni
	                    on uni.Id = lot.UnidadeOrganizacionalId
                    inner join horariodetrabalho hor
	                    on hor.Id = vin.HorarioDeTrabalhoId
                    inner join tipodovinculodetrabalho tip
	                    on tip.Id = vin.TipoDoVinculoDeTrabalhoId
                    inner join pessoa pes
	                    on pes.Id = ser.PessoaId
                    left join endereco en
	                    on en.Id = uni.EnderecoId
                    where
	                    ser.OrganizacaoId = @ORGANIZACAOID
                        {(unidadeOrganizacionalId.HasValue ? "and  lot.UnidadeOrganizacionalId = @UNIDADEID" : string.Empty)}
                        {(horarioDeTrabalhoId.HasValue ? "and hor.Id = @HORARIOID" : string.Empty)}
                        {(tipoDeVinculoDeTrabalhoId.HasValue ? "and tip.Id = @TIPOID" : string.Empty)}
                        {(departamentoId.HasValue ? "and vin.DepartamentoId = @DEPARTAMENTOID" : string.Empty)}
                        {(entrada.HasValue ? "and lot.Entrada >= @ENTRADA" : string.Empty)}
                        and vin.Situacao = @ATIVA
                        and lot.Saida is null
                    order by uni.Nome, pes.Nome, lot.Entrada", new
                    {
                        @ORGANIZACAOID = organizacaoId,
                        @UNIDADEID = unidadeOrganizacionalId,
                        @HORARIOID = horarioDeTrabalhoId,
                        @TIPOID = tipoDeVinculoDeTrabalhoId,
                        @DEPARTAMENTOID = departamentoId,
                        @ENTRADA = entrada,
                        @ATIVA = eSituacaoVinculoDeTrabalho.Normal
                    });
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<RelatorioAbsenteismoDTO> ObtenhaRelatorioDeAbsenteismo(
            int organizacaoId,
            int? unidadeId,
            DateTime inicio,
            DateTime fim,
            int? departamentoId)
        {
            try
            {
                var query = @"select 
                                v.Id as VinculoDeTrabalhoId,
                                v.Matricula,
                                pe.Nome as NomeServidor,
                                dep.Descricao as Departamento,
                                f.Descricao as Funcao,
                                p.Data,
                                p.HorasNegativas,
                                IF(p.HorasTrabalhadasConsiderandoAbono is null or p.HorasTrabalhadasConsiderandoAbono = '00:00:00', 'Falta Integral', 'Falta Parcial/Atraso') as TipoAusencia,
                                TIME_FORMAT(p.HorasNegativas, '%H:%i') as TotalAtrasoOuFalta
                            from pontododia p
                            inner join vinculodetrabalho v on v.Id = p.VinculoDeTrabalhoId
                            inner join servidor s on s.Id = v.ServidorId
                            inner join pessoa pe on pe.Id = s.PessoaId
                            left join departamento dep on dep.Id = v.DepartamentoId
                            left join funcao f on f.Id = v.FuncaoId
                            where p.OrganizacaoId = @ORG
                              and p.Data between @INICIO and @FIM
                              and p.HorasNegativas is not null and p.HorasNegativas > '00:00:00'";

                if (unidadeId.HasValue)
                {
                    query += " and exists (select 1 from lotacaounidadeorganizacional l where l.VinculoDeTrabalhoId = v.Id and l.UnidadeOrganizacionalId = @UNIDADEID)";
                }

                if (departamentoId.HasValue)
                {
                    query += " and v.DepartamentoId = @DEPARTAMENTOID";
                }

                query += " order by pe.Nome, p.Data";

                return _repositorio.ConsultaDapper<RelatorioAbsenteismoDTO>(query, new
                {
                    @ORG = organizacaoId,
                    @INICIO = inicio.Date,
                    @FIM = fim.Date,
                    @UNIDADEID = unidadeId,
                    @DEPARTAMENTOID = departamentoId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public List<RelatorioAuditoriaDeAusenciasDTO> ObtenhaRelatorioDeAuditoriaDeAusencias(
            int organizacaoId,
            DateTime? inicioAfastamento,
            DateTime? fimAfastamento,
            int? unidadeLotacaoId)
        {
            try
            {
                var query = $@"
                    SELECT 
                        pes.Nome as ServidorNome,
                        jus.Descricao as Justificativa,
                        af.Inicio as InicioAfastamento,
                        af.Fim as FimAfastamento,
                        log.UsuarioNome as OperadorNome,
                        log.DataHora as DataHoraAcao,
                        log.Acao as Acao,
                        log.Descricao as Descricao
                    FROM LogAuditoriaPonto log
                    INNER JOIN VinculoDeTrabalho v ON v.Id = log.VinculoDeTrabalhoId
                    INNER JOIN Servidor s ON s.Id = v.ServidorId
                    INNER JOIN Pessoa pes ON pes.Id = s.PessoaId
                    LEFT JOIN PontoDoDia pd ON pd.Id = log.PontoDoDiaId
                    LEFT JOIN Afastamento af ON af.Id = pd.AfastamentoId
                    LEFT JOIN JustificativaDeAusencia jus ON jus.Id = af.JustificativaDeAusenciaId
                    WHERE log.OrganizacaoId = @ORG
                    AND (log.Acao LIKE '%Afastamento%' OR log.Acao LIKE '%Ausência%')
                    {(inicioAfastamento.HasValue ? "AND af.Inicio >= @INICIO" : "")}
                    {(fimAfastamento.HasValue ? "AND (af.Fim IS NULL OR af.Fim <= @FIM)" : "")}
                    {(unidadeLotacaoId.HasValue ? "AND EXISTS (SELECT 1 FROM LotacaoUnidadeOrganizacional l WHERE l.VinculoDeTrabalhoId = v.Id AND l.UnidadeOrganizacionalId = @UNID)" : "")}
                    ORDER BY log.DataHora DESC";

                return _repositorio.ConsultaDapper<RelatorioAuditoriaDeAusenciasDTO>(query, new 
                { 
                    @ORG = organizacaoId, 
                    @INICIO = inicioAfastamento, 
                    @FIM = fimAfastamento, 
                    @UNID = unidadeLotacaoId 
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}