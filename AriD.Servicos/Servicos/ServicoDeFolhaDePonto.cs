using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeFolhaDePonto : Servico<PontoDoDia>, IServicoDeFolhaDePonto
    {
        private readonly IRepositorio<PontoDoDia> _repositorio;
        private readonly IRepositorio<Servidor> _repositorioServidor;
        private readonly IRepositorio<LotacaoUnidadeOrganizacional> _repositorioLotacao;
        private readonly IRepositorio<JustificativaDeAusencia> _repositorioJustificativa;
        private readonly IRepositorio<VinculoDeTrabalho> _repositorioVinculo;

        public ServicoDeFolhaDePonto(
            IRepositorio<PontoDoDia> repositorio,
            IRepositorio<Servidor> repositorioServidor,
            IRepositorio<LotacaoUnidadeOrganizacional> repositorioLotacao,
            IRepositorio<JustificativaDeAusencia> repositorioJustificativa,
            IRepositorio<VinculoDeTrabalho> repositorioVinculo)
            : base(repositorio)
        {
            _repositorio = repositorio;
            _repositorioServidor = repositorioServidor;
            _repositorioLotacao = repositorioLotacao;
            _repositorioJustificativa = repositorioJustificativa;
            _repositorioVinculo = repositorioVinculo;
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
            int unidadeId)
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
                                and s.OrganizacaoId = @ORGANIZACAOID
                            order by p.Nome";

                var servidores = _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, new
                {
                    @UNIDADEID = unidadeId,
                    @ORGANIZACAOID = organizacaoId
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
            int unidadeId)
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
                                and l.UnidadeOrganizacionalId = @UNIDADEID
                            order by v.Inicio, v.Matricula";

                var vinculos = _repositorio.ConsultaDapper<CodigoDescricaoDTO>(query, new
                {
                    @UNIDADEID = unidadeId,
                    @ORGANIZACAOID = organizacaoId,
                    @SERVIDORID = servidorId
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

                List<EscalaDoServidor> escalasDoPeriodo = new();
                List<Afastamento> afastamentosDoPeriodo = new();
                List<EventoAnual> eventosDoPeriodo = EventosDaFolhaDePonto(organizacaoId, inicio, fim);
                Tuple<TimeSpan?, TimeSpan?> bancoDeHoras = ObtenhaCreditoDebitoDoPeriodoAnterior(vinculoDeTrabalho.HorarioDeTrabalho.UtilizaBancoDeHoras, vinculoDeTrabalhoId, inicio) ?? new(null, null);

                var dataAuxiliar = inicio;
                while (dataAuxiliar <= fim)
                {
                    var eventoNoDia = eventosDoPeriodo
                        .FirstOrDefault(c => c.Data.Date == dataAuxiliar.Date);
                    var afastamento = afastamentosDoPeriodo
                        .FirstOrDefault(d => d.Inicio.Date <= dataAuxiliar.Date && (!d.Fim.HasValue || d.Fim.Value.Date >= dataAuxiliar.Date));
                    var horarioDoDia = vinculoDeTrabalho.HorarioDeTrabalho.Dias.FirstOrDefault(c => c.DiaDaSemana == (eDiaDaSemana)dataAuxiliar.DayOfWeek);

                    var escalaNoDia = escalasDoPeriodo.FirstOrDefault(c => c.Data.Date == dataAuxiliar.Date);
                    if (escalaNoDia != null)
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

                    PontoDoDia pontoDoDia = pontosDoPeriodo
                        .FirstOrDefault(c => c.Data.Date == dataAuxiliar.Date);

                    if (pontoDoDia == null)
                    {
                        pontoDoDia = new() 
                            { Data = dataAuxiliar, VinculoDeTrabalhoId = vinculoDeTrabalhoId, OrganizacaoId = organizacaoId };
                        pontosDoPeriodo.Add(pontoDoDia);
                    }

                    /* TODO: Carregar registros de frequência do servidor */
                    if (!pontoDoDia.PontoFechado && !pontoDoDia.DataFutura)
                    {
                        if (afastamento != null)
                            pontoDoDia.AfastamentoId = afastamento.Id;
                        else
                            pontoDoDia.AfastamentoId = null;

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
                    c.VinculoDeTrabalhoId = vinculoDeTrabalhoId;
                    c.VinculoDeTrabalho = vinculoDeTrabalho;

                    if (c.JustificativaPeriodo1Id.HasValue)
                        c.JustificativaPeriodo1 = _repositorioJustificativa.Obtenha(c.JustificativaPeriodo1Id.Value);

                    if (c.JustificativaPeriodo2Id.HasValue)
                        c.JustificativaPeriodo2 = _repositorioJustificativa.Obtenha(c.JustificativaPeriodo2Id.Value);

                    if (c.JustificativaPeriodo3Id.HasValue)
                        c.JustificativaPeriodo3 = _repositorioJustificativa.Obtenha(c.JustificativaPeriodo3Id.Value);

                    if (c.JustificativaPeriodo4Id.HasValue)
                        c.JustificativaPeriodo4 = _repositorioJustificativa.Obtenha(c.JustificativaPeriodo4Id.Value);

                    if (c.JustificativaPeriodo5Id.HasValue)
                        c.JustificativaPeriodo5 = _repositorioJustificativa.Obtenha(c.JustificativaPeriodo5Id.Value);

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
    }
}