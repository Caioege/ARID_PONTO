using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using Dapper;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeFolhaDePonto : Servico<PontoDoDia>, IServicoDeFolhaDePonto
    {
        private static string FmtHora(TimeSpan? t) => t.HasValue ? t.Value.ToString(@"hh\:mm") : "-";
        private static string FmtInt(int? v) => v.HasValue ? v.Value.ToString() : "-";

        private static string CampoAmigavel(string campo)
        {
            if (string.IsNullOrWhiteSpace(campo)) return "Campo";

            campo = campo.Trim().ToLowerInvariant();

            return campo switch
            {
                "entrada1" => "Entrada 1",
                "saida1" => "Saída 1",
                "entrada2" => "Entrada 2",
                "saida2" => "Saída 2",
                "entrada3" => "Entrada 3",
                "saida3" => "Saída 3",
                "entrada4" => "Entrada 4",
                "saida4" => "Saída 4",
                "entrada5" => "Entrada 5",
                "saida5" => "Saída 5",
                "abono" => "Abono",

                "justificativaperiodo1id" => "Justificativa Período 1",
                "justificativaperiodo2id" => "Justificativa Período 2",
                "justificativaperiodo3id" => "Justificativa Período 3",
                "justificativaperiodo4id" => "Justificativa Período 4",
                "justificativaperiodo5id" => "Justificativa Período 5",

                _ => campo
            };
        }

        private static string ValorDoCampo(PontoDoDia p, string acao)
        {
            var a = acao?.Trim()?.ToLowerInvariant();

            return a switch
            {
                "entrada1" => FmtHora(p.Entrada1),
                "saida1" => FmtHora(p.Saida1),
                "entrada2" => FmtHora(p.Entrada2),
                "saida2" => FmtHora(p.Saida2),
                "entrada3" => FmtHora(p.Entrada3),
                "saida3" => FmtHora(p.Saida3),
                "entrada4" => FmtHora(p.Entrada4),
                "saida4" => FmtHora(p.Saida4),
                "entrada5" => FmtHora(p.Entrada5),
                "saida5" => FmtHora(p.Saida5),
                "abono" => FmtHora(p.Abono),

                "justificativaperiodo1id" => FmtInt(p.JustificativaPeriodo1Id),
                "justificativaperiodo2id" => FmtInt(p.JustificativaPeriodo2Id),
                "justificativaperiodo3id" => FmtInt(p.JustificativaPeriodo3Id),
                "justificativaperiodo4id" => FmtInt(p.JustificativaPeriodo4Id),
                "justificativaperiodo5id" => FmtInt(p.JustificativaPeriodo5Id),

                _ => "-"
            };
        }

        private static string TipoDeMudanca(string antes, string depois)
        {
            if (antes == "-" && depois != "-") return "Inclusão manual";
            if (antes != "-" && depois == "-") return "Remoção manual";
            if (antes != depois) return "Edição manual";
            return "Atualização manual";
        }

        private static string ProximoCampo(string classe, bool avancar)
        {
            var c = (classe ?? "").Trim().ToLowerInvariant();
            string[] seq = { "entrada1", "saida1", "entrada2", "saida2", "entrada3", "saida3", "entrada4", "saida4", "entrada5", "saida5" };

            var idx = Array.IndexOf(seq, c);
            if (idx < 0) return c;

            if (avancar) return (idx + 1 < seq.Length) ? seq[idx + 1] : seq[idx];
            return (idx - 1 >= 0) ? seq[idx - 1] : seq[idx];
        }


        private readonly IUsuarioAtual _usuarioAtual;

        private readonly IRepositorio<PontoDoDia> _repositorio;
        private readonly IRepositorio<Servidor> _repositorioServidor;
        private readonly IRepositorio<LotacaoUnidadeOrganizacional> _repositorioLotacao;
        private readonly IRepositorio<JustificativaDeAusencia> _repositorioJustificativa;
        private readonly IRepositorio<VinculoDeTrabalho> _repositorioVinculo;
        private readonly IRepositorio<Afastamento> _repositorioAfastamento;
        private readonly IRepositorio<EscalaDoServidor> _repositorioEscala;
        private readonly IRepositorio<RegistroAplicativo> _repositorioRegistroAplicativo;
        private readonly IRepositorio<RegistroDePonto> _repositorioRegistroDePonto;
        private readonly IRepositorio<RegraHoraExtra> _repositorioRegraHoraExtra;
        private readonly IRepositorio<FaixaHoraExtra> _repositorioFaixaHoraExtra;
        private readonly IRepositorio<PontoDoDiaHoraExtra> _repositorioPontoDoDiaHoraExtra;
        private readonly IRepositorio<LogAuditoriaPonto> _repositorioAuditoriaPonto;
        private readonly IRepositorio<HorarioDeTrabalhoVigencia> _repositorioHorarioVigencia;
        private readonly IRepositorio<HorarioDeTrabalhoDia> _repositorioHorarioDia;
        private readonly IRepositorio<OcorrenciaDoEspelhoPonto> _repositorioOcorrencia;

        public ServicoDeFolhaDePonto(
            IRepositorio<PontoDoDia> repositorio,
            IRepositorio<Servidor> repositorioServidor,
            IRepositorio<LotacaoUnidadeOrganizacional> repositorioLotacao,
            IRepositorio<JustificativaDeAusencia> repositorioJustificativa,
            IRepositorio<VinculoDeTrabalho> repositorioVinculo,
            IRepositorio<Afastamento> repositorioAfastamento,
            IRepositorio<EscalaDoServidor> repositorioEscala,
            IRepositorio<RegistroAplicativo> repositorioRegistroAplicativo,
            IRepositorio<RegistroDePonto> repositorioRegistroDePonto,
            IRepositorio<RegraHoraExtra> repositorioRegraHoraExtra,
            IRepositorio<FaixaHoraExtra> repositorioFaixaHoraExtra,
            IRepositorio<PontoDoDiaHoraExtra> repositorioPontoDoDiaHoraExtra,
            IRepositorio<LogAuditoriaPonto> repositorioAuditoriaPonto,
            IUsuarioAtual usuarioAtual,
            IRepositorio<HorarioDeTrabalhoVigencia> repositorioHorarioVigencia,
            IRepositorio<HorarioDeTrabalhoDia> repositorioHorarioDia,
            IRepositorio<OcorrenciaDoEspelhoPonto> repositorioOcorrencia)
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
            _repositorioRegraHoraExtra = repositorioRegraHoraExtra;
            _repositorioFaixaHoraExtra = repositorioFaixaHoraExtra;
            _repositorioPontoDoDiaHoraExtra = repositorioPontoDoDiaHoraExtra;
            _repositorioAuditoriaPonto = repositorioAuditoriaPonto;
            _usuarioAtual = usuarioAtual;
            _repositorioHorarioVigencia = repositorioHorarioVigencia;
            _repositorioHorarioDia = repositorioHorarioDia;
            _repositorioOcorrencia = repositorioOcorrencia;
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
            string acao,
            string motivoAcao,
            bool desconsideraRegistroAtual)
        {
            try
            {
                var pontoDoDia = ObtenhaPontoDoDia(vinculoDeTrabalhoId, data);
                pontoDoDia.OrganizacaoId = organizacaoId;

                if (pontoDoDia.Id == 0)
                    pontoDoDia.VinculoDeTrabalho = _repositorioVinculo.Obtenha(vinculoDeTrabalhoId);

                if (justificativaId.HasValue)
                {
                    ValideLimiteDeUso(pontoDoDia, justificativaId.Value, acao);
                }

                var campo = CampoAmigavel(acao);
                var antesValor = pontoDoDia != null ? ValorDoCampo(pontoDoDia, acao) : "-";
                int? registroDePontoDesconsiderarId = null;

                Func<int?, JustificativaDeAusencia> CarregueJustificativa = new Func<int?, JustificativaDeAusencia>((id) =>
                {
                    return id.HasValue ?
                        _repositorioJustificativa.Obtenha(id.Value) :
                        null;
                });

                switch (acao.ToLower())
                {
                    case "entrada1":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoEntrada1 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Entrada1 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoEntrada1Id;
                            pontoDoDia.RegistroDePontoEntrada1Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada1 != valorHora)))
                            {
                                pontoDoDia.TipoEntrada1 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoEntrada1 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Entrada1 = valorHora;
                            pontoDoDia.JustificativaPeriodo1Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo1 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada1.HasValue || pontoDoDia.Saida1.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.1/SAI.1).");
                        }

                        break;
                    case "saida1":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoSaida1 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Saida1 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoSaida1Id;
                            pontoDoDia.RegistroDePontoSaida1Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida1 != valorHora)))
                            {
                                pontoDoDia.TipoSaida1 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoSaida1 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Saida1 = valorHora;
                            pontoDoDia.JustificativaPeriodo1Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo1 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada1.HasValue || pontoDoDia.Saida1.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.1/SAI.1).");
                        }

                        break;

                    case "entrada2":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoEntrada2 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Entrada2 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoEntrada2Id;
                            pontoDoDia.RegistroDePontoEntrada2Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada2 != valorHora)))
                            {
                                pontoDoDia.TipoEntrada2 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoEntrada2 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Entrada2 = valorHora;
                            pontoDoDia.JustificativaPeriodo2Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo2 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada2.HasValue || pontoDoDia.Saida2.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.2/SAI.2).");
                        }

                        break;
                    case "saida2":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoSaida2 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Saida2 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoSaida2Id;
                            pontoDoDia.RegistroDePontoSaida2Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida2 != valorHora)))
                            {
                                pontoDoDia.TipoSaida2 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoSaida2 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Saida2 = valorHora;
                            pontoDoDia.JustificativaPeriodo2Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo2 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada2.HasValue || pontoDoDia.Saida2.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.2/SAI.2).");
                        }

                        break;

                    case "entrada3":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoEntrada3 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Entrada3 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoEntrada3Id;
                            pontoDoDia.RegistroDePontoEntrada3Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada3 != valorHora)))
                            {
                                pontoDoDia.TipoEntrada3 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoEntrada3 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Entrada3 = valorHora;
                            pontoDoDia.JustificativaPeriodo3Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo3 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada3.HasValue || pontoDoDia.Saida3.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.3/SAI.3).");
                        }

                        break;
                    case "saida3":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoSaida3 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Saida3 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoSaida3Id;
                            pontoDoDia.RegistroDePontoSaida3Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida3 != valorHora)))
                            {
                                pontoDoDia.TipoSaida3 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoSaida3 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Saida3 = valorHora;
                            pontoDoDia.JustificativaPeriodo3Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo3 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada3.HasValue || pontoDoDia.Saida3.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.3/SAI.3).");
                        }

                        break;

                    case "entrada4":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoEntrada4 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Entrada4 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoEntrada4Id;
                            pontoDoDia.RegistroDePontoEntrada4Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada4 != valorHora)))
                            {
                                pontoDoDia.TipoEntrada4 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoEntrada4 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Entrada4 = valorHora;
                            pontoDoDia.JustificativaPeriodo4Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo4 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada4.HasValue || pontoDoDia.Saida4.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.4/SAI.4).");
                        }

                        break;
                    case "saida4":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoSaida4 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Saida4 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoSaida4Id;
                            pontoDoDia.RegistroDePontoSaida4Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida4 != valorHora)))
                            {
                                pontoDoDia.TipoSaida4 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoSaida4 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Saida4 = valorHora;
                            pontoDoDia.JustificativaPeriodo4Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo4 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada4.HasValue || pontoDoDia.Saida4.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.4/SAI.4).");
                        }

                        break;

                    case "entrada5":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoEntrada5 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Entrada5 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoEntrada5Id;
                            pontoDoDia.RegistroDePontoEntrada5Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Entrada5 != valorHora)))
                            {
                                pontoDoDia.TipoEntrada5 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoEntrada5 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Entrada5 = valorHora;
                            pontoDoDia.JustificativaPeriodo5Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo5 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada5.HasValue || pontoDoDia.Saida5.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.5/SAI.5).");
                        }

                        break;
                    case "saida5":
                        if (desconsideraRegistroAtual)
                        {
                            pontoDoDia.TipoSaida5 = eTipoDeRegistroDePeriodo.SemRegistro;
                            pontoDoDia.Saida5 = null;
                            registroDePontoDesconsiderarId = pontoDoDia.RegistroDePontoSaida5Id;
                            pontoDoDia.RegistroDePontoSaida5Id = null;
                        }
                        else
                        {
                            if (valorHora.HasValue && (pontoDoDia.Id == 0 || (pontoDoDia.Saida5 != valorHora)))
                            {
                                pontoDoDia.TipoSaida5 = eTipoDeRegistroDePeriodo.RegistroManual;
                            }
                            else if (!valorHora.HasValue)
                                pontoDoDia.TipoSaida5 = eTipoDeRegistroDePeriodo.SemRegistro;

                            pontoDoDia.Saida5 = valorHora;
                            pontoDoDia.JustificativaPeriodo5Id = justificativaId;
                            pontoDoDia.JustificativaPeriodo5 = CarregueJustificativa(justificativaId);
                            if (justificativaId.HasValue && (pontoDoDia.Entrada5.HasValue || pontoDoDia.Saida5.HasValue))
                                throw new ApplicationException("Você não pode registrar a justificativa pois existe um registro de hora nesse período (ENT.5/SAI.5).");
                        }

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

                var depoisValor = pontoDoDia != null ? ValorDoCampo(pontoDoDia, acao) : ValorDoCampo(pontoDoDia, acao);
                var tipoMudanca = TipoDeMudanca(antesValor, depoisValor);

                if (registroDePontoDesconsiderarId.HasValue)
                    DesconsiderarRegistroDePonto(organizacaoId, pontoDoDia.Id, registroDePontoDesconsiderarId.Value, motivoAcao);
                else
                    Auditar(organizacaoId,
                        vinculoDeTrabalhoId,
                        new MesAno(pontoDoDia.Data),
                        pontoDoDia.Id,
                        "AJUSTE_PONTO_DIA",
                        $"{tipoMudanca} em \"{campo}\" no dia {pontoDoDia.Data:dd/MM/yyyy}: {antesValor} → {depoisValor}");

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

                var vigenciasBase = CarregueVigenciasComDias(organizacaoId, vinculoDeTrabalho.HorarioDeTrabalhoId, fim);
                var idxVigBase = 0;

                var saldoBh = ObtenhaSaldoBancoDeHorasAnterior(vinculoDeTrabalhoId, inicio);


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
                    var vig = ObtenhaVigenciaVigente(vigenciasBase, dataAuxiliar, ref idxVigBase);

                    var eventoNoDia = eventosDoPeriodo
                        .FirstOrDefault(c => c.Data.Date == dataAuxiliar.Date);
                    var afastamento = afastamentosDoPeriodo
                        .FirstOrDefault(d => d.Inicio.Date <= dataAuxiliar.Date && (!d.Fim.HasValue || d.Fim.Value.Date >= dataAuxiliar.Date));
                    var horarioDoDia = vig.Dias.FirstOrDefault(c => c.DiaDaSemana == (eDiaDaSemana)dataAuxiliar.DayOfWeek);

                    var escalaNoDia = escalasDoPeriodo
                        .FirstOrDefault(c =>
                            (c.Escala.Tipo == eTipoDeEscala.Mensal && c.Data.Date == dataAuxiliar.Date) ||
                            (c.Escala.Tipo == eTipoDeEscala.Ciclica && c.Data <= dataAuxiliar && (!c.DataFim.HasValue || c.DataFim >= dataAuxiliar)));

                    if (escalaNoDia != null)
                    {
                        var vigenciaOverrideId = TryGetIntProp(escalaNoDia, "HorarioDeTrabalhoVigenciaId");
                        var horarioOverrideId = TryGetIntProp(escalaNoDia, "HorarioDeTrabalhoId");

                        if (vigenciaOverrideId.HasValue)
                        {
                            vig = CarregueVigenciaPorIdComDias(organizacaoId, vigenciaOverrideId.Value);
                        }
                        else if (horarioOverrideId.HasValue && horarioOverrideId.Value != vinculoDeTrabalho.HorarioDeTrabalhoId)
                        {
                            var vigsOutro = CarregueVigenciasComDias(organizacaoId, horarioOverrideId.Value, fim);
                            int idxOutro = 0;
                            vig = ObtenhaVigenciaVigente(vigsOutro, dataAuxiliar, ref idxOutro);
                        }

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
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoEntrada1 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }

                                if (!pontoDoDia.Saida1.HasValue && !pontoDoDia.JustificativaPeriodo1Id.HasValue)
                                {
                                    pontoDoDia.Saida1 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida1Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoSaida1 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }

                                if (!pontoDoDia.Entrada2.HasValue && !pontoDoDia.JustificativaPeriodo2Id.HasValue)
                                {
                                    pontoDoDia.Entrada2 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada2Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoEntrada2 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }

                                if (!pontoDoDia.Saida2.HasValue && !pontoDoDia.JustificativaPeriodo2Id.HasValue)
                                {
                                    pontoDoDia.Saida2 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida2Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoSaida2 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }

                                if (!pontoDoDia.Entrada3.HasValue && !pontoDoDia.JustificativaPeriodo3Id.HasValue)
                                {
                                    pontoDoDia.Entrada3 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada3Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoEntrada3 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }

                                if (!pontoDoDia.Saida3.HasValue && !pontoDoDia.JustificativaPeriodo3Id.HasValue)
                                {
                                    pontoDoDia.Saida3 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida3Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoSaida3 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }

                                if (!pontoDoDia.Entrada4.HasValue && !pontoDoDia.JustificativaPeriodo4Id.HasValue)
                                {
                                    pontoDoDia.Entrada4 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada4Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoEntrada4 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }

                                if (!pontoDoDia.Saida4.HasValue && !pontoDoDia.JustificativaPeriodo4Id.HasValue)
                                {
                                    pontoDoDia.Saida4 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida4Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoSaida4 = eTipoDeRegistroDePeriodo.RegistroAplicativo;

                                    continue;
                                }

                                if (!pontoDoDia.Entrada5.HasValue && !pontoDoDia.JustificativaPeriodo5Id.HasValue)
                                {
                                    pontoDoDia.Entrada5 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoEntrada5Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoEntrada4 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }

                                if (!pontoDoDia.Saida5.HasValue && !pontoDoDia.JustificativaPeriodo5Id.HasValue)
                                {
                                    pontoDoDia.Saida5 = TruncarSegundos(registroNaPosicao.DataHoraRegistro.TimeOfDay);
                                    pontoDoDia.RegistroDePontoSaida5Id = registroNaPosicao.Id;
                                    if (registroNaPosicao.RegistroAplicativoId.HasValue)
                                        pontoDoDia.TipoSaida5 = eTipoDeRegistroDePeriodo.RegistroAplicativo;
                                    continue;
                                }
                            }

                            if (horarioDoDia != null && afastamento == null)
                            {
                                if (pontoDoDia.Id == 0)
                                {
                                    var configIntervalo = vig.IntervaloAutomatico;
                                    ProcessarIntervalosAutomaticos(pontoDoDia, horarioDoDia, configIntervalo);
                                }
                            }
                        }

                        CalculeCargaHorariaDoDia(ref pontoDoDia, eventoNoDia, horarioDoDia, afastamento, vig.TipoCargaHoraria);
                        CalculeHorasTrabalhadas(ref pontoDoDia, horarioDoDia, vig);
                        CalculeHorasTrabalhadasConsiderandoAbono(ref pontoDoDia, eventoNoDia, horarioDoDia, afastamento, vig);

                        ApliqueToleranciaDsr(ref pontoDoDia, vig, horarioDoDia, eventoNoDia, afastamento);

                        CalculeHorasPositivas(ref pontoDoDia, afastamento);
                        CalculeHorasNegativas(ref pontoDoDia, afastamento);

                        ApliqueToleranciaNoSaldo(ref pontoDoDia, vig.ToleranciaDiariaEmMinutos > 0 ? vig.ToleranciaDiariaEmMinutos : 0);

                        var resumoAprovada = GerarEventosHoraExtraDoDia(organizacaoId, pontoDoDia, vig, eventoNoDia);

                        var creditosDia = ObtenhaCreditosDiaParaBH(pontoDoDia, vig, resumoAprovada);
                        saldoBh = CalculeBancoDeHorasComPrioridade(ref pontoDoDia, vig, saldoBh, creditosDia);
                    }

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

                var totalDias = pontosDoPeriodo.Count;

                foreach (var dia in pontosDoPeriodo)
                {
                    var registroPersistido = _repositorio.Obtenha(dia.Id);

                    if (registroPersistido.RegistroDePontoEntrada1Id.HasValue && 
                        registroPersistido.RegistroDePontoEntrada1.RegistroAplicativoId.HasValue && 
                        registroPersistido.RegistroDePontoEntrada1.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada1.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada1.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada1);
                        registroPersistido.RegistroDePontoEntrada1Id = null;
                    }

                    if (registroPersistido.RegistroDePontoEntrada2Id.HasValue &&
                        registroPersistido.RegistroDePontoEntrada2.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoEntrada2.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada2.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada2.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada2);
                        registroPersistido.RegistroDePontoEntrada2Id = null;
                    }

                    if (registroPersistido.RegistroDePontoEntrada3Id.HasValue &&
                        registroPersistido.RegistroDePontoEntrada3.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoEntrada3.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada3.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada3.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada3);
                        registroPersistido.RegistroDePontoEntrada3Id = null;
                    }

                    if (registroPersistido.RegistroDePontoEntrada4Id.HasValue &&
                        registroPersistido.RegistroDePontoEntrada4.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoEntrada4.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada4.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada4.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada4);
                        registroPersistido.RegistroDePontoEntrada4Id = null;
                    }

                    if (registroPersistido.RegistroDePontoEntrada5Id.HasValue &&
                        registroPersistido.RegistroDePontoEntrada5.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoEntrada5.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoEntrada5.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoEntrada5.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoEntrada5);
                        registroPersistido.RegistroDePontoEntrada5Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida1Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida1.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida1.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida1.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida1.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida1);
                        registroPersistido.RegistroDePontoSaida1Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida2Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida2.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida2.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida2.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida2.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida2);
                        registroPersistido.RegistroDePontoSaida2Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida3Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida3.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida3.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida3.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida3.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida3);
                        registroPersistido.RegistroDePontoSaida3Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida4Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida4.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida4.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida4.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida4.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida4);
                        registroPersistido.RegistroDePontoSaida4Id = null;
                    }

                    if (registroPersistido.RegistroDePontoSaida5Id.HasValue &&
                        registroPersistido.RegistroDePontoSaida5.RegistroAplicativoId.HasValue &&
                        registroPersistido.RegistroDePontoSaida5.RegistroAplicativo.Situacao == eSituacaoRegistroAplicativo.Aprovado)
                    {
                        registroPersistido.RegistroDePontoSaida5.RegistroAplicativo.Situacao = eSituacaoRegistroAplicativo.AguardandoAvaliacao;
                        _repositorioRegistroAplicativo.Atualizar(registroPersistido.RegistroDePontoSaida5.RegistroAplicativo);

                        _repositorioRegistroDePonto.Remover(registroPersistido.RegistroDePontoSaida5);
                        registroPersistido.RegistroDePontoSaida5Id = null;
                    }

                    if (registroPersistido.ListaDeHoraExtra != null && registroPersistido.ListaDeHoraExtra.Any())
                    {
                        foreach (var item in registroPersistido.ListaDeHoraExtra)
                            _repositorioPontoDoDiaHoraExtra.Remover(item);
                    }

                    registroPersistido.AfastamentoId = null;
                    registroPersistido.BancoDeHorasAjuste = null;
                    registroPersistido.BancoDeHorasCredito = null;
                    registroPersistido.BancoDeHorasDebito = null;
                    registroPersistido.CargaHoraria = null;
                    registroPersistido.Entrada1 = null;
                    registroPersistido.Entrada2 = null;
                    registroPersistido.Entrada3 = null;
                    registroPersistido.Entrada4 = null;
                    registroPersistido.Entrada5 = null;
                    registroPersistido.Saida1 = null;
                    registroPersistido.Saida2 = null;
                    registroPersistido.Saida3 = null;
                    registroPersistido.Saida4 = null;
                    registroPersistido.Saida5 = null;
                    registroPersistido.JustificativaPeriodo1Id = null;
                    registroPersistido.JustificativaPeriodo2Id = null;
                    registroPersistido.JustificativaPeriodo3Id = null;
                    registroPersistido.JustificativaPeriodo4Id = null;
                    registroPersistido.JustificativaPeriodo5Id = null;
                    registroPersistido.HorasTrabalhadas = null;
                    registroPersistido.HorasTrabalhadasConsiderandoAbono = null;
                    registroPersistido.HorasPositivas = null;
                    registroPersistido.HorasNegativas = null;
                    registroPersistido.TipoEntrada1 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoEntrada2 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoEntrada3 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoEntrada4 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoEntrada5 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoSaida1 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoSaida2 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoSaida3 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoSaida4 = eTipoDeRegistroDePeriodo.SemRegistro;
                    registroPersistido.TipoSaida5 = eTipoDeRegistroDePeriodo.SemRegistro;

                    _repositorio.Atualizar(registroPersistido);
                }

                List<RegistroDePonto> registrosDePonto = ObtenhaRegistrosDePontoDoPeriodo(vinculoDeTrabalhoId, unidadeLotacaoId, inicio, fim, true);
                foreach (var item in registrosDePonto)
                {
                    var registroPersistido = _repositorioRegistroDePonto.Obtenha(item.Id);
                    registroPersistido.Desconsiderado = false;
                    registroPersistido.AprovadoForaTolerancia = null;
                    registroPersistido.AcaoAprovacao = null;
                    registroPersistido.MotivoAprovacaoTolerancia = null;
                    registroPersistido.UsuarioAprovacaoToleranciaNome = null;
                    registroPersistido.DataAprovacaoTolerancia = null;
                    _repositorioRegistroDePonto.Atualizar(registroPersistido);
                }

                _repositorio.Commit();

                Auditar(organizacaoId,
                    vinculoDeTrabalhoId,
                    mesAno,
                    null,
                    "RESET_FOLHA",
                    $"Reset da folha do mês {mesAno} (remoção de lançamentos manuais e reprocessamento).");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ObtenhaRegistrosDePonto(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            int unidadeLotacaoId,
            MesAno mesAno,
            out VinculoDeTrabalho vinculoDeTrabalho,
            out DateTime inicio,
            out DateTime fim,
            out List<PontoDoDia> pontosDoPeriodo)
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

                var qtd = pontosDoPeriodo.Count();

                Auditar(organizacaoId,
                        vinculoDeTrabalhoId,
                        mesAno,
                        null,
                        fechar ? "FECHAR_FOLHA" : "ABRIR_FOLHA",
                        fechar
                            ? $"Fechamento da folha do mês {mesAno}."
                            : $"Reabertura da folha do mês {mesAno}.");
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

                var antesSit = registroAplicativo.Situacao;
                var dataHora = registroAplicativo.DataHora;

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

                    var horarioId = registroAplicativo.VinculoDeTrabalho.HorarioDeTrabalhoId;
                    var vigs = CarregueVigenciasComDias(registroAplicativo.OrganizacaoId, horarioId, registroAplicativo.DataFinalAtestado.Value);
                    int idx = 0;

                    foreach (var pontoDia in pontosDoPeriodo)
                    {
                        var vig = ObtenhaVigenciaVigente(vigs, pontoDia.Data, ref idx);
                        var utiliza5Registros = vig.UtilizaCincoPeriodos;

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

                Auditar(registroAplicativo.OrganizacaoId,
                    registroAplicativo.VinculoDeTrabalhoId,
                    mesAno,
                    null,
                    "APROVAR_REGISTRO_APP",
                    $"Aprovação de registro do aplicativo em {dataHora:dd/MM/yyyy HH:mm}.");
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

                var dataHora = registroAplicativo.DataHora;

                registroAplicativo.Situacao = eSituacaoRegistroAplicativo.Reprovado;
                _repositorioRegistroAplicativo.Atualizar(registroAplicativo);
                _repositorioRegistroAplicativo.Commit();

                Auditar(registroAplicativo.OrganizacaoId,
                    registroAplicativo.VinculoDeTrabalhoId,
                    new MesAno(registroAplicativo.DataHora),
                    null,
                    "REPROVAR_REGISTRO_APP",
                    $"Reprovação de registro do aplicativo em {dataHora:dd/MM/yyyy HH:mm}.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string ObtenhaObservacaoDoServidorNaFolhaDePonto(int vinculoDeTrabalhoId)
        {
            try
            {
                return _repositorio.ConsultaDapper<string>(
                    @"select
	                    ser.AlertaManutencaoDePonto
                    from servidor ser
                    inner join vinculodetrabalho vin
	                    on vin.ServidorId = ser.Id
                    where
	                    vin.Id = @VINCULOID
                    limit 1", new { @VINCULOID = vinculoDeTrabalhoId }).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PontoDoDiaHoraExtra> ObtenhaHorasExtrasDoDia(int pontoDoDiaId)
        {
            return _repositorioPontoDoDiaHoraExtra
                .ObtenhaLista(e => e.PontoDoDiaId == pontoDoDiaId)
                .OrderBy(e => e.TipoDia)
                .ThenBy(e => e.Origem)
                .ThenBy(e => e.Percentual)
                .ToList();
        }

        public void AprovarHoraExtra(int horaExtraId, int minutosAprovados)
        {
            var ev = _repositorioPontoDoDiaHoraExtra.Obtenha(horaExtraId);
            if (ev == null) throw new Exception("Hora extra não encontrada.");

            var antesAprov = ev.MinutosAprovados;
            var antesStatus = ev    .Status;

            minutosAprovados = Math.Max(0, Math.Min(minutosAprovados, ev.Minutos));
            ev.Status = eStatusAprovacaoHoraExtra.Aprovado;
            ev.MinutosAprovados = minutosAprovados;

            _repositorioPontoDoDiaHoraExtra.Atualizar(ev);
            _repositorioPontoDoDiaHoraExtra.Commit();

            var campo = $"HE {ev.Percentual}% ({ev.Origem})";

            Auditar(ev.OrganizacaoId, ev.PontoDoDia.VinculoDeTrabalhoId, new MesAno(ev.PontoDoDia.Data), ev.PontoDoDiaId,
                "APROVAR_HE",
                $"Aprovação de {campo} no dia {ev.PontoDoDia.Data:dd/MM/yyyy}: {antesAprov}min ({antesStatus}) → {antesAprov}min (Aprovado)");
        }

        public void ReprovarHoraExtra(int horaExtraId)
        {
            var ev = _repositorioPontoDoDiaHoraExtra.Obtenha(horaExtraId);
            if (ev == null) throw new Exception("Hora extra não encontrada.");

            var antesAprov = ev.MinutosAprovados;
            var antesStatus = ev.Status;

            ev.Status = eStatusAprovacaoHoraExtra.Reprovado;
            ev.MinutosAprovados = 0;

            _repositorioPontoDoDiaHoraExtra.Atualizar(ev);
            _repositorioPontoDoDiaHoraExtra.Commit();

            var campo = $"HE {ev.Percentual}% ({ev.Origem})";
            Auditar(ev.OrganizacaoId, ev.PontoDoDia.VinculoDeTrabalhoId, new MesAno(ev.PontoDoDia.Data), ev.PontoDoDiaId,
                "REPROVAR_HE",
                $"Reprovação de {campo} no dia {ev.PontoDoDia.Data:dd/MM/yyyy}.");
        }

        public List<PontoDoDiaHoraExtra> HorasExtrasDaFolhaDePonto(int organizacaoId, int vinculoDeTrabalhoId, DateTime inicio, DateTime fim)
        {
            var pontoIds = _repositorio
                .ObtenhaLista(p => p.OrganizacaoId == organizacaoId
                               && p.VinculoDeTrabalhoId == vinculoDeTrabalhoId
                               && p.Data.Date >= inicio.Date
                               && p.Data.Date <= fim.Date)
                .Select(p => p.Id)
                .ToList();

            if (pontoIds.Count == 0) return new List<PontoDoDiaHoraExtra>();

            return _repositorioPontoDoDiaHoraExtra
                .ObtenhaLista(e => e.OrganizacaoId == organizacaoId && pontoIds.Contains(e.PontoDoDiaId))
                .ToList();
        }

        public List<LogAuditoriaPonto> ObtenhaAuditoriaDaFolha(int organizacaoId, int vinculoDeTrabalhoId, MesAno mesAno)
        {
            return _repositorioAuditoriaPonto.ObtenhaLista(l =>
                    l.OrganizacaoId == organizacaoId &&
                    l.VinculoDeTrabalhoId == vinculoDeTrabalhoId &&
                    l.MesAno == mesAno.ToString())
                .OrderBy(l => l.DataHora)
                .ThenBy(c => c.Id)
                .ToList();
        }

        public List<LogAuditoriaPonto> ObtenhaAuditoriaDoDia(int organizacaoId, int pontoDoDiaId)
        {
            return _repositorioAuditoriaPonto.ObtenhaLista(l =>
                    l.OrganizacaoId == organizacaoId &&
                    l.PontoDoDiaId == pontoDoDiaId)
                .OrderBy(l => l.DataHora)
                .ThenBy(c => c.Id)
                .ToList();
        }

        private void CalculeCargaHorariaDoDia(
            ref PontoDoDia pontoDoDia,
            EventoAnual eventoNoDia,
            HorarioDeTrabalhoDia horarioDia,
            Afastamento afastamento,
            eTipoCargaHoraria tipoCargaHoraria)
        {
            pontoDoDia.CargaHoraria = null;

            if (horarioDia == null || (afastamento != null && afastamento.JustificativaDeAusencia.Abono))
                return;

            if (tipoCargaHoraria == eTipoCargaHoraria.MensalFixa)
                return;

            pontoDoDia.CargaHoraria = horarioDia.CalculeCargaHorariaTotal(eventoNoDia != null);
        }

        private TimeSpan? CalculeDuracaoPeriodo(
            TimeSpan? entrada, TimeSpan? saida,
            TimeSpan? entradaEsperada, TimeSpan? saidaEsperada,
            HorarioDeTrabalhoVigencia vigencia)
        {
            if (!entrada.HasValue || !saida.HasValue) return null;

            var entResult = entrada.Value;
            var saiResult = saida.Value;

            if (vigencia != null && entradaEsperada.HasValue && saidaEsperada.HasValue)
            {
                if (entResult < entradaEsperada.Value && entradaEsperada.Value.Subtract(entResult).TotalMinutes <= vigencia.ToleranciaAntesDaEntradaEmMinutos)
                    entResult = entradaEsperada.Value;
                
                if (entResult > entradaEsperada.Value && entResult.Subtract(entradaEsperada.Value).TotalMinutes <= vigencia.ToleranciaAposAEntradaEmMinutos)
                    entResult = entradaEsperada.Value;

                if (saiResult < saidaEsperada.Value && saidaEsperada.Value.Subtract(saiResult).TotalMinutes <= vigencia.ToleranciaAntesDaSaidaEmMinutos)
                    saiResult = saidaEsperada.Value;
                
                if (saiResult > saidaEsperada.Value && saiResult.Subtract(saidaEsperada.Value).TotalMinutes <= vigencia.ToleranciaAposASaidaEmMinutos)
                    saiResult = saidaEsperada.Value;
            }

            return saiResult.Subtract(entResult);
        }

        private void CalculeHorasTrabalhadas(
            ref PontoDoDia pontoDoDia,
            HorarioDeTrabalhoDia horarioDia,
            HorarioDeTrabalhoVigencia vigencia)
        {
            TimeSpan? htPeriodo_1 = CalculeDuracaoPeriodo(pontoDoDia.Entrada1, pontoDoDia.Saida1, horarioDia?.Entrada1, horarioDia?.Saida1, vigencia);
            TimeSpan? htPeriodo_2 = CalculeDuracaoPeriodo(pontoDoDia.Entrada2, pontoDoDia.Saida2, horarioDia?.Entrada2, horarioDia?.Saida2, vigencia);
            TimeSpan? htPeriodo_3 = CalculeDuracaoPeriodo(pontoDoDia.Entrada3, pontoDoDia.Saida3, horarioDia?.Entrada3, horarioDia?.Saida3, vigencia);
            TimeSpan? htPeriodo_4 = CalculeDuracaoPeriodo(pontoDoDia.Entrada4, pontoDoDia.Saida4, horarioDia?.Entrada4, horarioDia?.Saida4, vigencia);
            TimeSpan? htPeriodo_5 = CalculeDuracaoPeriodo(pontoDoDia.Entrada5, pontoDoDia.Saida5, horarioDia?.Entrada5, horarioDia?.Saida5, vigencia);

            TimeSpan? horasTrabalhadas = null;

            if (htPeriodo_1.HasValue || 
                htPeriodo_2.HasValue || 
                htPeriodo_3.HasValue || 
                htPeriodo_4.HasValue || 
                htPeriodo_5.HasValue)
            {
                horasTrabalhadas = TimeSpan.Zero;

                if (htPeriodo_1.HasValue) horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_1.Value);
                if (htPeriodo_2.HasValue) horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_2.Value);
                if (htPeriodo_3.HasValue) horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_3.Value);
                if (htPeriodo_4.HasValue) horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_4.Value);
                if (htPeriodo_5.HasValue) horasTrabalhadas = horasTrabalhadas.Value.Add(htPeriodo_5.Value);
            }

            pontoDoDia.HorasTrabalhadas = horasTrabalhadas;
        }

        private void CalculeHorasTrabalhadasConsiderandoAbono(
            ref PontoDoDia pontoDoDia,
            EventoAnual eventoNoDia,
            HorarioDeTrabalhoDia horarioDia,
            Afastamento afastamento,
            HorarioDeTrabalhoVigencia vigencia)
        {
            TimeSpan? htPeriodo_1 = null;
            TimeSpan? htPeriodo_2 = null;
            TimeSpan? htPeriodo_3 = null;
            TimeSpan? htPeriodo_4 = null;
            TimeSpan? htPeriodo_5 = null;

            if (afastamento != null)
            {
                if (afastamento.JustificativaDeAusencia.Abono && horarioDia != null)
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
                    CalculeDuracaoPeriodo(pontoDoDia.Entrada1, pontoDoDia.Saida1, horarioDia?.Entrada1, horarioDia?.Saida1, vigencia) :
                    pontoDoDia.JustificativaPeriodo1Id.HasValue && pontoDoDia.JustificativaPeriodo1.Abono ?
                        horarioDia?.CargaHorariaPeriodo(1) : null;

                htPeriodo_2 = pontoDoDia.Entrada2.HasValue && pontoDoDia.Saida2.HasValue ?
                    CalculeDuracaoPeriodo(pontoDoDia.Entrada2, pontoDoDia.Saida2, horarioDia?.Entrada2, horarioDia?.Saida2, vigencia) :
                    pontoDoDia.JustificativaPeriodo2Id.HasValue && pontoDoDia.JustificativaPeriodo2.Abono ?
                        horarioDia?.CargaHorariaPeriodo(2) : null;

                htPeriodo_3 = pontoDoDia.Entrada3.HasValue && pontoDoDia.Saida3.HasValue ?
                    CalculeDuracaoPeriodo(pontoDoDia.Entrada3, pontoDoDia.Saida3, horarioDia?.Entrada3, horarioDia?.Saida3, vigencia) :
                    pontoDoDia.JustificativaPeriodo3Id.HasValue && pontoDoDia.JustificativaPeriodo3.Abono ?
                        horarioDia?.CargaHorariaPeriodo(3) : null;

                htPeriodo_4 = pontoDoDia.Entrada4.HasValue && pontoDoDia.Saida4.HasValue ?
                    CalculeDuracaoPeriodo(pontoDoDia.Entrada4, pontoDoDia.Saida4, horarioDia?.Entrada4, horarioDia?.Saida4, vigencia) :
                    pontoDoDia.JustificativaPeriodo4Id.HasValue && pontoDoDia.JustificativaPeriodo4.Abono ?
                        horarioDia?.CargaHorariaPeriodo(4) : null;

                htPeriodo_5 = pontoDoDia.Entrada5.HasValue && pontoDoDia.Saida5.HasValue ?
                    CalculeDuracaoPeriodo(pontoDoDia.Entrada5, pontoDoDia.Saida5, horarioDia?.Entrada5, horarioDia?.Saida5, vigencia) :
                    pontoDoDia.JustificativaPeriodo5Id.HasValue && pontoDoDia.JustificativaPeriodo5.Abono ?
                        horarioDia?.CargaHorariaPeriodo(5) : null;
            }

            TimeSpan? horasTrabalhadass = null;

            if (htPeriodo_1.HasValue || htPeriodo_2.HasValue || htPeriodo_3.HasValue || htPeriodo_4.HasValue || htPeriodo_5.HasValue)
            {
                horasTrabalhadass = TimeSpan.Zero;

                if (htPeriodo_1.HasValue) horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_1.Value);
                if (htPeriodo_2.HasValue) horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_2.Value);
                if (htPeriodo_3.HasValue) horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_3.Value);
                if (htPeriodo_4.HasValue) horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_4.Value);
                if (htPeriodo_5.HasValue) horasTrabalhadass = horasTrabalhadass.Value.Add(htPeriodo_5.Value);
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

        private Tuple<TimeSpan?, TimeSpan?> ObtenhaCreditoDebitoDoPeriodoAnterior(int vinculoDeTrabalhoId, DateTime inicioPeriodo)
        {
            var query = @"select
	                    BancoDeHorasCredito as 'Item1',
                        BancoDeHorasDebito as 'Item2'
                  from pontododia
                  where VinculoDeTrabalhoId = @VINCULODETRABALHOID
                    and Data < @INICIO
                  order by Data desc
                  limit 1";

            return _repositorio.ConsultaDapper<Tuple<TimeSpan?, TimeSpan?>>(query, new
            {
                @VINCULODETRABALHOID = vinculoDeTrabalhoId,
                @INICIO = inicioPeriodo
            }).FirstOrDefault();
        }

        private BancoDeHorasSaldo ObtenhaSaldoBancoDeHorasAnterior(int vinculoId, DateTime inicioPeriodo)
        {
            var sql = @"
                select BancoDeHorasCredito, BancoDeHorasDebito, BancoDeHorasCreditosJson
                from PontoDoDia
                where VinculoDeTrabalhoId = @V
                  and Data < @INI
                order by Data desc
                limit 1;";

            var row = _repositorio.ConsultaDapper<dynamic>(sql, new { @V = vinculoId, @INI = inicioPeriodo.Date }).FirstOrDefault();
            var saldo = new BancoDeHorasSaldo();

            if (row == null) return saldo;

            // tenta json
            try
            {
                string json = row.BancoDeHorasCreditosJson as string;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    saldo.CreditosPorPercentual =
                        System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(json)
                        ?? new Dictionary<int, int>();
                }
            }
            catch { /* ignora */ }

            // fallback legado: se não tem JSON, joga o crédito como bucket 0
            try
            {
                TimeSpan? cred = row.BancoDeHorasCredito as TimeSpan?;
                TimeSpan? deb = row.BancoDeHorasDebito as TimeSpan?;

                if ((saldo.CreditosPorPercentual == null || saldo.CreditosPorPercentual.Count == 0) && cred.HasValue && cred.Value > TimeSpan.Zero)
                    saldo.CreditosPorPercentual[0] = (int)Math.Round(cred.Value.TotalMinutes);

                if (deb.HasValue && deb.Value > TimeSpan.Zero)
                    saldo.DebitoMinutos = (int)Math.Round(deb.Value.TotalMinutes);
            }
            catch { }

            return saldo;
        }

        private static List<int> ParsePrioridade(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<int> { 100, 70, 50 };

            return texto.Split(',')
                .Select(x => x.Trim())
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();
        }

        private void CalculeBancoDeHoras(
            ref PontoDoDia pontoDoDia,
            HorarioDeTrabalhoVigencia vigencia,
            Tuple<TimeSpan?, TimeSpan?> bancoDeHorasDiaAnterior)
        {
            pontoDoDia.BancoDeHorasCredito = null;
            pontoDoDia.BancoDeHorasDebito = null;

            var inicioBh = vigencia.InicioBancoDeHoras?.Date ?? DateTime.MinValue.Date;

            if (!vigencia.UtilizaBancoDeHoras || inicioBh > pontoDoDia.Data.Date)
                return;

            var saldoAnterior = (bancoDeHorasDiaAnterior.Item1 ?? TimeSpan.Zero)
                              - (bancoDeHorasDiaAnterior.Item2 ?? TimeSpan.Zero);

            var saldoDia = (pontoDoDia.HorasPositivas ?? TimeSpan.Zero)
                         - (pontoDoDia.HorasNegativas ?? TimeSpan.Zero);

            var ajusteManual = pontoDoDia.BancoDeHorasAjuste ?? TimeSpan.Zero;

            var saldoFinal = saldoAnterior + saldoDia + ajusteManual;

            if (saldoFinal > TimeSpan.Zero)
            {
                pontoDoDia.BancoDeHorasCredito = saldoFinal;
                pontoDoDia.BancoDeHorasDebito = TimeSpan.Zero;
            }
            else if (saldoFinal < TimeSpan.Zero)
            {
                pontoDoDia.BancoDeHorasCredito = TimeSpan.Zero;
                pontoDoDia.BancoDeHorasDebito = saldoFinal.Duration();
            }
            else
            {
                pontoDoDia.BancoDeHorasCredito = TimeSpan.Zero;
                pontoDoDia.BancoDeHorasDebito = TimeSpan.Zero;
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
            DateTime fim,
            bool somenteDesconsiderados = false)
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
                            AND r.Desconsiderado = @SOMENTEDESCONSIDERADOS
		                    and l.UnidadeOrganizacionalId = @UNIDADEID))
                    union
                    (select
	                    r.*
                    from registrodeponto r
                    inner join registroaplicativo a
	                    on a.Id = r.RegistroAplicativoId
                    where
	                    a.VinculoDeTrabalhoId = @VINCULOID
                        AND r.Desconsiderado = @SOMENTEDESCONSIDERADOS
                        and date(a.DataHora) between date(@INICIO) and date(@FIM))";

            return _repositorio.ConsultaDapper<RegistroDePonto>(query, new
            {
                @UNIDADEID = unidadeId,
                @VINCULOID = vinculoDeTrabalhoId,
                @INICIO = inicio,
                @FIM = fim,
                @SOMENTEDESCONSIDERADOS = somenteDesconsiderados
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

                var origemCampo = CampoAmigavel(classe);
                var valorMovido = pontoDia != null ? ValorDoCampo(pontoDia, classe) : "-";

                var entrada1 = pontoDia.Entrada1;
                var registroAppEntrada1 = pontoDia.RegistroDePontoEntrada1Id;
                var tipoEntrada1 = pontoDia.TipoEntrada1;

                var saida1 = pontoDia.Saida1;
                var registroAppSaida1 = pontoDia.RegistroDePontoSaida1Id;
                var tipoSaida1 = pontoDia.TipoSaida1;

                var entrada2 = pontoDia.Entrada2;
                var registroAppEntrada2 = pontoDia.RegistroDePontoEntrada2Id;
                var tipoEntrada2 = pontoDia.TipoEntrada2;

                var saida2 = pontoDia.Saida2;
                var registroAppSaida2 = pontoDia.RegistroDePontoSaida2Id;
                var tipoSaida2 = pontoDia.TipoSaida2;

                var entrada3 = pontoDia.Entrada3;
                var registroAppEntrada3 = pontoDia.RegistroDePontoEntrada3Id;
                var tipoEntrada3 = pontoDia.TipoEntrada3;

                var saida3 = pontoDia.Saida3;
                var registroAppSaida3 = pontoDia.RegistroDePontoSaida3Id;
                var tipoSaida3 = pontoDia.TipoSaida3;

                var entrada4 = pontoDia.Entrada4;
                var registroAppEntrada4 = pontoDia.RegistroDePontoEntrada4Id;
                var tipoEntrada4 = pontoDia.TipoEntrada4;

                var saida4 = pontoDia.Saida4;
                var registroAppSaida4 = pontoDia.RegistroDePontoSaida4Id;
                var tipoSaida4 = pontoDia.TipoSaida4;

                var entrada5 = pontoDia.Entrada5;
                var registroAppEntrada5 = pontoDia.RegistroDePontoEntrada5Id;
                var tipoEntrada5 = pontoDia.TipoEntrada5;

                var saida5 = pontoDia.Saida5;
                var registroAppSaida5 = pontoDia.RegistroDePontoSaida5Id;
                var tipoSaida5 = pontoDia.TipoSaida5;

                switch (classe)
                {
                    case "entrada1":
                        pontoDia.Entrada1 = saida1;
                        pontoDia.Saida1 = entrada1;
                        pontoDia.RegistroDePontoEntrada1Id = registroAppSaida1;
                        pontoDia.RegistroDePontoSaida1Id = registroAppEntrada1;
                        pontoDia.TipoEntrada1 = tipoSaida1;
                        pontoDia.TipoSaida1 = tipoEntrada1;
                        break;

                    case "saida1":
                        if (avancar)
                        {
                            pontoDia.Entrada2 = saida1;
                            pontoDia.RegistroDePontoEntrada2Id = registroAppSaida1;
                            pontoDia.Saida1 = entrada2;
                            pontoDia.RegistroDePontoSaida1Id = registroAppEntrada2;
                            pontoDia.TipoEntrada2 = tipoSaida1;
                            pontoDia.TipoSaida1 = tipoEntrada2;
                        }
                        else
                        {
                            pontoDia.Entrada1 = saida1;
                            pontoDia.RegistroDePontoEntrada1Id = registroAppSaida1;
                            pontoDia.Saida1 = entrada1;
                            pontoDia.RegistroDePontoSaida1Id = registroAppEntrada1;
                            pontoDia.TipoEntrada1 = tipoSaida1;
                            pontoDia.TipoSaida1 = tipoEntrada1;
                        }
                        break;

                    case "entrada2":
                        if (avancar)
                        {
                            pontoDia.Saida2 = entrada2;
                            pontoDia.RegistroDePontoSaida2Id = registroAppEntrada2;
                            pontoDia.Entrada2 = saida2;
                            pontoDia.RegistroDePontoEntrada2Id = registroAppSaida2;
                            pontoDia.TipoSaida2 = tipoEntrada2;
                            pontoDia.TipoEntrada2 = tipoSaida2;
                        }
                        else
                        {
                            pontoDia.Saida1 = entrada2;
                            pontoDia.RegistroDePontoSaida1Id = registroAppEntrada2;
                            pontoDia.Entrada2 = saida1;
                            pontoDia.RegistroDePontoEntrada2Id = registroAppSaida1;
                            pontoDia.TipoSaida1 = tipoEntrada2;
                            pontoDia.TipoEntrada2 = tipoSaida1;
                        }
                        break;

                    case "saida2":
                        if (avancar)
                        {
                            pontoDia.Entrada3 = saida2;
                            pontoDia.RegistroDePontoEntrada3Id = registroAppSaida2;
                            pontoDia.Saida2 = entrada3;
                            pontoDia.RegistroDePontoSaida2Id = registroAppEntrada3;
                            pontoDia.TipoEntrada3 = tipoSaida2;
                            pontoDia.TipoSaida2 = tipoEntrada3;
                        }
                        else
                        {
                            pontoDia.Saida2 = entrada2;
                            pontoDia.RegistroDePontoSaida2Id = registroAppEntrada2;
                            pontoDia.Entrada2 = saida2;
                            pontoDia.RegistroDePontoEntrada2Id = registroAppSaida2;
                            pontoDia.TipoSaida2 = tipoEntrada2;
                            pontoDia.TipoEntrada2 = tipoSaida2;
                        }
                        break;

                    case "entrada3":
                        if (avancar)
                        {
                            pontoDia.Saida3 = entrada3;
                            pontoDia.RegistroDePontoSaida3Id = registroAppEntrada3;
                            pontoDia.Entrada3 = saida3;
                            pontoDia.RegistroDePontoEntrada3Id = registroAppSaida3;
                            pontoDia.TipoSaida3 = tipoEntrada3;
                            pontoDia.TipoEntrada3 = tipoSaida3;
                        }
                        else
                        {
                            pontoDia.Saida2 = entrada3;
                            pontoDia.RegistroDePontoSaida2Id = registroAppEntrada3;
                            pontoDia.Entrada3 = saida2;
                            pontoDia.RegistroDePontoEntrada3Id = registroAppSaida2;
                            pontoDia.TipoSaida2 = tipoEntrada3;
                            pontoDia.TipoEntrada3 = tipoSaida2;
                        }
                        break;

                    case "entrada4":
                        if (avancar)
                        {
                            pontoDia.Entrada4 = saida4;
                            pontoDia.RegistroDePontoEntrada4Id = registroAppSaida4;
                            pontoDia.Saida4 = entrada4;
                            pontoDia.RegistroDePontoSaida4Id = registroAppEntrada4;
                            pontoDia.TipoEntrada4 = tipoSaida4;
                            pontoDia.TipoSaida4 = tipoEntrada4;
                        }
                        else
                        {
                            pontoDia.Saida3 = entrada4;
                            pontoDia.RegistroDePontoSaida3Id = registroAppEntrada4;
                            pontoDia.Entrada4 = saida3;
                            pontoDia.RegistroDePontoEntrada4Id = registroAppSaida3;
                            pontoDia.TipoSaida3 = tipoEntrada4;
                            pontoDia.TipoEntrada4 = tipoSaida3;
                        }
                        break;

                    case "saida4":
                        if (avancar)
                        {
                            pontoDia.Entrada5 = saida4;
                            pontoDia.RegistroDePontoEntrada5Id = registroAppSaida4;
                            pontoDia.Saida4 = entrada5;
                            pontoDia.RegistroDePontoSaida4Id = registroAppEntrada5;
                            pontoDia.TipoEntrada5 = tipoSaida4;
                            pontoDia.TipoSaida4 = tipoEntrada5;
                        }
                        else
                        {
                            pontoDia.Entrada4 = saida4;
                            pontoDia.RegistroDePontoEntrada4Id = registroAppSaida4;
                            pontoDia.Saida4 = entrada4;
                            pontoDia.RegistroDePontoSaida4Id = registroAppEntrada4;
                            pontoDia.TipoEntrada4 = tipoSaida4;
                            pontoDia.TipoSaida4 = tipoEntrada4;
                        }
                        break;

                    case "entrada5":
                        if (avancar)
                        {
                            pontoDia.Saida5 = entrada5;
                            pontoDia.RegistroDePontoSaida5Id = registroAppEntrada5;
                            pontoDia.Entrada5 = saida5;
                            pontoDia.RegistroDePontoEntrada5Id = registroAppSaida5;
                            pontoDia.TipoSaida5 = tipoEntrada5;
                            pontoDia.TipoEntrada5 = tipoSaida5;
                        }
                        else
                        {
                            pontoDia.Saida4 = entrada5;
                            pontoDia.RegistroDePontoSaida4Id = registroAppEntrada5;
                            pontoDia.Entrada5 = saida4;
                            pontoDia.RegistroDePontoEntrada5Id = registroAppSaida4;
                            pontoDia.TipoSaida4 = tipoEntrada5;
                            pontoDia.TipoEntrada5 = tipoSaida4;
                        }
                        break;

                    case "saida5":
                        pontoDia.Entrada5 = saida5;
                        pontoDia.RegistroDePontoEntrada5Id = registroAppSaida5;
                        pontoDia.Saida5 = entrada5;
                        pontoDia.RegistroDePontoSaida5Id = registroAppEntrada5;
                        pontoDia.TipoEntrada5 = tipoSaida5;
                        pontoDia.TipoSaida5 = tipoEntrada5;

                        break;
                }


                _repositorio.Atualizar(pontoDia);
                _repositorio.Commit();

                var destinoClasse = ProximoCampo(classe, avancar);
                var destinoCampo = CampoAmigavel(destinoClasse);

                Auditar(pontoDia.OrganizacaoId,
                        pontoDia.VinculoDeTrabalhoId,
                        new MesAno(pontoDia.Data),
                        pontoDia.Id,
                        "MOVER_REGISTRO",
                        $"Movimentação de registro ({valorMovido}) de \"{origemCampo}\" para \"{destinoCampo}\" no dia {pontoDia.Data:dd/MM/yyyy}.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ReconsiderarRegistroDePonto(int organizacaoId, int pontoDoDiaId, int registroDePontoId)
        {
            var r = _repositorioRegistroDePonto.Obtenha(registroDePontoId);
            if (r == null) throw new Exception("Registro de ponto não encontrado.");


            if (!r.Desconsiderado) return;

            var pontoDoDia = _repositorio.Obtenha(pontoDoDiaId);

            r.Desconsiderado = false;
            r.MotivoDesconsideracao = null;
            r.UsuarioDesconsideracaoNome = _usuarioAtual?.Nome ?? "Sistema";
            r.DataDesconsideracao = DateTime.Now;

            _repositorioRegistroDePonto.Atualizar(r);
            _repositorioRegistroDePonto.Commit();

            Auditar(organizacaoId, pontoDoDia.VinculoDeTrabalhoId, new MesAno(pontoDoDia.Data), pontoDoDiaId, "RECONSIDERAR_BATIDA", $"Reconsideração de marcação {r.DataHoraRegistro:dd/MM/yyyy HH:mm}.");
        }

        public List<OcorrenciaDoEspelhoPonto> ObtenhaOcorrenciasDoEspelhoPonto(int organizacaoId, int vinculoId, string mesReferencia)
        {
            var sql = @"
                SELECT * FROM OcorrenciaDoEspelhoPonto 
                WHERE OrganizacaoId = @OrgId 
                  AND VinculoDeTrabalhoId = @VincId 
                  AND MesReferencia = @MesAno
                ORDER BY DataHoraCadastro DESC";

            return _repositorio.ConsultaDapper<OcorrenciaDoEspelhoPonto>(sql, new { OrgId = organizacaoId, VincId = vinculoId, MesAno = mesReferencia });
        }

        public OcorrenciaDoEspelhoPonto SalvarOcorrenciaDoEspelhoPonto(int organizacaoId, int vinculoId, string mesReferencia, string descricao, int usuarioCadastroId, string usuarioCadastroNome)
        {
            var ocorrencia = new OcorrenciaDoEspelhoPonto(organizacaoId, vinculoId, mesReferencia, descricao, usuarioCadastroId, usuarioCadastroNome);
            
            _repositorioOcorrencia.Add(ocorrencia);
            _repositorioOcorrencia.Commit();

            Auditar(organizacaoId, vinculoId, new MesAno(mesReferencia), null, "SALVAR_OCORRENCIA_ESPELHO", $"Inclusão de ocorrência no espelho: {descricao}");

            return ocorrencia;
        }

        public void ExcluirOcorrenciaDoEspelhoPonto(int id)
        {
            var ocorrencia = _repositorioOcorrencia.Obtenha(id);
            if (ocorrencia == null) return;

            Auditar(ocorrencia.OrganizacaoId, ocorrencia.VinculoDeTrabalhoId, new MesAno(ocorrencia.MesReferencia), null, "EXCLUIR_OCORRENCIA_ESPELHO", $"Exclusão de ocorrência no espelho: {ocorrencia.Descricao}");

            _repositorioOcorrencia.Remover(ocorrencia);
            _repositorioOcorrencia.Commit();
        }

        private void DesconsiderarRegistroDePonto(int organizacaoId, int pontoDoDiaId, int registroDePontoId, string motivo)
        {
            var r = _repositorioRegistroDePonto.Obtenha(registroDePontoId);
            if (r == null) throw new Exception("Registro de ponto não encontrado.");

            if (r.Desconsiderado) return;

            var pontoDoDia = _repositorio.Obtenha(pontoDoDiaId);

            r.Desconsiderado = true;
            r.MotivoDesconsideracao = motivo;
            r.UsuarioDesconsideracaoNome = _usuarioAtual?.Nome ?? "Sistema";
            r.DataDesconsideracao = DateTime.Now;

            _repositorioRegistroDePonto.Atualizar(r);

            Auditar(organizacaoId, pontoDoDia.VinculoDeTrabalhoId, new MesAno(pontoDoDia.Data), pontoDoDiaId, "DESCONSIDERAR_BATIDA", $"Desconsideração de marcação {r.DataHoraRegistro:dd/MM/yyyy HH:mm}. Motivo: {motivo}");
        }

        private void ValideLimiteDeUso(PontoDoDia ponto, int justificativaId, string acao)
        {
            var justificativa = _repositorioJustificativa.Obtenha(justificativaId);

            if (justificativa == null || !justificativa.TotalDeUsos.HasValue || justificativa.TipoDeLimite == eTipoDeLimiteDeJustificativa.NaoUtiliza)
                return;

            DateTime dataReferencia = ponto.Data;
            DateTime dataInicio = dataReferencia;
            DateTime dataFim = dataReferencia;

            if (justificativa.TipoDeLimite == eTipoDeLimiteDeJustificativa.Semanal)
            {
                dataInicio = dataReferencia.AddDays(-(int)dataReferencia.DayOfWeek);
                dataFim = dataInicio.AddDays(6);
            }
            else if (justificativa.TipoDeLimite == eTipoDeLimiteDeJustificativa.Mensal)
            {
                dataInicio = new DateTime(dataReferencia.Year, dataReferencia.Month, 1);
                dataFim = dataInicio.AddMonths(1).AddDays(-1);
            }

            var query = _repositorio.ObtenhaLista(p =>
                p.VinculoDeTrabalhoId == ponto.VinculoDeTrabalhoId &&
                p.Id != ponto.Id &&
                p.Data >= dataInicio &&
                p.Data <= dataFim
            );

            var outrosRegistros = query.Select(p => new {
                p.JustificativaPeriodo1Id,
                p.JustificativaPeriodo2Id,
                p.JustificativaPeriodo3Id,
                p.JustificativaPeriodo4Id,
                p.JustificativaPeriodo5Id
            }).ToList();

            int usosNoBanco = 0;
            foreach (var r in outrosRegistros)
            {
                if (r.JustificativaPeriodo1Id == justificativaId) usosNoBanco++;
                if (r.JustificativaPeriodo2Id == justificativaId) usosNoBanco++;
                if (r.JustificativaPeriodo3Id == justificativaId) usosNoBanco++;
                if (r.JustificativaPeriodo4Id == justificativaId) usosNoBanco++;
                if (r.JustificativaPeriodo5Id == justificativaId) usosNoBanco++;
            }

            int usosNesteDia = 0;

            int slotSendoEditado = int.Parse(acao.Substring(acao.Length - 1));

            if (slotSendoEditado != 1 && ponto.JustificativaPeriodo1Id == justificativaId) usosNesteDia++;
            if (slotSendoEditado != 2 && ponto.JustificativaPeriodo2Id == justificativaId) usosNesteDia++;
            if (slotSendoEditado != 3 && ponto.JustificativaPeriodo3Id == justificativaId) usosNesteDia++;
            if (slotSendoEditado != 4 && ponto.JustificativaPeriodo4Id == justificativaId) usosNesteDia++;
            if (slotSendoEditado != 5 && ponto.JustificativaPeriodo5Id == justificativaId) usosNesteDia++;

            int totalGeral = usosNoBanco + usosNesteDia + 1;

            if (totalGeral > justificativa.TotalDeUsos.Value)
            {
                throw new ApplicationException($"O limite de {justificativa.TotalDeUsos.Value} usos para a justificativa '{justificativa.Descricao}' foi excedido.");
            }
        }

        private void ProcessarIntervalosAutomaticos(
            PontoDoDia ponto,
            HorarioDeTrabalhoDia horario,
            eIntervaloAutomatico tipoIntervalo)
        {
            if (tipoIntervalo == eIntervaloAutomatico.NaoUtiliza)
                return;

            (TimeSpan? Valor, eTipoDeRegistroDePeriodo Tipo) ObterValorAutomatico(
                TimeSpan? horarioEsperado,
                TimeSpan? valorAtual,
                eTipoDeRegistroDePeriodo tipoAtual)
            {
                if (!horarioEsperado.HasValue)
                    return (valorAtual, tipoAtual);

                if (valorAtual.HasValue)
                    return (valorAtual, tipoAtual);

                var novoHorario = new TimeSpan(horarioEsperado.Value.Hours, horarioEsperado.Value.Minutes, 0);
                return (novoHorario, eTipoDeRegistroDePeriodo.Automatico);
            }

            switch (tipoIntervalo)
            {
                case eIntervaloAutomatico.Saida1_Entrada2:
                    (ponto.Saida1, ponto.TipoSaida1) = ObterValorAutomatico(horario.Saida1, ponto.Saida1, ponto.TipoSaida1);
                    (ponto.Entrada2, ponto.TipoEntrada2) = ObterValorAutomatico(horario.Entrada2, ponto.Entrada2, ponto.TipoEntrada2);
                    break;

                case eIntervaloAutomatico.Saida2_Entrada3:
                    (ponto.Saida2, ponto.TipoSaida2) = ObterValorAutomatico(horario.Saida2, ponto.Saida2, ponto.TipoSaida2);
                    (ponto.Entrada3, ponto.TipoEntrada3) = ObterValorAutomatico(horario.Entrada3, ponto.Entrada3, ponto.TipoEntrada3);
                    break;

                case eIntervaloAutomatico.Saida3_Entrada4:
                    (ponto.Saida3, ponto.TipoSaida3) = ObterValorAutomatico(horario.Saida3, ponto.Saida3, ponto.TipoSaida3);
                    (ponto.Entrada4, ponto.TipoEntrada4) = ObterValorAutomatico(horario.Entrada4, ponto.Entrada4, ponto.TipoEntrada4);
                    break;

                case eIntervaloAutomatico.Saida4_Entrada5:
                    (ponto.Saida4, ponto.TipoSaida4) = ObterValorAutomatico(horario.Saida4, ponto.Saida4, ponto.TipoSaida4);
                    (ponto.Entrada5, ponto.TipoEntrada5) = ObterValorAutomatico(horario.Entrada5, ponto.Entrada5, ponto.TipoEntrada5);
                    break;
            }
        }

        private void ApliqueToleranciaNoSaldo(ref PontoDoDia ponto, int toleranciaMinutos)
        {
            if (toleranciaMinutos == 0)
                return;

            if (!ponto.HorasPositivas.HasValue && !ponto.HorasNegativas.HasValue)
                return;

            var extras = ponto.HorasPositivas ?? TimeSpan.Zero;
            var faltas = ponto.HorasNegativas ?? TimeSpan.Zero;

            double totalMinutosVariacao = Math.Abs(extras.Subtract(faltas).TotalMinutes);

            if (totalMinutosVariacao <= toleranciaMinutos)
            {
                ponto.HorasPositivas = null;
                ponto.HorasNegativas = null;
            }
        }

        private List<(decimal Percentual, int Minutos)> DistribuirEmFaixas(int minutos, List<FaixaHoraExtra> faixas)
        {
            var saida = new List<(decimal Percentual, int Minutos)>();
            if (minutos <= 0) return saida;

            var restante = minutos;
            foreach (var fx in faixas.Where(f => f.Ativo).OrderBy(f => f.Ordem))
            {
                if (restante <= 0) break;

                var consumir = fx.MinutosAte.HasValue
                    ? Math.Min(restante, fx.MinutosAte.Value)
                    : restante;

                if (consumir > 0)
                {
                    saida.Add((fx.Percentual, consumir));
                    restante -= consumir;
                }
            }

            if (restante > 0 && faixas.Any())
            {
                var last = faixas.Where(f => f.Ativo).OrderBy(f => f.Ordem).Last();
                saida.Add((last.Percentual, restante));
            }

            return saida;
        }

        private eTipoDiaHoraExtra ClassificarTipoDiaHoraExtra(
            PontoDoDia pontoDoDia,
            EventoAnual eventoNoDia,
            HorarioDeTrabalhoVigencia vigencia)
        {
            var ehFeriado = eventoNoDia?.Tipo == eTipoDeEvento.Feriado;
            var ehFacultativo = eventoNoDia?.Tipo == eTipoDeEvento.Facultativo;

            if (ehFeriado) return eTipoDiaHoraExtra.Feriado;

            if (ehFacultativo && vigencia.ConsiderarFacultativoComoFeriadoHoraExtra)
                return eTipoDiaHoraExtra.Feriado;

            bool temCarga = pontoDoDia.CargaHoraria.HasValue && pontoDoDia.CargaHoraria.Value > TimeSpan.Zero;
            return temCarga ? eTipoDiaHoraExtra.DiaTrabalho : eTipoDiaHoraExtra.DiaFolga;
        }

        private Dictionary<int, int> GerarEventosHoraExtraDoDia(
            int organizacaoId,
            PontoDoDia pontoDoDia,
            HorarioDeTrabalhoVigencia vigencia,
            EventoAnual eventoNoDia)
        {
            // Se não tem trabalho, zera eventos
            var minutosTrabalhados = (int)Math.Round((pontoDoDia.HorasTrabalhadasConsiderandoAbono ?? TimeSpan.Zero).TotalMinutes);
            if (minutosTrabalhados <= 0)
            {
                var antigosZero = _repositorioPontoDoDiaHoraExtra.ObtenhaLista(e => e.PontoDoDiaId == pontoDoDia.Id);
                antigosZero.ForEach(_repositorioPontoDoDiaHoraExtra.Remover);
                return new Dictionary<int, int>();
            }

            var tipoDia = ClassificarTipoDiaHoraExtra(pontoDoDia, eventoNoDia, vigencia);

            // Busca regra da VIGÊNCIA p/ este tipo de dia.
            // (Se não existir, fallback pra DiaTrabalho)
            var regra = _repositorioRegraHoraExtra.Obtenha(r =>
                r.OrganizacaoId == organizacaoId &&
                r.HorarioDeTrabalhoVigenciaId == vigencia.Id &&
                r.TipoDia == tipoDia &&
                r.Ativo);

            if (regra == null)
            {
                regra = _repositorioRegraHoraExtra.Obtenha(r =>
                    r.OrganizacaoId == organizacaoId &&
                    r.HorarioDeTrabalhoVigenciaId == vigencia.Id &&
                    r.TipoDia == eTipoDiaHoraExtra.DiaTrabalho &&
                    r.Ativo);
            }

            if (regra == null)
            {
                // Sem regra -> não gera detalhamento (mas HorasPositivas continua existindo)
                return new Dictionary<int, int>();
            }

            var statusPadrao = regra.AprovarAutomaticamente
                ? eStatusAprovacaoHoraExtra.Aprovado
                : eStatusAprovacaoHoraExtra.Pendente;

            int aprovados(int minutos) => regra.AprovarAutomaticamente ? minutos : 0;

            var faixas = _repositorioFaixaHoraExtra
                .ObtenhaLista(f => f.OrganizacaoId == organizacaoId && f.RegraHoraExtraId == regra.Id && f.Ativo)
                .OrderBy(f => f.Ordem)
                .ToList();

            // minutos excedentes (já calculado corretamente no seu fluxo)
            var minutosExcedente = (int)Math.Round((pontoDoDia.HorasPositivas ?? TimeSpan.Zero).TotalMinutes);

            // base da jornada: min(trabalhado, carga)
            int minutosBase = 0;
            if (pontoDoDia.CargaHoraria.HasValue && pontoDoDia.CargaHoraria.Value > TimeSpan.Zero)
            {
                var mCarga = (int)Math.Round(pontoDoDia.CargaHoraria.Value.TotalMinutes);
                minutosBase = Math.Min(minutosTrabalhados, mCarga);
            }

            var diaSemana = (eDiaDaSemana)pontoDoDia.Data.DayOfWeek;
            var ehFeriado = eventoNoDia?.Tipo == eTipoDeEvento.Feriado;
            var ehFacultativo = eventoNoDia?.Tipo == eTipoDeEvento.Facultativo;

            var novos = new List<PontoDoDiaHoraExtra>();

            if (regra.GerarHoraExtraSobreBaseDaJornada && minutosBase > 0 && regra.PercentualBase > 0)
            {
                novos.Add(new PontoDoDiaHoraExtra
                {
                    OrganizacaoId = organizacaoId,
                    PontoDoDiaId = pontoDoDia.Id,
                    TipoDia = tipoDia,
                    DiaDaSemana = diaSemana,
                    EhFeriado = ehFeriado,
                    EhFacultativo = ehFacultativo,
                    Origem = eOrigemHoraExtra.BaseDaJornada,
                    Percentual = regra.PercentualBase,
                    Minutos = minutosBase,
                    Status = statusPadrao,
                    MinutosAprovados = aprovados(minutosBase)
                });
            }

            foreach (var (percentual, minutos) in DistribuirEmFaixas(minutosExcedente, faixas))
            {
                novos.Add(new PontoDoDiaHoraExtra
                {
                    OrganizacaoId = organizacaoId,
                    PontoDoDiaId = pontoDoDia.Id,
                    TipoDia = tipoDia,
                    DiaDaSemana = diaSemana,
                    EhFeriado = ehFeriado,
                    EhFacultativo = ehFacultativo,
                    Origem = eOrigemHoraExtra.Excedente,
                    Percentual = percentual,
                    Minutos = minutos,
                    Status = statusPadrao,
                    MinutosAprovados = aprovados(minutos)
                });
            }

            var antigos = _repositorioPontoDoDiaHoraExtra.ObtenhaLista(e => e.PontoDoDiaId == pontoDoDia.Id);

            // preserva status e minutos aprovados anteriores quando possível
            foreach (var n in novos)
            {
                var match = antigos.FirstOrDefault(a =>
                    a.TipoDia == n.TipoDia &&
                    a.Origem == n.Origem &&
                    a.Percentual == n.Percentual);

                if (match != null)
                {
                    n.Status = match.Status;
                    n.MinutosAprovados = Math.Min(match.MinutosAprovados, n.Minutos);

                    if (n.Status == eStatusAprovacaoHoraExtra.Reprovado)
                        n.MinutosAprovados = 0;
                }
            }

            antigos.ForEach(_repositorioPontoDoDiaHoraExtra.Remover);
            novos.ForEach(_repositorioPontoDoDiaHoraExtra.Add);

            return ResumoAprovadoPorPercentual(novos);
        }

        private Dictionary<int, int> ResumoAprovadoPorPercentual(List<PontoDoDiaHoraExtra> eventos)
        {
            return eventos
                .Where(x => x.Status == eStatusAprovacaoHoraExtra.Aprovado && x.MinutosAprovados > 0)
                .GroupBy(x => (int)x.Percentual)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.MinutosAprovados));
        }   

        private void ApliqueToleranciaDsr(
            ref PontoDoDia pontoDoDia,
            HorarioDeTrabalhoVigencia vigencia,
            HorarioDeTrabalhoDia horarioDoDia,
            EventoAnual eventoNoDia,
            Afastamento afastamento)
        {
            var tolerancia = vigencia.ToleranciaDsrEmMinutos;
            if (tolerancia <= 0)
                return;

            // DSR aqui = dia de descanso do servidor (sem carga) e NÃO é feriado/facultativo.
            // Se tem evento anual (feriado/facultativo), sai.
            if (eventoNoDia != null)
                return;

            // Se estiver afastado, não faz sentido tratar como DSR
            if (afastamento != null)
                return;

            // Mensal fixa não usa carga diária, então ignore DSR
            if (vigencia.TipoCargaHoraria == eTipoCargaHoraria.MensalFixa)
                return;

            bool diaSemCarga = horarioDoDia == null
                || !pontoDoDia.CargaHoraria.HasValue
                || pontoDoDia.CargaHoraria.Value == TimeSpan.Zero;

            if (!diaSemCarga)
                return;

            var trabalhadas = pontoDoDia.HorasTrabalhadasConsiderandoAbono;
            if (!trabalhadas.HasValue)
                return;

            // Se trabalhou pouco no descanso, zera o "trabalhado" para não gerar HE/saldo.
            if (trabalhadas.Value.TotalMinutes <= tolerancia)
            {
                pontoDoDia.HorasTrabalhadas = null;
                pontoDoDia.HorasTrabalhadasConsiderandoAbono = null;

                // Limpa saldos do dia (evita HE/saldo e BH indevido)
                pontoDoDia.HorasPositivas = null;
                pontoDoDia.HorasNegativas = null;
            }
        }

        private void Auditar(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            MesAno mesAno,
            int? pontoDoDiaId,
            string acao,
            string descricao)
        {
            _repositorioAuditoriaPonto.Add(new LogAuditoriaPonto
            {
                OrganizacaoId = organizacaoId,
                VinculoDeTrabalhoId = vinculoDeTrabalhoId,
                PontoDoDiaId = pontoDoDiaId,
                UsuarioNome = _usuarioAtual?.Nome ?? "Sistema",
                DataHora = DateTime.Now,
                Acao = acao,
                Descricao = descricao,
                MesAno = mesAno.ToString()
            });

            _repositorioAuditoriaPonto.Commit();
        }

        private List<HorarioDeTrabalhoVigencia> CarregueVigenciasComDias(int organizacaoId, int horarioDeTrabalhoId, DateTime fim)
        {
            var vigencias = _repositorioHorarioVigencia
                .ObtenhaLista(v =>
                    v.OrganizacaoId == organizacaoId &&
                    v.HorarioDeTrabalhoId == horarioDeTrabalhoId &&
                    v.VigenciaInicio.Date <= fim.Date)
                .OrderBy(v => v.VigenciaInicio)
                .ToList();

            if (!vigencias.Any())
                throw new ApplicationException("Horário sem vigência cadastrada. Execute o backfill.");

            var ids = vigencias.Select(v => v.Id).ToList();

            var dias = _repositorioHorarioDia
                .ObtenhaLista(d => d.OrganizacaoId == organizacaoId && ids.Contains(d.HorarioDeTrabalhoVigenciaId))
                .ToList();

            var diasPorVig = dias
                .GroupBy(d => d.HorarioDeTrabalhoVigenciaId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DiaDaSemana).ToList());

            foreach (var v in vigencias)
                v.Dias = diasPorVig.TryGetValue(v.Id, out var lst) ? lst : new List<HorarioDeTrabalhoDia>();

            return vigencias;
        }

        private static HorarioDeTrabalhoVigencia ObtenhaVigenciaVigente(List<HorarioDeTrabalhoVigencia> vigenciasOrdenadas, DateTime data, ref int idx)
        {
            while (idx + 1 < vigenciasOrdenadas.Count &&
                   vigenciasOrdenadas[idx + 1].VigenciaInicio.Date <= data.Date)
            {
                idx++;
            }

            return vigenciasOrdenadas[idx];
        }

        private static int? TryGetIntProp(object obj, string propName)
        {
            if (obj == null) return null;
            var p = obj.GetType().GetProperty(propName);
            if (p == null) return null;
            var val = p.GetValue(obj);
            if (val == null) return null;
            if (val is int i) return i;
            if (val is int ni) return ni;
            return null;
        }

        private HorarioDeTrabalhoVigencia CarregueVigenciaPorIdComDias(int organizacaoId, int vigenciaId)
        {
            var vig = _repositorioHorarioVigencia.Obtenha(v => v.OrganizacaoId == organizacaoId && v.Id == vigenciaId);
            if (vig == null) throw new ApplicationException("Vigência inválida.");

            vig.Dias = _repositorioHorarioDia
                .ObtenhaLista(d => d.OrganizacaoId == organizacaoId && d.HorarioDeTrabalhoVigenciaId == vigenciaId)
                .OrderBy(d => d.DiaDaSemana)
                .ToList();

            return vig;
        }

        private Dictionary<int, int> ObtenhaCreditosDiaParaBH(
            PontoDoDia pontoDoDia,
            HorarioDeTrabalhoVigencia vig,
            Dictionary<int, int> resumoAprovadoHePorPercentual)
        {
            var totalPositivos = (int)Math.Round((pontoDoDia.HorasPositivas ?? TimeSpan.Zero).TotalMinutes);
            if (totalPositivos <= 0) totalPositivos = 0;

            resumoAprovadoHePorPercentual ??= new Dictionary<int, int>();

            if (vig.BancoDeHorasSomenteHorasExtrasAprovadas)
            {
                // Só HE aprovadas (por percentual)
                return resumoAprovadoHePorPercentual
                    .Where(k => k.Value > 0)
                    .ToDictionary(k => k.Key, k => k.Value);
            }

            var dict = resumoAprovadoHePorPercentual
                .Where(k => k.Value > 0)
                .ToDictionary(k => k.Key, k => k.Value);

            var somaAprovadas = dict.Values.Sum();
            var resto = Math.Max(0, totalPositivos - somaAprovadas);

            if (resto > 0)
                dict[0] = (dict.TryGetValue(0, out var cur) ? cur : 0) + resto;

            if (dict.Count == 0 && totalPositivos > 0)
                dict[0] = totalPositivos;

            return dict;
        }

        private BancoDeHorasSaldo CalculeBancoDeHorasComPrioridade(
            ref PontoDoDia pontoDoDia,
            HorarioDeTrabalhoVigencia vig,
            BancoDeHorasSaldo saldoAnterior,
            Dictionary<int, int> creditosDia)
        {
            pontoDoDia.BancoDeHorasCredito = null;
            pontoDoDia.BancoDeHorasDebito = null;
            pontoDoDia.BancoDeHorasCreditosJson = null;

            var inicioBh = vig.InicioBancoDeHoras?.Date ?? DateTime.MinValue.Date;
            if (!vig.UtilizaBancoDeHoras || inicioBh > pontoDoDia.Data.Date)
                return new BancoDeHorasSaldo();

            // base
            var creditos = new Dictionary<int, int>(saldoAnterior.CreditosPorPercentual ?? new Dictionary<int, int>());
            var debito = saldoAnterior.DebitoMinutos;

            // debito do dia (horas negativas)
            debito += (int)Math.Round((pontoDoDia.HorasNegativas ?? TimeSpan.Zero).TotalMinutes);

            // ajuste manual
            var ajuste = (int)Math.Round((pontoDoDia.BancoDeHorasAjuste ?? TimeSpan.Zero).TotalMinutes);
            if (ajuste > 0)
                creditos[0] = (creditos.TryGetValue(0, out var c0) ? c0 : 0) + ajuste;
            else if (ajuste < 0)
                debito += Math.Abs(ajuste);

            // soma créditos do dia
            creditosDia ??= new Dictionary<int, int>();
            foreach (var kv in creditosDia)
                creditos[kv.Key] = (creditos.TryGetValue(kv.Key, out var cur) ? cur : 0) + kv.Value;

            // prioridade
            var prioridade = ParsePrioridade(vig.BancoDeHorasPrioridadePercentuais);

            // monta ordem final: prioridade + restantes (bucket 0 sempre por último)
            var restantes = creditos.Keys
                .Where(k => !prioridade.Contains(k) && k != 0)
                .OrderBy(k => k)
                .ToList();

            var ordem = prioridade
                .Where(p => p != 0)
                .Concat(restantes)
                .Concat(creditos.ContainsKey(0) ? new[] { 0 } : Array.Empty<int>())
                .ToList();

            // consome crédito para compensar débito
            foreach (var perc in ordem)
            {
                if (debito <= 0) break;
                if (!creditos.TryGetValue(perc, out var disponivel) || disponivel <= 0) continue;

                var consumir = Math.Min(disponivel, debito);
                creditos[perc] = disponivel - consumir;
                debito -= consumir;
            }

            // limpa zero
            var creditosFinal = creditos.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value);

            var totalCred = creditosFinal.Values.Sum();
            pontoDoDia.BancoDeHorasCredito = TimeSpan.FromMinutes(totalCred);
            pontoDoDia.BancoDeHorasDebito = TimeSpan.FromMinutes(Math.Max(0, debito));
            pontoDoDia.BancoDeHorasCreditosJson = System.Text.Json.JsonSerializer.Serialize(creditosFinal);

            return new BancoDeHorasSaldo
            {
                CreditosPorPercentual = creditosFinal,
                DebitoMinutos = Math.Max(0, debito)
            };
        }

        private sealed class BancoDeHorasSaldo
        {
            public Dictionary<int, int> CreditosPorPercentual { get; set; } = new();
            public int DebitoMinutos { get; set; } = 0;
        }

        public (int Total, List<RegistroForaDaToleranciaDTO> Itens) ObtenhaRegistrosForaDaTolerancia(
            int organizacaoId, int? unidadeId, int? departamentoId, int? horarioId, 
            DateTime dataInicial, DateTime dataFinal, int pagina, int qtd)
        {
            var offset = (pagina - 1) * qtd;

            // Busca registros brutos e os cruza com a escala via lotação/vínculo
            var sql = @"
                SELECT 
                    r.Id as RegistroId,
                    v.Id as VinculoDeTrabalhoId,
                    s.Nome as ServidorNome,
                    u.Nome as UnidadeNome,
                    d.Descricao as DepartamentoNome,
                    r.DataHoraRegistro,
                    r.AprovadoForaTolerancia,
                    r.AcaoAprovacao,
                    r.UsuarioAprovacaoToleranciaNome,
                    hd.Entrada1, hd.Saida1, hd.Entrada2, hd.Saida2, hd.Entrada3, hd.Saida3, hd.Entrada4, hd.Saida4, hd.Entrada5, hd.Saida5,
                    hvg.ToleranciaAntesDaEntradaEmMinutos, hvg.ToleranciaAposAEntradaEmMinutos,
                    hvg.ToleranciaAntesDaSaidaEmMinutos, hvg.ToleranciaAposASaidaEmMinutos
                FROM RegistrosDePonto r
                LEFT JOIN RegistroAplicativo ra ON ra.Id = r.RegistroAplicativoId
                LEFT JOIN LotacaoUnidadeOrganizacional lot ON lot.MatriculaEquipamento = r.UsuarioEquipamentoId 
                    AND lot.OrganizacaoId = r.OrganizacaoId
                    AND CAST(r.DataHoraRegistro AS DATE) >= CAST(lot.Entrada AS DATE)
                    AND (lot.Saida IS NULL OR CAST(r.DataHoraRegistro AS DATE) <= CAST(lot.Saida AS DATE))
                JOIN VinculosDeTrabalho v ON v.Id = COALESCE(ra.VinculoDeTrabalhoId, lot.VinculoDeTrabalhoId)
                JOIN Servidor s ON s.Id = v.ServidorId
                JOIN Departamentos d ON d.Id = v.DepartamentoId
                LEFT JOIN UnidadesOrganizacionais u ON u.Id = lot.UnidadeOrganizacionalId
                JOIN HorarioDeTrabalhoVigencia hvg ON hvg.HorarioDeTrabalhoId = v.HorarioDeTrabalhoId
                    AND hvg.VigenciaInicio <= CAST(r.DataHoraRegistro AS DATE)
                JOIN HorarioDeTrabalhoDia hd ON hd.HorarioDeTrabalhoVigenciaId = hvg.Id
                    AND hd.DiaDaSemana = CASE WHEN (DAYOFWEEK(r.DataHoraRegistro) - 1) = 0 THEN 0 ELSE (DAYOFWEEK(r.DataHoraRegistro) - 1) END
                WHERE r.OrganizacaoId = @OrgId 
                  AND CAST(r.DataHoraRegistro AS DATE) >= @DataIni 
                  AND CAST(r.DataHoraRegistro AS DATE) <= @DataFim
                  AND r.Desconsiderado = 0
                  AND (ra.Id IS NULL OR ra.Manual = 0)
                  -- Filtro para pegar apenas a vigência correta (mais próxima do registro)
                  AND hvg.VigenciaInicio = (
                      SELECT MAX(h2.VigenciaInicio) 
                      FROM HorarioDeTrabalhoVigencia h2 
                      WHERE h2.HorarioDeTrabalhoId = v.HorarioDeTrabalhoId 
                        AND h2.VigenciaInicio <= CAST(r.DataHoraRegistro AS DATE)
                  )";

            if (unidadeId.HasValue) sql += " AND (u.Id = @UnidadeId OR lot.UnidadeOrganizacionalId = @UnidadeId)";
            if (departamentoId.HasValue) sql += " AND v.DepartamentoId = @DepartamentoId";
            if (horarioId.HasValue) sql += " AND v.HorarioDeTrabalhoId = @HorarioId";

            var rows = _repositorio.ConsultaDapper<dynamic>(sql, new { 
                OrgId = organizacaoId, 
                DataIni = dataInicial.Date, 
                DataFim = dataFinal.Date,
                UnidadeId = unidadeId,
                DepartamentoId = departamentoId,
                HorarioId = horarioId
            }).ToList();

            var listaFinal = new List<RegistroForaDaToleranciaDTO>();

            foreach (var r in rows)
            {
                var times = new List<(TimeSpan? Time, string Label, bool isEntrada)>
                {
                    (r.Entrada1, "Entrada 1", true), (r.Saida1, "Saída 1", false),
                    (r.Entrada2, "Entrada 2", true), (r.Saida2, "Saída 2", false),
                    (r.Entrada3, "Entrada 3", true), (r.Saida3, "Saída 3", false),
                    (r.Entrada4, "Entrada 4", true), (r.Saida4, "Saída 4", false),
                    (r.Entrada5, "Entrada 5", true), (r.Saida5, "Saída 5", false)
                }.Where(x => x.Time.HasValue).ToList();

                if (!times.Any()) continue;

                var regTime = ((DateTime)r.DataHoraRegistro).TimeOfDay;
                
                // Encontrar o slot mais próximo
                var closest = times
                    .Select(t => new { t.Time, t.Label, t.isEntrada, Diff = (t.Time.Value - regTime).Duration() })
                    .OrderBy(x => x.Diff)
                    .First();

                double diffMinutos = (closest.Time.Value - regTime).TotalMinutes;
                int tolAntes = closest.isEntrada ? (int)r.ToleranciaAntesDaEntradaEmMinutos : (int)r.ToleranciaAntesDaSaidaEmMinutos;
                int tolDepois = closest.isEntrada ? (int)r.ToleranciaAposAEntradaEmMinutos : (int)r.ToleranciaAposASaidaEmMinutos;

                bool foraTolerancia = false;
                if (diffMinutos > 0 && diffMinutos > tolAntes) foraTolerancia = true; // Antecipado demais (ex: esperado 08:00, bateu 07:45, tolAntes=10 -> diff=15 > 10)
                else if (diffMinutos < 0 && Math.Abs(diffMinutos) > tolDepois) foraTolerancia = true; // Atrasado demais

                if (foraTolerancia || r.AprovadoForaTolerancia != null)
                {
                    listaFinal.Add(new RegistroForaDaToleranciaDTO
                    {
                        RegistroId = r.RegistroId,
                        VinculoDeTrabalhoId = r.VinculoDeTrabalhoId,
                        ServidorNome = r.ServidorNome,
                        UnidadeNome = r.UnidadeNome ?? "N/A",
                        DepartamentoNome = r.DepartamentoNome,
                        DataHoraRegistro = r.DataHoraRegistro,
                        HorarioEsperado = closest.Time.Value,
                        TipoRegistro = closest.Label,
                        MinutosForaTolerancia = (int)Math.Abs(diffMinutos),
                        AprovadoForaTolerancia = r.AprovadoForaTolerancia == null ? (bool?)null : Convert.ToBoolean(r.AprovadoForaTolerancia),
                        AcaoAprovacao = r.AcaoAprovacao,
                        UsuarioAprovacaoNome = r.UsuarioAprovacaoToleranciaNome
                    });
                }
            }

            var totalCount = listaFinal.Count;
            var itens = listaFinal.OrderByDescending(x => x.DataHoraRegistro).Skip(offset).Take(qtd).ToList();

            return (totalCount, itens);
        }


        public void AproveOuDesconsidereLote(int organizacaoId, List<int> registrosIds, string acao, string motivo)
        {
            foreach(var id in registrosIds)
            {
                var r = _repositorioRegistroDePonto.Obtenha(id);
                if(r != null)
                {
                    r.AprovadoForaTolerancia = (acao == "Aprovar");
                    r.AcaoAprovacao = acao;
                    r.MotivoAprovacaoTolerancia = motivo;
                    r.UsuarioAprovacaoToleranciaNome = _usuarioAtual?.Nome ?? "Sistema";
                    r.DataAprovacaoTolerancia = DateTime.Now;

                    if (acao == "Desconsiderar") {
                        r.Desconsiderado = true;
                        r.UsuarioDesconsideracaoNome = _usuarioAtual?.Nome ?? "Sistema";
                        r.MotivoDesconsideracao = motivo;
                        r.DataDesconsideracao = DateTime.Now;
                    }
                    _repositorioRegistroDePonto.Atualizar(r);
                }
            }
            _repositorioRegistroDePonto.Commit();
        }
    }
}