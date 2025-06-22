using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeFolhaDePonto : Servico<PontoDoDia>, IServicoDeFolhaDePonto
    {
        private readonly IRepositorio<PontoDoDia> _repositorio;
        private readonly IRepositorio<Servidor> _repositorioServidor;
        private readonly IRepositorio<LotacaoUnidadeOrganizacional> _repositorioLotacao;
        private readonly IRepositorio<JustificativaDeAusencia> _repositorioJustificativa;
        private readonly IRepositorio<VinculoDeTrabalho> _repositorioVinculo;
        private readonly IRepositorio<Afastamento> _repositorioAfastamento;
        private readonly IRepositorio<EscalaDoServidor> _repositorioEscala;
        private readonly IRepositorio<RegistroAplicativo> _repositorioRegistroAplicativo;
        private readonly IRepositorio<RegistroDePonto> _repositorioRegistroDePonto;

        public ServicoDeFolhaDePonto(
            IRepositorio<PontoDoDia> repositorio,
            IRepositorio<Servidor> repositorioServidor,
            IRepositorio<LotacaoUnidadeOrganizacional> repositorioLotacao,
            IRepositorio<JustificativaDeAusencia> repositorioJustificativa,
            IRepositorio<VinculoDeTrabalho> repositorioVinculo,
            IRepositorio<Afastamento> repositorioAfastamento,
            IRepositorio<EscalaDoServidor> repositorioEscala,
            IRepositorio<RegistroAplicativo> repositorioRegistroAplicativo,
            IRepositorio<RegistroDePonto> repositorioRegistroDePonto)
            : base(repositorio)
        {
            _repositorio = repositorio;
            _repositorioServidor = repositorioServidor;
            _repositorioLotacao = repositorioLotacao;
            _repositorioJustificativa = repositorioJustificativa;
            _repositorioVinculo = repositorioVinculo;
            _repositorioAfastamento = repositorioAfastamento;
            _repositorioEscala = repositorioEscala;
            _repositorioRegistroAplicativo = repositorioRegistroAplicativo;
            _repositorioRegistroDePonto = repositorioRegistroDePonto;
        }

        public (List<CodigoDescricaoDTO> Horarios, List<CodigoDescricaoDTO> Funcoes, List<CodigoDescricaoDTO> Departamentos) ObtenhaFiltrosPontoDia(
            int organizacaoId,
            int unidadeOrganizacionalId)
        {
            try
            {
                var query = @"select
	                            h.Id as Codigo,
                                concat('[', h.Sigla, '] ', h.Descricao) as Descricao
                            from lotacaounidadeorganizacional l
                            inner join vinculodetrabalho v
	                            on v.Id = l.VinculoDeTrabalhoId
                            inner join horariodetrabalho h
	                            on h.Id = v.HorarioDeTrabalhoId
                            where
	                            l.OrganizacaoId = @ORGANIZACAOID
                                and l.UnidadeOrganizacionalId = @UNIDADEID
                            group by 1
                            order by h.Sigla, h.Descricao";

                var parametros = new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @UNIDADEID = unidadeOrganizacionalId
                };

                var horarios = _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, parametros);

                query = @"select
	                        f.Id as Codigo,
                            concat('[', f.Sigla, '] ', f.Descricao) as Descricao
                        from lotacaounidadeorganizacional l
                        inner join vinculodetrabalho v
	                        on v.Id = l.VinculoDeTrabalhoId
                        inner join funcao f
	                        on f.Id = v.FuncaoId
                        where
	                        l.OrganizacaoId = @ORGANIZACAOID
                            and l.UnidadeOrganizacionalId = @UNIDADEID
                        group by 1
                        order by f.Sigla, f.Descricao;";

                var funcoes = _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, parametros);

                query = @"select
	                        d.Id as Codigo,
                            concat('[', d.Sigla, '] ', d.Descricao) as Descricao
                        from lotacaounidadeorganizacional l
                        inner join vinculodetrabalho v
	                        on v.Id = l.VinculoDeTrabalhoId
                        inner join departamento d
	                        on d.Id = v.DepartamentoId
                        where
	                        l.OrganizacaoId = @ORGANIZACAOID
                            and l.UnidadeOrganizacionalId = @UNIDADEID
                        group by 1
                        order by d.Sigla, d.Descricao;";

                var departamentos = _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, parametros);

                return
                (
                    horarios,
                    funcoes,
                    departamentos
                );
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PontoDoDia> ObtenhaPontosDoDia(
            DateTime data,
            int organizacaoId,
            int unidadeOrganizacionalId,
            int horarioDeTrabalhoId,
            int? funcaoId,
            int? departamentoId)
        {
            try
            {
                var lotacoes = _repositorioLotacao
                    .ObtenhaLista(c => 
                    c.OrganizacaoId == organizacaoId &&
                        c.VinculoDeTrabalho.HorarioDeTrabalhoId == horarioDeTrabalhoId &&
                        (!c.Saida.HasValue || c.Saida >= data) &&
                        c.UnidadeOrganizacionalId == unidadeOrganizacionalId);

                if (funcaoId.HasValue)
                    lotacoes.RemoveAll(c => c.VinculoDeTrabalho.FuncaoId != funcaoId);

                if (departamentoId.HasValue)
                    lotacoes.RemoveAll(c => c.VinculoDeTrabalho.DepartamentoId != departamentoId);

                if (lotacoes == null || !lotacoes.Any())
                    throw new ApplicationException("Nenhuma lotação encontrada.");

                var vinculosIds = lotacoes.Select(c => c.VinculoDeTrabalhoId).Distinct();

                var pontosDoDia = _repositorio
                    .ObtenhaLista(c => 
                    c.Data == data &&
                    vinculosIds.Contains(c.VinculoDeTrabalhoId));

                foreach (var vinculoDeTrabalho in lotacoes.GroupBy(c =>  c.VinculoDeTrabalhoId))
                {
                    if (!pontosDoDia.Any(c => c.VinculoDeTrabalhoId == vinculoDeTrabalho.Key))
                    {
                        pontosDoDia.Add(new PontoDoDia
                        {
                            VinculoDeTrabalho = vinculoDeTrabalho.First().VinculoDeTrabalho,
                            VinculoDeTrabalhoId = vinculoDeTrabalho.Key,
                            Data = data,
                            OrganizacaoId = organizacaoId
                        });
                    }
                }

                return pontosDoDia
                    .OrderBy(c => c.VinculoDeTrabalho.Servidor.Nome)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PontoDoDia ObtenhaPontoDoDia(int vinculoDeTrabalhoId, DateTime data) 
        {
            try
            {
                return _repositorio.Obtenha(c =>
                        c.VinculoDeTrabalhoId == vinculoDeTrabalhoId &&
                        c.Data == data) ??
                    new PontoDoDia
                    {
                        VinculoDeTrabalhoId = vinculoDeTrabalhoId,
                        Data = data
                    };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PontoDoDia AtualizePontoDoDia(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            DateTime data,
            TimeSpan? valorHora,
            int? justificativaId,
            string acao)
        {
            try
            {
                var pontoDoDia = ObtenhaPontoDoDia(vinculoDeTrabalhoId, data);
                pontoDoDia.OrganizacaoId = organizacaoId;

                if (pontoDoDia.Id == 0)
                    pontoDoDia.VinculoDeTrabalho = _repositorioVinculo.Obtenha(vinculoDeTrabalhoId);

                Func<int?, JustificativaDeAusencia> CarregueJustificativa = new Func<int?, JustificativaDeAusencia>((id) =>
                {
                    return id.HasValue ?
                        _repositorioJustificativa.Obtenha(id.Value) :
                        null;
                });

                switch (acao.ToLower())
                {
                    case "entrada1":
                        pontoDoDia.Entrada1 = valorHora;
                        pontoDoDia.JustificativaPeriodo1Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo1 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada1 != valorHora)))
                        {
                            pontoDoDia.TipoEntrada1 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoEntrada1 = eTipoDeRegistroDePeriodo.SemRegistro;

                        break;
                    case "saida1":
                        pontoDoDia.Saida1 = valorHora;
                        pontoDoDia.JustificativaPeriodo1Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo1 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida1 != valorHora)))
                        {
                            pontoDoDia.TipoSaida1 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoSaida1 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;

                    case "entrada2":
                        pontoDoDia.Entrada2 = valorHora;
                        pontoDoDia.JustificativaPeriodo2Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo2 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada2 != valorHora)))
                        {
                            pontoDoDia.TipoEntrada2 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoEntrada2 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;
                    case "saida2":
                        pontoDoDia.Saida2 = valorHora;
                        pontoDoDia.JustificativaPeriodo2Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo2 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida2 != valorHora)))
                        {
                            pontoDoDia.TipoSaida2 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoSaida2 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;

                    case "entrada3":
                        pontoDoDia.Entrada3 = valorHora;
                        pontoDoDia.JustificativaPeriodo3Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo3 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada3 != valorHora)))
                        {
                            pontoDoDia.TipoEntrada3 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoEntrada3 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;
                    case "saida3":
                        pontoDoDia.Saida3 = valorHora;
                        pontoDoDia.JustificativaPeriodo3Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo3 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida3 != valorHora)))
                        {
                            pontoDoDia.TipoSaida3 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoSaida3 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;

                    case "entrada4":
                        pontoDoDia.Entrada4 = valorHora;
                        pontoDoDia.JustificativaPeriodo4Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo4 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada4 != valorHora)))
                        {
                            pontoDoDia.TipoEntrada4 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoEntrada4 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;
                    case "saida4":
                        pontoDoDia.Saida4 = valorHora;
                        pontoDoDia.JustificativaPeriodo4Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo4 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida4 != valorHora)))
                        {
                            pontoDoDia.TipoSaida4 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoSaida4 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;

                    case "entrada5":
                        pontoDoDia.Entrada5 = valorHora;
                        pontoDoDia.JustificativaPeriodo5Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo5 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada5 != valorHora)))
                        {
                            pontoDoDia.TipoEntrada5 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoEntrada5 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;
                    case "saida5":
                        pontoDoDia.Saida5 = valorHora;
                        pontoDoDia.JustificativaPeriodo5Id = justificativaId;
                        pontoDoDia.JustificativaPeriodo5 = CarregueJustificativa(justificativaId);

                        if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida5 != valorHora)))
                        {
                            pontoDoDia.TipoSaida5 = eTipoDeRegistroDePeriodo.RegistroManual;
                        }
                        else if (!valorHora.HasValue)
                            pontoDoDia.TipoSaida5 = eTipoDeRegistroDePeriodo.SemRegistro;
                        break;

                    case "abono":
                        pontoDoDia.Abono = valorHora;
                        break;
                }

                if (pontoDoDia.Id > 0)
                {
                    _repositorio.Atualizar(pontoDoDia);
                }
                else
                {
                    _repositorio.Add(pontoDoDia);
                }

                _repositorio.Commit();

                return pontoDoDia;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<CodigoDescricaoDTO> ObtenhaServidoresLotadosNaUnidade(
            int organizacaoId,
            int unidadeId,
            int? departamentoId)
        {
            try
            {
                var query = @"select
	                            s.Id as Codigo,
                                if(p.Cpf is not null, concat(p.Nome, ' (', p.Cpf, ')'), p.Nome) as Descricao
                            from lotacaounidadeorganizacional l
                            inner join vinculodetrabalho v
	                            on v.Id = l.VinculoDeTrabalhoId
                            inner join servidor s
	                            on s.Id = v.ServidorId
                            inner join pessoa p 
	                            on p.Id = s.PessoaId
                            where
	                            l.UnidadeOrganizacionalId = @UNIDADEID
                                and s.OrganizacaoId = @ORGANIZACAOID ";

                if (departamentoId.HasValue)
                    query += " and v.DepartamentoId = @DEPARTAMENTOID ";

                query += " order by p.Nome";

                var servidores = _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, new
                {
                    @UNIDADEID = unidadeId,
                    @ORGANIZACAOID = organizacaoId,
                    @DEPARTAMENTOID = departamentoId
                });

                return servidores
                    .DistinctBy(c => c.Codigo)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<CodigoDescricaoDTO> ObtenhaVinculosDeTrabalhoDoServido(
            int organizacaoId,
            int servidorId,
            int unidadeId,
            int? departamentoId)
        {
            try
            {
                var query = @"select
	                            v.Id as Codigo,
                                concat(v.Matricula, ' - ', t.Descricao) as Descricao
                            from lotacaounidadeorganizacional l
                            inner join vinculodetrabalho v
	                            on v.Id = l.VinculoDeTrabalhoId
                            inner join tipodovinculodetrabalho t
	                            on t.Id = v.TipoDoVinculoDeTrabalhoId
                            where
	                            l.OrganizacaoId = @ORGANIZACAOID
                                and v.ServidorId = @SERVIDORID
                                and l.UnidadeOrganizacionalId = @UNIDADEID";

                if (departamentoId.HasValue)
                    query += " and v.DepartamentoId = @DEPARTAMENTOID ";

                query += " order by v.Inicio, v.Matricula";

                var vinculos = _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, new
                {
                    @UNIDADEID = unidadeId,
                    @ORGANIZACAOID = organizacaoId,
                    @SERVIDORID = servidorId,
                    @DEPARTAMENTOID = departamentoId
                });

                return vinculos
                    .DistinctBy(c => c.Codigo)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PontoDoDia> CarregueFolhaDePonto(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            int unidadeLotacaoId,
            MesAno mesAno)
        {
            try
            {
                ObtenhaRegistrosDePonto(
                    organizacaoId, 
                    vinculoDeTrabalhoId, 
                    unidadeLotacaoId, 
                    mesAno, 
                    out var vinculoDeTrabalho, 
                    out var inicio, 
                    out var  fim, 
                    out var pontosDoPeriodo);

                List<RegistroDePonto> registrosDePonto = ObtenhaRegistrosDePontoDoPeriodo(vinculoDeTrabalhoId, unidadeLotacaoId, inicio, fim);
                List<EscalaDoServidor> escalasDoPeriodo = ObtenhaEscalasDoServidorNoPeriodo(vinculoDeTrabalhoId, inicio, fim);
                List<Afastamento> afastamentosDoPeriodo = ObtenhaAfastamentosDoPeriodo(vinculoDeTrabalhoId, inicio, fim);
                List<EventoAnual> eventosDoPeriodo = EventosDaFolhaDePonto(organizacaoId, inicio, fim);
                Tuple<TimeSpan?, TimeSpan?> bancoDeHoras = ObtenhaCreditoDebitoDoPeriodoAnterior(vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras, vinculoDeTrabalhoId, inicio) ?? new(null, null);

                registrosDePonto = registrosDePonto
                    .OrderBy(r => r.DataHoraRegistro)
                    .GroupBy(r => r.DataHoraRegistro.Date)
                    .SelectMany(g =>
                        g.Aggregate(new List<RegistroDePonto>(), (acc, atual) =>
                        {
                            if (!acc.Any() || (atual.DataHoraRegistro - acc.Last().DataHoraRegistro).TotalMinutes > 5)
                                acc.Add(atual);
                            return acc;
                        })
                    )
                .ToList();

                var dataAuxiliar = inicio;
                while (dataAuxiliar <= fim)
                {
                    var eventoNoDia = eventosDoPeriodo
                        .FirstOrDefault(c => c.Data.Date == dataAuxiliar.Date);
                    var afastamento = afastamentosDoPeriodo
                        .FirstOrDefault(d => d.Inicio.Date <= dataAuxiliar.Date && (!d.Fim.HasValue || d.Fim.Value.Date >= dataAuxiliar.Date));
                    var horarioDoDia = vinculoDeTrabalho.HorarioDeTrabalho.Dias.FirstOrDefault(c => c.DiaDaSemana == (eDiaDaSemana)dataAuxiliar.DayOfWeek);

                    var escalaNoDia = escalasDoPeriodo
                        .FirstOrDefault(c =>
                            (c.Escala.Tipo == eTipoDeEscala.Mensal && c.Data.Date == dataAuxiliar.Date) ||
                            (c.Escala.Tipo == eTipoDeEscala.Ciclica && c.Data <= dataAuxiliar && (!c.DataFim.HasValue || c.DataFim >= dataAuxiliar)));

                    if (escalaNoDia != null)
                    {
                        if (escalaNoDia.Escala.Tipo == eTipoDeEscala.Mensal)
                        {
                            horarioDoDia = new()
                            {
                                DiaDaSemana = (eDiaDaSemana)dataAuxiliar.DayOfWeek,
                                Entrada1 = escalaNoDia.CicloDaEscala.Entrada1,
                                Entrada2 = escalaNoDia.CicloDaEscala.Entrada2,
                                Entrada3 = escalaNoDia.CicloDaEscala.Entrada3,
                                Entrada4 = escalaNoDia.CicloDaEscala.Entrada4,
                                Entrada5 = escalaNoDia.CicloDaEscala.Entrada5,
                                Saida1 = escalaNoDia.CicloDaEscala.Saida1,
                                Saida2 = escalaNoDia.CicloDaEscala.Saida2,
                                Saida3 = escalaNoDia.CicloDaEscala.Saida3,
                                Saida4 = escalaNoDia.CicloDaEscala.Saida4,
                                Saida5 = escalaNoDia.CicloDaEscala.Saida5
                            };
                        }
                        else
                        {
                            var cicloDoDia = escalaNoDia.ObterCicloAtual(dataAuxiliar);
                            horarioDoDia = new()
                            {
                                DiaDaSemana = (eDiaDaSemana)dataAuxiliar.DayOfWeek,
                                Entrada1 = cicloDoDia.Entrada1,
                                Entrada2 = cicloDoDia.Entrada2,
                                Entrada3 = cicloDoDia.Entrada3,
                                Entrada4 = cicloDoDia.Entrada4,
                                Entrada5 = cicloDoDia.Entrada5,
                                Saida1 = cicloDoDia.Saida1,
                                Saida2 = cicloDoDia.Saida2,
                                Saida3 = cicloDoDia.Saida3,
                                Saida4 = cicloDoDia.Saida4,
                                Saida5 = cicloDoDia.Saida5
                            };
                        }
                    }

                    PontoDoDia pontoDoDia = pontosDoPeriodo
                        .FirstOrDefault(c => c.Data.Date == dataAuxiliar.Date);

                    if (pontoDoDia == null)
                    {
                        pontoDoDia = new()
                        { Data = dataAuxiliar, VinculoDeTrabalhoId = vinculoDeTrabalhoId, OrganizacaoId = organizacaoId };
                        pontosDoPeriodo.Add(pontoDoDia);
                    }

                    var registrosNoDia = registrosDePonto
                        .Where(r => r.DataHoraRegistro.Date == dataAuxiliar.Date)
                        .OrderBy(r => r.DataHoraRegistro)
                    .ToList();

                    TimeSpan TruncarSegundos(TimeSpan horario) => new(horario.Hours, horario.Minutes, 0);

                    var registrosJaUtilizados = new List<TimeSpan>();
                    if (pontoDoDia.Entrada1.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Entrada1.Value));
                    if (pontoDoDia.Entrada2.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Entrada2.Value));
                    if (pontoDoDia.Entrada3.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Entrada3.Value));
                    if (pontoDoDia.Entrada4.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Entrada4.Value));
                    if (pontoDoDia.Entrada5.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Entrada5.Value));
                    if (pontoDoDia.Saida1.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Saida1.Value));
                    if (pontoDoDia.Saida2.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Saida2.Value));
                    if (pontoDoDia.Saida3.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Saida3.Value));
                    if (pontoDoDia.Saida4.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Saida4.Value));
                    if (pontoDoDia.Saida5.HasValue)
                        registrosJaUtilizados.Add(TruncarSegundos(pontoDoDia.Saida5.Value));

                    if (registrosJaUtilizados.Any())
                        registrosNoDia.RemoveAll(c => registrosJaUtilizados.Contains(TruncarSegundos(c.DataHoraRegistro.TimeOfDay)));

                    pontoDoDia.VinculoDeTrabalhoId = vinculoDeTrabalhoId;
                    pontoDoDia.VinculoDeTrabalho = vinculoDeTrabalho;

                    if (pontoDoDia.JustificativaPeriodo1Id.HasValue)
                        pontoDoDia.JustificativaPeriodo1 = _repositorioJustificativa.Obtenha(pontoDoDia.JustificativaPeriodo1Id.Value);

                    if (pontoDoDia.JustificativaPeriodo2Id.HasValue)
                        pontoDoDia.JustificativaPeriodo2 = _repositorioJustificativa.Obtenha(pontoDoDia.JustificativaPeriodo2Id.Value);

                    if (pontoDoDia.JustificativaPeriodo3Id.HasValue)
                        pontoDoDia.JustificativaPeriodo3 = _repositorioJustificativa.Obtenha(pontoDoDia.JustificativaPeriodo3Id.Value);

                    if (pontoDoDia.JustificativaPeriodo4Id.HasValue)
                        pontoDoDia.JustificativaPeriodo4 = _repositorioJustificativa.Obtenha(pontoDoDia.JustificativaPeriodo4Id.Value);

                    if (pontoDoDia.JustificativaPeriodo5Id.HasValue)
                        pontoDoDia.JustificativaPeriodo5 = _repositorioJustificativa.Obtenha(pontoDoDia.JustificativaPeriodo5Id.Value);

                    if (!pontoDoDia.PontoFechado && !pontoDoDia.DataFutura)
                    {
                        if (afastamento != null)
                            pontoDoDia.AfastamentoId = afastamento.Id;
                        else
                        {
                            pontoDoDia.AfastamentoId = null;

                            int indexRegistro = 0;
                            while (registrosNoDia.ElementAtOrDefault(indexRegistro) != null)
                            {
                                var registroNaPosicao = registrosNoDia.ElementAt(indexRegistro);

                                indexRegistro++;

                                if (!pontoDoDia.Entrada1.HasValue && !pontoDoDia.JustificativaPeriodo1Id.HasValue)
                                {
                                    pontoDoDia.Entrada1 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada1Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Saida1.HasValue && !pontoDoDia.JustificativaPeriodo1Id.HasValue)
                                {
                                    pontoDoDia.Saida1 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida1Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Entrada2.HasValue && !pontoDoDia.JustificativaPeriodo2Id.HasValue)
                                {
                                    pontoDoDia.Entrada2 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada2Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Saida2.HasValue && !pontoDoDia.JustificativaPeriodo2Id.HasValue)
                                {
                                    pontoDoDia.Saida2 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida2Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Entrada3.HasValue && !pontoDoDia.JustificativaPeriodo3Id.HasValue)
                                {
                                    pontoDoDia.Entrada3 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada3Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Saida3.HasValue && !pontoDoDia.JustificativaPeriodo3Id.HasValue)
                                {
                                    pontoDoDia.Saida3 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida3Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Entrada4.HasValue && !pontoDoDia.JustificativaPeriodo4Id.HasValue)
                                {
                                    pontoDoDia.Entrada4 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada4Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Saida4.HasValue && !pontoDoDia.JustificativaPeriodo4Id.HasValue)
                                {
                                    pontoDoDia.Saida4 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida4Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Entrada5.HasValue && !pontoDoDia.JustificativaPeriodo5Id.HasValue)
                                {
                                    pontoDoDia.Entrada5 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada5Id = registroNaPosicao.Id;
                                    continue;
                                }

                                if (!pontoDoDia.Saida5.HasValue && !pontoDoDia.JustificativaPeriodo5Id.HasValue)
                                {
                                    pontoDoDia.Saida5 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida5Id = registroNaPosicao.Id;
                                    continue;
                                }
                            }
                        }

                        CalculeCargaHorariaDoDia(ref pontoDoDia, eventoNoDia, horarioDoDia, afastamento);
                        CalculeHorasTrabalhadas(ref pontoDoDia);
                        CalculeHorasTrabalhadasConsiderandoAbono(ref pontoDoDia, eventoNoDia, horarioDoDia, afastamento);
                        CalculeHorasPositivas(ref pontoDoDia, afastamento);
                        CalculeHorasNegativas(ref pontoDoDia, afastamento);
                        CalculeBancoDeHoras(ref pontoDoDia, vinculoDeTrabalho, bancoDeHoras);
                    }

                    bancoDeHoras = new Tuple<TimeSpan?, TimeSpan?>(pontoDoDia.BancoDeHorasCredito, pontoDoDia.BancoDeHorasDebito);
                    dataAuxiliar = dataAuxiliar.AddDays(1);
                }

                pontosDoPeriodo.ForEach(c =>
                {
                    if (c.Id == 0)
                        _repositorio.Add(c);
                    else
                        _repositorio.Atualizar(c);
                });

                _repositorio.Commit();

                return pontosDoPeriodo
                    .DistinctBy(c => c.Data)
                    .OrderBy(c => c.Data)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ResetarFolhaDePonto(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            int unidadeLotacaoId,
            MesAno mesAno)
        {
            try
            {
                ObtenhaRegistrosDePonto(
                    organizacaoId,
                    vinculoDeTrabalhoId,
                    unidadeLotacaoId,
                    mesAno,
                    out var vinculoDeTrabalho,
                    out var inicio,
                    out var fim,
                    out var pontosDoPeriodo);

                foreach (var dia in pontosDoPeriodo)
                {
                    var registroPersistido = _repositorio.Obtenha(dia.Id);

                    if (registroPersistido.RegistroDePontoEntrada1Id.HasValue && 
                        registroPersistido.RegistroDePontoEntrada1.RegistroAplicativoId.HasValue && 
                        registroPersistido.RegistroDePontoEntrada1.RegistroAplicativo.Manual && 
                        registroPersistido.RegistroDePontoEntrada1.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada1.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada1.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada1);
                        registroPersistido.RegistroDePontoEntrada1Id = null;
                    }

                    if (registroPersistido.RegistroDePontoEntrada2Id.HasValue &&
                        registroPersistido.RegistroDePontoEntrada2.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoEntrada2.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoEntrada2.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada2.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada2.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada2);
                        registroPersistido.RegistroDePontoEntrada2Id = null;
                    }

                    if (registroPersistido.RegistroDePontoEntrada3Id.HasValue &&
                        registroPersistido.RegistroDePontoEntrada3.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoEntrada3.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoEntrada3.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada3.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada3.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada3);
                        registroPersistido.RegistroDePontoEntrada3Id = null;
                    }

                    if (registroPersistido.RegistroDePontoEntrada4Id.HasValue &&
                        registroPersistido.RegistroDePontoEntrada4.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoEntrada4.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoEntrada4.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada4.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada4.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada4);
                        registroPersistido.RegistroDePontoEntrada4Id = null;
                    }

                    if (registroPersistido.RegistroDePontoEntrada5Id.HasValue &&
                        registroPersistido.RegistroDePontoEntrada5.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoEntrada5.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoEntrada5.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada5.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada5.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada5);
                        registroPersistido.RegistroDePontoEntrada5Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida1Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida1.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida1.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoSaida1.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida1.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida1.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida1);
                        registroPersistido.RegistroDePontoSaida1Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida2Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida2.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida2.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoSaida2.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida2.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida2.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida2);
                        registroPersistido.RegistroDePontoSaida2Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida3Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida3.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida3.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoSaida3.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida3.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida3.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida3);
                        registroPersistido.RegistroDePontoSaida3Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida4Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida4.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida4.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoSaida4.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida4.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida4.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida4);
                        registroPersistido.RegistroDePontoSaida4Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida5Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida5.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida5.RegistroAplicativo.Manual &&
                        registroPersistido.RegistroDePontoSaida5.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida5.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida5.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida5);
                        registroPersistido.RegistroDePontoSaida5Id = null;
                    }

                    _repositorio.Remover(registroPersistido);
                }

                _repositorio.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ObtenhaRegistrosDePonto(int organizacaoId, int vinculoDeTrabalhoId, int unidadeLotacaoId, MesAno mesAno, out VinculoDeTrabalho vinculoDeTrabalho, out DateTime inicio, out DateTime fim, out List<PontoDoDia> pontosDoPeriodo)
        {
            vinculoDeTrabalho = _repositorioVinculo.Obtenha(vinculoDeTrabalhoId);
            if (vinculoDeTrabalho.Fim.HasValue && vinculoDeTrabalho.Fim < mesAno.Inicio)
                throw new ApplicationException("A data final do vínculo de trabalho é menor que o início do período selecionado.");

            if (!vinculoDeTrabalho.Lotacoes.Any(d => d.UnidadeOrganizacionalId == unidadeLotacaoId && (!d.Saida.HasValue || d.Saida > mesAno.Inicio)))
                throw new ApplicationException("A data final da lotação é menor que o início do período selecionado.");

            inicio = mesAno.Inicio;
            fim = mesAno.Fim;
            if (vinculoDeTrabalho.Inicio > inicio)
                inicio = vinculoDeTrabalho.Inicio;

            if (vinculoDeTrabalho.Fim.HasValue && vinculoDeTrabalho.Fim < fim)
                fim = vinculoDeTrabalho.Fim.Value;

            var ultimaLotacao = vinculoDeTrabalho
                .Lotacoes
                .OrderBy(c => c.Entrada)
                .Last(c => c.UnidadeOrganizacionalId == unidadeLotacaoId);

            if (ultimaLotacao.Entrada > inicio)
                inicio = ultimaLotacao.Entrada;

            if (ultimaLotacao.Saida.HasValue && ultimaLotacao.Saida < fim)
                fim = ultimaLotacao.Saida.Value;

            var query = @"select	
	                            *
                            from pontododia
                            where
	                            OrganizacaoId = @ORGANIZACAOID
	                            and VinculoDeTrabalhoId = @VINCULOID
	                            and date(Data) >= date(@INICIO)
                                and date(Data) <= date(@FIM)
                            order by Data";

            pontosDoPeriodo = _repositorio.ConsultaDapper<PontoDoDia>(query, new
            {
                @ORGANIZACAOID = organizacaoId,
                @VINCULOID = vinculoDeTrabalhoId,
                @INICIO = inicio,
                @FIM = fim
            });

            Func<int, RegistroDePonto> CarregueRegistroDePonto = new Func<int, RegistroDePonto>((registroId) 
                => _repositorio.ConsultaDapper<RegistroDePonto>("select * from registrodeponto where Id = @REGISTROID", new { @REGISTROID = registroId }).FirstOrDefault());

            foreach (var pontoDia in pontosDoPeriodo)
            {
                if (pontoDia.RegistroDePontoEntrada1Id.HasValue)
                    pontoDia.RegistroDePontoEntrada1 = CarregueRegistroDePonto(pontoDia.RegistroDePontoEntrada1Id.Value);
                if (pontoDia.RegistroDePontoEntrada2Id.HasValue)
                    pontoDia.RegistroDePontoEntrada2 = CarregueRegistroDePonto(pontoDia.RegistroDePontoEntrada2Id.Value);
                if (pontoDia.RegistroDePontoEntrada3Id.HasValue)
                    pontoDia.RegistroDePontoEntrada3 = CarregueRegistroDePonto(pontoDia.RegistroDePontoEntrada3Id.Value);
                if (pontoDia.RegistroDePontoEntrada4Id.HasValue)
                    pontoDia.RegistroDePontoEntrada4 = CarregueRegistroDePonto(pontoDia.RegistroDePontoEntrada4Id.Value);
                if (pontoDia.RegistroDePontoEntrada5Id.HasValue)
                    pontoDia.RegistroDePontoEntrada5 = CarregueRegistroDePonto(pontoDia.RegistroDePontoEntrada5Id.Value);

                if (pontoDia.RegistroDePontoSaida1Id.HasValue)
                    pontoDia.RegistroDePontoSaida1 = CarregueRegistroDePonto(pontoDia.RegistroDePontoSaida1Id.Value);
                if (pontoDia.RegistroDePontoSaida2Id.HasValue)
                    pontoDia.RegistroDePontoSaida2 = CarregueRegistroDePonto(pontoDia.RegistroDePontoSaida2Id.Value);
                if (pontoDia.RegistroDePontoSaida3Id.HasValue)
                    pontoDia.RegistroDePontoSaida3 = CarregueRegistroDePonto(pontoDia.RegistroDePontoSaida3Id.Value);
                if (pontoDia.RegistroDePontoSaida4Id.HasValue)
                    pontoDia.RegistroDePontoSaida4 = CarregueRegistroDePonto(pontoDia.RegistroDePontoSaida4Id.Value);
                if (pontoDia.RegistroDePontoSaida5Id.HasValue)
                    pontoDia.RegistroDePontoSaida5 = CarregueRegistroDePonto(pontoDia.RegistroDePontoSaida5Id.Value);
            }
        }

        public List<EventoAnual> EventosDaFolhaDePonto(
            int organizacaoId,
            DateTime inicio,
            DateTime fim)
        {
            try
            {
                var query = @"select
	                            *
                            from eventoanual
                            where
	                            OrganizacaoId = @ORGANIZACAOID
                                and date(Data) between date(@INICIO) and date(@FIM)";

                return _repositorio.ConsultaDapper<EventoAnual>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @INICIO = inicio,
                    @FIM = fim
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void FecharOuAbrirFolhaDePonto(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            MesAno mesAno,
            int unidadeLotacaoId,
            bool fechar)
        {
            try
            {
                var vinculoDeTrabalho = _repositorioVinculo.Obtenha(vinculoDeTrabalhoId);

                if (vinculoDeTrabalho.Fim.HasValue && vinculoDeTrabalho.Fim < mesAno.Inicio)
                    throw new ApplicationException("A data final do vínculo de trabalho é menor que o início do período selecionado.");

                if (!vinculoDeTrabalho.Lotacoes.Any(d => d.UnidadeOrganizacionalId == unidadeLotacaoId && (!d.Saida.HasValue || d.Saida > mesAno.Inicio)))
                    throw new ApplicationException("A data final da lotação é menor que o início do período selecionado.");

                DateTime inicio = mesAno.Inicio;
                DateTime fim = mesAno.Fim;

                if (vinculoDeTrabalho.Inicio > inicio)
                    inicio = vinculoDeTrabalho.Inicio;

                if (vinculoDeTrabalho.Fim.HasValue && vinculoDeTrabalho.Fim < fim)
                    fim = vinculoDeTrabalho.Fim.Value;

                var ultimaLotacao = vinculoDeTrabalho
                    .Lotacoes
                    .OrderBy(c => c.Entrada)
                    .Last(c => c.UnidadeOrganizacionalId == unidadeLotacaoId);

                if (ultimaLotacao.Entrada > inicio)
                    inicio = ultimaLotacao.Entrada;

                if (ultimaLotacao.Saida.HasValue && ultimaLotacao.Saida < fim)
                    fim = ultimaLotacao.Saida.Value;

                var query = @"select	
	                            *
                            from pontododia
                            where
	                            OrganizacaoId = @ORGANIZACAOID
	                            and VinculoDeTrabalhoId = @VINCULOID
	                            and date(Data) >= date(@INICIO)
                                and date(Data) <= date(@FIM)
                            order by Data";

                var pontosDoPeriodo = _repositorio.ConsultaDapper<PontoDoDia>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @VINCULOID = vinculoDeTrabalhoId,
                    @INICIO = inicio,
                    @FIM = fim
                });

                foreach (var item in pontosDoPeriodo)
                {
                    item.PontoFechado = fechar;
                    _repositorio.Atualizar(item);
                }

                _repositorio.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<CodigoDescricaoDTO> ObtenhaListaDeUnidadesLotadasNoDepartamento(
            int organizacaoId,
            int departamentoId)
        {
            try
            {
                var query =
                    @"select
	                    distinct u.Id as Codigo,
                        u.Nome as Descricao
                    from vinculodetrabalho v
                    inner join lotacaounidadeorganizacional l
	                    on l.VinculoDeTrabalhoId = v.Id
                    inner join unidadeorganizacional u
	                    on u.Id = l.UnidadeOrganizacionalId
                    where
	                    v.OrganizacaoId = @ORGANIZACAOID
                        and v.DepartamentoId = @DEPARTAMENTOID
                    order by 2";

                return _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, new
                {
                    @ORGANIZACAOID = organizacaoId,
                    @DEPARTAMENTOID = departamentoId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RegistroAplicativo> ObtenhaRegistrosDeAplicativo(int vinculoId, MesAno periodo)
        {
            try
            {
                return _repositorioRegistroAplicativo.ObtenhaLista(c =>
                    c.VinculoDeTrabalhoId == vinculoId &&
                    c.Situacao == eSituacaoRegistroAplicativo.AguardandoAvaliacao &&
                    ((c.JustificativaDeAusenciaId.HasValue && (c.DataFinalAtestado >= periodo.Inicio && c.DataInicialAtestado <= periodo.Fim)) ||
                    (!c.JustificativaDeAusenciaId.HasValue && periodo.Inicio <= c.DataHora && periodo.Fim >= c.DataHora)));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AprovarRegistroAplicativo(int registroId, int unidadeLotacaoId, MesAno mesAno)
        {
            try
            {
                var registroAplicativo = _repositorioRegistroAplicativo.Obtenha(registroId);
                if (registroAplicativo.Situacao != eSituacaoRegistroAplicativo.AguardandoAvaliacao)
                    throw new ApplicationException("Esse registro não pode ter sua situação alterada.");

                registroAplicativo.Situacao = eSituacaoRegistroAplicativo.Aprovado;
                _repositorioRegistroAplicativo.Atualizar(registroAplicativo);

                if (!registroAplicativo.JustificativaDeAusenciaId.HasValue)
                {
                    _repositorioRegistroDePonto.Add(new RegistroDePonto
                    {
                        OrganizacaoId = registroAplicativo.OrganizacaoId,
                        DataHoraRegistro = registroAplicativo.DataHora,
                        DataHoraRecebimento = DateTime.Now,
                        RegistroAplicativoId = registroAplicativo.Id,
                        TipoRegistro = eTipoDeRegistroEquipamento.Aplicativo
                    });
                }
                else if (registroAplicativo.DataInicialAtestado.HasValue && registroAplicativo.DataFinalAtestado.HasValue)
                {
                    CarregueFolhaDePonto(registroAplicativo.OrganizacaoId, registroAplicativo.VinculoDeTrabalhoId, unidadeLotacaoId, mesAno);

                    var pontosDoPeriodo = _repositorio.ObtenhaLista(c => 
                        c.VinculoDeTrabalhoId == registroAplicativo.VinculoDeTrabalhoId && c.Data >= registroAplicativo.DataInicialAtestado 
                        && c.Data <= registroAplicativo.DataFinalAtestado);

                    var utiliza5Registros = registroAplicativo.VinculoDeTrabalho.HorarioDeTrabalho.UtilizaCincoPeriodos;
                    foreach (var pontoDia in pontosDoPeriodo)
                    {
                        pontoDia.JustificativaPeriodo1Id = registroAplicativo.JustificativaDeAusenciaId;
                        pontoDia.JustificativaPeriodo2Id = registroAplicativo.JustificativaDeAusenciaId;
                        pontoDia.JustificativaPeriodo3Id = registroAplicativo.JustificativaDeAusenciaId;

                        if (utiliza5Registros)
                        {
                            pontoDia.JustificativaPeriodo4Id = registroAplicativo.JustificativaDeAusenciaId;
                            pontoDia.JustificativaPeriodo5Id = registroAplicativo.JustificativaDeAusenciaId;
                        }

                        _repositorio.Atualizar(pontoDia);
                    }
                }

                _repositorioRegistroAplicativo.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ReprovarRegistroAplicativo(int registroId)
        {
            try
            {
                var registroAplicativo = _repositorioRegistroAplicativo.Obtenha(registroId);
                if (registroAplicativo.Situacao != eSituacaoRegistroAplicativo.AguardandoAvaliacao)
                    throw new ApplicationException("Esse registro não pode ter sua situação alterada.");

                registroAplicativo.Situacao = eSituacaoRegistroAplicativo.Reprovado;
                _repositorioRegistroAplicativo.Atualizar(registroAplicativo);
                _repositorioRegistroAplicativo.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CalculeCargaHorariaDoDia(
            ref PontoDoDia pontoDoDia,
            EventoAnual eventoNoDia,
            HorarioDeTrabalhoDia horarioDia,
            Afastamento afastamento)
        {
            pontoDoDia.CargaHoraria = null;

            if (horarioDia == null || (afastamento != null && afastamento.JustificativaDeAusencia.Abono))
                return;

            pontoDoDia.CargaHoraria = horarioDia.CalculeCargaHorariaTotal(eventoNoDia != null);
        }

        private void CalculeHorasTrabalhadas(ref PontoDoDia pontoDoDia)
       {
            TimeSpan? htPeriodo_1 = pontoDoDia.Entrada1.HasValue && pontoDoDia.Saida1.HasValue ?
                pontoDoDia.Saida1.Value.Subtract(pontoDoDia.Entrada1.Value) :
                null;

            TimeSpan? htPeriodo_2 = pontoDoDia.Entrada2.HasValue && pontoDoDia.Saida2.HasValue ?
                pontoDoDia.Saida2.Value.Subtract(pontoDoDia.Entrada2.Value) :
                null;

            TimeSpan? htPeriodo_3 = pontoDoDia.Entrada3.HasValue && pontoDoDia.Saida3.HasValue ?
                pontoDoDia.Saida3.Value.Subtract(pontoDoDia.Entrada3.Value) :
                null;

            TimeSpan? htPeriodo_4 = pontoDoDia.Entrada4.HasValue && pontoDoDia.Saida4.HasValue ?
                pontoDoDia.Saida4.Value.Subtract(pontoDoDia.Entrada4.Value) :
                null;

            TimeSpan? htPeriodo_5 = pontoDoDia.Entrada5.HasValue && pontoDoDia.Saida5.HasValue ?
                pontoDoDia.Saida5.Value.Subtract(pontoDoDia.Entrada5.Value) :
                null;

            TimeSpan? horasTrabalhadas = null;

            if (htPeriodo_1.HasValue || 
                htPeriodo_2.HasValue || 
                htPeriodo_3.HasValue || 
                htPeriodo_4.HasValue || 
                htPeriodo_5.HasValue)
            {
                horasTrabalhadas = TimeSpan.Zero;

                if (htPeriodo_1.HasValue)
                    horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_1.Value);

                if (htPeriodo_2.HasValue)
                    horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_2.Value);

                if (htPeriodo_3.HasValue)
                    horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_3.Value);

                if (htPeriodo_4.HasValue)
                    horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_4.Value);

                if (htPeriodo_5.HasValue)
                    horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_5.Value);
            }

            pontoDoDia.HorasTrabalhadas = horasTrabalhadas;
        }

        private void CalculeHorasTrabalhadasConsiderandoAbono(
            ref PontoDoDia pontoDoDia,
            EventoAnual eventoNoDia,
            HorarioDeTrabalhoDia horarioDia,
            Afastamento afastamento)
        {
            TimeSpan? htPeriodo_1 = null;
            TimeSpan? htPeriodo_2 = null;
            TimeSpan? htPeriodo_3 = null;
            TimeSpan? htPeriodo_4 = null;
            TimeSpan? htPeriodo_5 = null;

            if (afastamento != null)
            {
                if (afastamento.JustificativaDeAusencia.Abono)
                {
                    htPeriodo_1 = horarioDia.CargaHorariaPeriodo(1);
                    htPeriodo_2 = horarioDia.CargaHorariaPeriodo(2);
                    htPeriodo_3 = horarioDia.CargaHorariaPeriodo(3);
                    htPeriodo_4 = horarioDia.CargaHorariaPeriodo(4);
                    htPeriodo_5 = horarioDia.CargaHorariaPeriodo(5);
                }
            }
            else
            {
                htPeriodo_1 = pontoDoDia.Entrada1.HasValue && pontoDoDia.Saida1.HasValue ?
                pontoDoDia.Saida1.Value.Subtract(pontoDoDia.Entrada1.Value) :
                pontoDoDia.JustificativaPeriodo1Id.HasValue && pontoDoDia.JustificativaPeriodo1.Abono ?
                    horarioDia.CargaHorariaPeriodo(1) :
                    null;

                htPeriodo_2 = pontoDoDia.Entrada2.HasValue && pontoDoDia.Saida2.HasValue ?
                    pontoDoDia.Saida2.Value.Subtract(pontoDoDia.Entrada2.Value) :
                    pontoDoDia.JustificativaPeriodo2Id.HasValue && pontoDoDia.JustificativaPeriodo2.Abono ?
                        horarioDia.CargaHorariaPeriodo(2) :
                        null;

                htPeriodo_3 = pontoDoDia.Entrada3.HasValue && pontoDoDia.Saida3.HasValue ?
                    pontoDoDia.Saida3.Value.Subtract(pontoDoDia.Entrada3.Value) :
                    pontoDoDia.JustificativaPeriodo3Id.HasValue && pontoDoDia.JustificativaPeriodo3.Abono ?
                        horarioDia.CargaHorariaPeriodo(3) :
                        null;

                htPeriodo_4 = pontoDoDia.Entrada4.HasValue && pontoDoDia.Saida4.HasValue ?
                    pontoDoDia.Saida4.Value.Subtract(pontoDoDia.Entrada4.Value) :
                    pontoDoDia.JustificativaPeriodo4Id.HasValue && pontoDoDia.JustificativaPeriodo4.Abono ?
                        horarioDia.CargaHorariaPeriodo(4) :
                        null;

                htPeriodo_5 = pontoDoDia.Entrada5.HasValue && pontoDoDia.Saida5.HasValue ?
                    pontoDoDia.Saida5.Value.Subtract(pontoDoDia.Entrada5.Value) :
                    pontoDoDia.JustificativaPeriodo5Id.HasValue && pontoDoDia.JustificativaPeriodo5.Abono ?
                        horarioDia.CargaHorariaPeriodo(5) :
                        null;
            }

            TimeSpan? horasTrabalhadass = null;

            if (htPeriodo_1.HasValue || htPeriodo_2.HasValue || htPeriodo_3.HasValue || htPeriodo_4.HasValue || htPeriodo_5.HasValue)
            {
                horasTrabalhadass = TimeSpan.Zero;

                if (htPeriodo_1.HasValue)
                    horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_1.Value);

                if (htPeriodo_2.HasValue)
                    horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_2.Value);

                if (htPeriodo_3.HasValue)
                    horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_3.Value);

                if (htPeriodo_4.HasValue)
                    horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_4.Value);

                if (htPeriodo_5.HasValue)
                    horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_5.Value);
            }

            pontoDoDia.HorasTrabalhadasConsiderandoAbono = horasTrabalhadass;
        }

        private void CalculeHorasPositivas(
            ref PontoDoDia pontoDoDia,
            Afastamento afastamento)
        {
            pontoDoDia.HorasPositivas = null;

            if (afastamento != null)
                return;

            var cargaHorariaDoDia = pontoDoDia.CargaHoraria;

            if (!cargaHorariaDoDia.HasValue && !pontoDoDia.HorasTrabalhadasConsiderandoAbono.HasValue)
                return;

            if (!cargaHorariaDoDia.HasValue && pontoDoDia.HorasTrabalhadasConsiderandoAbono.HasValue)
                pontoDoDia.HorasPositivas = pontoDoDia.HorasTrabalhadasConsiderandoAbono;

            if (pontoDoDia.HorasTrabalhadasConsiderandoAbono > cargaHorariaDoDia)
                pontoDoDia.HorasPositivas = pontoDoDia.HorasTrabalhadasConsiderandoAbono.Value.Subtract(cargaHorariaDoDia.Value);
        }

        private void CalculeHorasNegativas(
            ref PontoDoDia pontoDoDia,
            Afastamento afastamento)
        {
            pontoDoDia.HorasNegativas = null;

            if (!pontoDoDia.CargaHoraria.HasValue || 
                (afastamento != null && afastamento.JustificativaDeAusencia.Abono))
                return;

            var cargaHorariaDoDia = pontoDoDia.CargaHoraria;

            if (cargaHorariaDoDia > ((pontoDoDia.HorasTrabalhadasConsiderandoAbono ?? TimeSpan.Zero) + (pontoDoDia.Abono ?? TimeSpan.Zero)))
                pontoDoDia.HorasNegativas = cargaHorariaDoDia.Value.Subtract((pontoDoDia.HorasTrabalhadasConsiderandoAbono ?? TimeSpan.Zero) + (pontoDoDia.Abono ?? TimeSpan.Zero));
        }

        private Tuple<TimeSpan?, TimeSpan?> ObtenhaCreditoDebitoDoPeriodoAnterior(
            bool bancoDeHorasHabilitado,
            int vinculoDeTrabalhoId, 
            DateTime inicioPeriodo)
        {
            if (!bancoDeHorasHabilitado)
                return new Tuple<TimeSpan?, TimeSpan?>(null, null);

            var query = @"select
	                        BancoDeHorasCredito as 'Item1',
                            BancoDeHorasDebito as 'Item2'
                        from pontododia
                        where
	                        VinculoDeTrabalhoId = @VINCULODETRABALHOID
                            and Data < @INICIO
                        order by Data desc
                        limit 1";

            return _repositorio.ConsultaDapper<Tuple<TimeSpan?, TimeSpan?>>(query, new
            {
                @VINCULODETRABALHOID = vinculoDeTrabalhoId,
                @INICIO = inicioPeriodo
            }).FirstOrDefault();
        }

        private void CalculeBancoDeHoras(
            ref PontoDoDia pontoDoDia,
            VinculoDeTrabalho vinculoDeTrabalho,
            Tuple<TimeSpan?, TimeSpan?> bancoDeHorasDiaAnterior)
        {
            pontoDoDia.BancoDeHorasCredito = null;
            pontoDoDia.BancoDeHorasDebito = null;

            if (vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras && vinculoDeTrabalho.HorarioDeTrabalho.InicioBancoDeHoras <= pontoDoDia.Data)
            {
                pontoDoDia.BancoDeHorasCredito = (bancoDeHorasDiaAnterior.Item1 ?? TimeSpan.Zero) + (pontoDoDia.HorasPositivas ?? TimeSpan.Zero);
                pontoDoDia.BancoDeHorasDebito = (bancoDeHorasDiaAnterior.Item2 ?? TimeSpan.Zero) + (pontoDoDia.HorasNegativas ?? TimeSpan.Zero);

                if ((pontoDoDia.BancoDeHorasCredito ?? TimeSpan.Zero) > TimeSpan.Zero &&
                    (pontoDoDia.BancoDeHorasDebito ?? TimeSpan.Zero) > TimeSpan.Zero)
                {
                    if (pontoDoDia.BancoDeHorasCredito > pontoDoDia.BancoDeHorasDebito)
                    {
                        pontoDoDia.BancoDeHorasCredito = pontoDoDia.BancoDeHorasCredito.Value.Subtract(pontoDoDia.BancoDeHorasDebito.Value);
                        pontoDoDia.BancoDeHorasDebito = TimeSpan.Zero;
                    }
                    else if (pontoDoDia.BancoDeHorasCredito == pontoDoDia.BancoDeHorasDebito)
                    {
                        pontoDoDia.BancoDeHorasCredito = TimeSpan.Zero;
                        pontoDoDia.BancoDeHorasDebito = TimeSpan.Zero;
                    }
                    else if (pontoDoDia.BancoDeHorasCredito < pontoDoDia.BancoDeHorasDebito)
                    {
                        pontoDoDia.BancoDeHorasDebito = pontoDoDia.BancoDeHorasDebito.Value.Subtract(pontoDoDia.BancoDeHorasCredito.Value);
                        pontoDoDia.BancoDeHorasCredito = TimeSpan.Zero;
                    }
                }
            }
        }

        private List<Afastamento> ObtenhaAfastamentosDoPeriodo(
            int vinculoDeTrabalhoId,
            DateTime inicio,
            DateTime fim)
        {
            return _repositorioAfastamento
                .ObtenhaLista(c => 
                    c.VinculoDeTrabalhoId == vinculoDeTrabalhoId && 
                    ((c.Inicio >= inicio && c.Inicio <= fim) || (c.Fim >= inicio && c.Fim <= fim)));
        }

        public List<EscalaDoServidor> ObtenhaEscalasDoServidorNoPeriodo(
            int vinculoDeTrabalhoId,
            DateTime inicio,
            DateTime fim)
        {
            return _repositorioEscala
                .ObtenhaLista(c =>
                    c.VinculoDeTrabalhoId == vinculoDeTrabalhoId);
        }

        public List<RegistroDePonto> ObtenhaRegistrosDePontoDoPeriodo(
            int vinculoDeTrabalhoId,
            int unidadeId,
            DateTime inicio,
            DateTime fim)
        {
            var query =
                    @"(select
	                    r.*
                    from registrodeponto r
                    inner join equipamentodeponto e
	                    on e.Id = r.EquipamentoDePontoId
                    where
	                    date(r.DataHoraRegistro) between date(@INICIO) and date(@FIM)
                        and e.UnidadeOrganizacionalId = @UNIDADEID
                        and r.UsuarioEquipamentoId in 
	                    (SELECT DISTINCT
		                    MatriculaEquipamento
	                    FROM
		                    lotacaounidadeorganizacional l
	                    WHERE
		                    l.VinculoDeTrabalhoId = @VINCULOID
                            and r.RegistroAplicativoId is null
		                    and l.UnidadeOrganizacionalId = @UNIDADEID))
                    union
                    (select
	                    r.*
                    from registrodeponto r
                    inner join registroaplicativo a
	                    on a.Id = r.RegistroAplicativoId
                    where
	                    a.VinculoDeTrabalhoId = @VINCULOID
                        and date(a.DataHora) between date(@INICIO) and date(@FIM))";

            return _repositorio.ConsultaDapper<RegistroDePonto>(query, new
            {
                @UNIDADEID = unidadeId,
                @VINCULOID = vinculoDeTrabalhoId,
                @INICIO = inicio,
                @FIM = fim
            });
        }

        public void MovimentarRegistro(
            int id, 
            string classe,
            bool avancar)
        {
            try
            {
                var pontoDia = _repositorio.Obtenha(id);
                if (pontoDia == null)
                    throw new Exception("Ponto dia não encontrado.");

                if (string.IsNullOrEmpty(classe))
                    throw new Exception("Classe não informada.");

                var entrada1 = pontoDia.Entrada1;
                var registroAppEntrada1 = pontoDia.RegistroDePontoEntrada1Id;
                var saida1 = pontoDia.Saida1;
                var registroAppSaida1 = pontoDia.RegistroDePontoSaida1Id;

                var entrada2 = pontoDia.Entrada2;
                var registroAppEntrada2 = pontoDia.RegistroDePontoEntrada2Id;
                var saida2 = pontoDia.Saida2;
                var registroAppSaida2 = pontoDia.RegistroDePontoSaida2Id;

                var entrada3 = pontoDia.Entrada3;
                var registroAppEntrada3 = pontoDia.RegistroDePontoEntrada3Id;
                var saida3 = pontoDia.Saida3;
                var registroAppSaida3 = pontoDia.RegistroDePontoSaida3Id;

                var entrada4 = pontoDia.Entrada4;
                var registroAppEntrada4 = pontoDia.RegistroDePontoEntrada4Id;
                var saida4 = pontoDia.Saida4;
                var registroAppSaida4 = pontoDia.RegistroDePontoSaida4Id;

                var entrada5 = pontoDia.Entrada5;
                var registroAppEntrada5 = pontoDia.RegistroDePontoEntrada5Id;
                var saida5 = pontoDia.Saida5;
                var registroAppSaida5 = pontoDia.RegistroDePontoSaida5Id;

                switch (classe)
                {
                    case "entrada1":
                        pontoDia.Entrada1 = saida1;
                        pontoDia.Saida1 = entrada1;
                        pontoDia.RegistroDePontoEntrada1Id = registroAppSaida1;
                        pontoDia.RegistroDePontoSaida1Id = registroAppEntrada1;
                        break;

                    case "saida1":
                        if (avancar)
                        {
                            pontoDia.Entrada2 = saida1;
                            pontoDia.RegistroDePontoEntrada2Id = registroAppSaida1;
                            pontoDia.Saida1 = entrada2;
                            pontoDia.RegistroDePontoSaida1Id = registroAppEntrada2;
                        }
                        else
                        {
                            pontoDia.Entrada1 = saida1;
                            pontoDia.RegistroDePontoEntrada1Id = registroAppSaida1;
                            pontoDia.Saida1 = entrada1;
                            pontoDia.RegistroDePontoSaida1Id = registroAppEntrada1;
                        }
                        break;

                    case "entrada2":
                        if (avancar)
                        {
                            pontoDia.Saida2 = entrada2;
                            pontoDia.RegistroDePontoSaida2Id = registroAppEntrada2;
                            pontoDia.Entrada2 = saida2;
                            pontoDia.RegistroDePontoEntrada2Id = registroAppSaida2;
                        }
                        else
                        {
                            pontoDia.Saida1 = entrada2;
                            pontoDia.RegistroDePontoSaida1Id = registroAppEntrada2;
                            pontoDia.Entrada2 = saida1;
                            pontoDia.RegistroDePontoEntrada2Id = registroAppSaida1;
                        }
                        break;

                    case "saida2":
                        if (avancar)
                        {
                            pontoDia.Entrada3 = saida2;
                            pontoDia.RegistroDePontoEntrada3Id = registroAppSaida2;
                            pontoDia.Saida2 = entrada3;
                            pontoDia.RegistroDePontoSaida2Id = registroAppEntrada3;
                        }
                        else
                        {
                            pontoDia.Saida2 = entrada2;
                            pontoDia.RegistroDePontoSaida2Id = registroAppEntrada2;
                            pontoDia.Entrada2 = saida2;
                            pontoDia.RegistroDePontoEntrada2Id = registroAppSaida2;
                        }
                        break;

                    case "entrada3":
                        if (avancar)
                        {
                            pontoDia.Saida3 = entrada3;
                            pontoDia.RegistroDePontoSaida3Id = registroAppEntrada3;
                            pontoDia.Entrada3 = saida3;
                            pontoDia.RegistroDePontoEntrada3Id = registroAppSaida3;
                        }
                        else
                        {
                            pontoDia.Saida2 = entrada3;
                            pontoDia.RegistroDePontoSaida2Id = registroAppEntrada3;
                            pontoDia.Entrada3 = saida2;
                            pontoDia.RegistroDePontoEntrada3Id = registroAppSaida2;
                        }
                        break;

                    case "entrada4":
                        if (avancar)
                        {
                            pontoDia.Entrada4 = saida4;
                            pontoDia.RegistroDePontoEntrada4Id = registroAppSaida4;
                            pontoDia.Saida4 = entrada4;
                            pontoDia.RegistroDePontoSaida4Id = registroAppEntrada4;
                        }
                        else
                        {
                            pontoDia.Saida3 = entrada4;
                            pontoDia.RegistroDePontoSaida3Id = registroAppEntrada4;
                            pontoDia.Entrada4 = saida3;
                            pontoDia.RegistroDePontoEntrada4Id = registroAppSaida3;
                        }
                        break;

                    case "saida4":
                        if (avancar)
                        {
                            pontoDia.Entrada5 = saida4;
                            pontoDia.RegistroDePontoEntrada5Id = registroAppSaida4;
                            pontoDia.Saida4 = entrada5;
                            pontoDia.RegistroDePontoSaida4Id = registroAppEntrada5;
                        }
                        else
                        {
                            pontoDia.Entrada4 = saida4;
                            pontoDia.RegistroDePontoEntrada4Id = registroAppSaida4;
                            pontoDia.Saida4 = entrada4;
                            pontoDia.RegistroDePontoSaida4Id = registroAppEntrada4;
                        }
                        break;

                    case "entrada5":
                        if (avancar)
                        {
                            pontoDia.Saida5 = entrada5;
                            pontoDia.RegistroDePontoSaida5Id = registroAppEntrada5;
                            pontoDia.Entrada5 = saida5;
                            pontoDia.RegistroDePontoEntrada5Id = registroAppSaida5;
                        }
                        else
                        {
                            pontoDia.Saida4 = entrada5;
                            pontoDia.RegistroDePontoSaida4Id = registroAppEntrada5;
                            pontoDia.Entrada5 = saida4;
                            pontoDia.RegistroDePontoEntrada5Id = registroAppSaida4;
                        }
                        break;

                    case "saida5":
                        pontoDia.Entrada5 = saida5;
                        pontoDia.RegistroDePontoEntrada5Id = registroAppSaida5;
                        pontoDia.Saida5 = entrada5;
                        pontoDia.RegistroDePontoSaida5Id = registroAppEntrada5;
                        break;
                }


                _repositorio.Atualizar(pontoDia);
                _repositorio.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}