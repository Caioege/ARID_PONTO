using System;
using System.Collections.Generic;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Extensao;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using System.Linq;

namespace AriD.Servicos.Servicos
{
    public class ServicoMonitoramentoRotas : IServicoMonitoramentoRotas
    {
        private const int StatusExecucaoEmAndamento = 1;
        private const int StatusExecucaoPausada = 2;
        private const int StatusExecucaoFinalizada = 3;
        private const int TipoEventoInicioRota = 1;
        private const int TipoEventoOrigem = 2;
        private const int TipoEventoParada = 3;
        private const int TipoEventoDestino = 4;
        private const int TipoEventoFimRota = 5;

        private readonly IRepositorio<Rota> _repositorio;

        public ServicoMonitoramentoRotas(IRepositorio<Rota> repositorio)
        {
            _repositorio = repositorio;
        }

        public MonitoramentoRotasResultadoDTO ObtenhaMonitoramento(int organizacaoId, DateTime dataBase, bool exibirFinalizadas)
        {
            var dataFim = new DateTime(dataBase.Year, dataBase.Month, dataBase.Day, 23, 59, 59);

            var queryExecucoes = @"
                SELECT
                    re.Id as ExecucaoId,
                    re.RotaId,
                    r.Descricao,
                    r.DataParaExecucao,
                    r.NomePaciente,
                    r.MedicoResponsavel,
                    r.Observacao as ObservacaoRota,
                    re.DataHoraInicio,
                    re.DataHoraFim,
                    re.Status,
                    re.MotoristaId,
                    p.Nome as MotoristaNome,
                    r.MotoristaId as MotoristaPrincipalId,
                    r.MotoristaSecundarioId,
                    re.VeiculoId,
                    re.ChecklistExecucaoId,
                    v.Placa,
                    v.Modelo,
                    v.TipoVeiculo,
                    re.PossuiRegistroOffline,
                    re.ExecucaoOfflineCompleta,
                    re.DataHoraUltimaComunicacaoApp,
                    r.UnidadeOrigemId,
                    uo.Nome as NomeUnidadeOrigem,
                    CONCAT_WS(', ', NULLIF(eo.Cep, ''), NULLIF(eo.Logradouro, ''), NULLIF(eo.Numero, ''), NULLIF(eo.Complemento, ''), NULLIF(eo.Bairro, ''), NULLIF(eo.Cidade, ''), ELT(eo.UF + 1, 'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO')) as EnderecoUnidadeOrigem,
                    uo.Latitude as OrigemLatitudeRota,
                    uo.Longitude as OrigemLongitudeRota,
                    r.UnidadeDestinoId,
                    ud.Nome as NomeUnidadeDestino,
                    CONCAT_WS(', ', NULLIF(ed.Cep, ''), NULLIF(ed.Logradouro, ''), NULLIF(ed.Numero, ''), NULLIF(ed.Complemento, ''), NULLIF(ed.Bairro, ''), NULLIF(ed.Cidade, ''), ELT(ed.UF + 1, 'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO')) as EnderecoUnidadeDestino,
                    ud.Latitude as DestinoLatitudeRota,
                    ud.Longitude as DestinoLongitudeRota
                FROM rotaexecucao re
                INNER JOIN rota r ON r.Id = re.RotaId
                INNER JOIN motorista m ON m.Id = re.MotoristaId
                INNER JOIN servidor s ON s.Id = m.ServidorId
                INNER JOIN pessoa p ON p.Id = s.PessoaId
                LEFT JOIN veiculo v ON v.Id = re.VeiculoId
                LEFT JOIN unidadeorganizacional uo ON uo.Id = r.UnidadeOrigemId
                LEFT JOIN endereco eo ON eo.Id = uo.EnderecoId
                LEFT JOIN unidadeorganizacional ud ON ud.Id = r.UnidadeDestinoId
                LEFT JOIN endereco ed ON ed.Id = ud.EnderecoId
                WHERE re.OrganizacaoId = @OrganizacaoId
                  AND re.DataHoraInicio >= @DataBase
                  AND re.DataHoraInicio <= @DataFim";

            var execucoes = _repositorio.ConsultaDapper<RotaExecucaoDapperDTO>(queryExecucoes, new
            {
                OrganizacaoId = organizacaoId,
                DataBase = dataBase.Date,
                DataFim = dataFim
            }).ToList();

            if (!execucoes.Any())
            {
                return new MonitoramentoRotasResultadoDTO
                {
                    Rotas = new List<MonitoramentoRotaDTO>(),
                    ManutencoesVeiculos = ObterManutencoesVeiculosMonitoramento(organizacaoId, new List<MonitoramentoRotaDTO>())
                };
            }

            var execucaoIds = execucoes.Select(e => e.ExecucaoId).Distinct().ToList();
            var veiculoIds = execucoes.Where(e => e.VeiculoId.HasValue).Select(e => e.VeiculoId!.Value).Distinct().ToList();

            var presencasPorExecucao = ObterPresencasPorExecucao(organizacaoId, execucaoIds);
            var eventosPorExecucao = ObterEventosRecentesPorExecucao(organizacaoId, execucaoIds);
            var manutencoesPorVeiculo = ObterManutencoesPorVeiculo(organizacaoId, veiculoIds);

            var queryLocalizacoes = @"
                SELECT
                    RotaExecucaoId as ExecucaoId,
                    DataHoraCaptura as DataHora,
                    Latitude,
                    Longitude,
                    VelocidadeMetrosPorSegundo,
                    RegistradoOffline,
                    DataHoraRegistroLocal,
                    DataHoraSincronizacao,
                    IdentificadorDispositivo,
                    ClientEventId
                FROM rotaexecucaolocalizacao
                WHERE OrganizacaoId = @OrganizacaoId
                  AND RotaExecucaoId IN @ExecucaoIds
                  AND DataHoraCaptura >= @DataBase
                  AND DataHoraCaptura <= @DataFim
                ORDER BY DataHora ASC";

            var localizacoesAll = _repositorio.ConsultaDapper<LocalizacaoDapperDTO>(queryLocalizacoes, new
            {
                OrganizacaoId = organizacaoId,
                ExecucaoIds = execucaoIds,
                DataBase = dataBase.Date,
                DataFim = dataFim
            }).ToList();

            var resultado = new List<MonitoramentoRotaDTO>();

            foreach (var exec in execucoes)
            {
                var locais = localizacoesAll
                    .Where(l => l.ExecucaoId == exec.ExecucaoId)
                    .ToList();

                var historico = locais.Select(l => new[] { ParseCoord(l.Latitude), ParseCoord(l.Longitude) }).ToList();
                var historicoDetalhado = locais.Select(l => new MonitoramentoLocalizacaoDTO
                {
                    Latitude = ParseCoord(l.Latitude),
                    Longitude = ParseCoord(l.Longitude),
                    DataHora = l.DataHora.ToString("dd/MM/yyyy HH:mm:ss"),
                    RegistradoOffline = l.RegistradoOffline,
                    DataHoraRegistroLocal = l.DataHoraRegistroLocal?.ToString("dd/MM/yyyy HH:mm:ss"),
                    DataHoraSincronizacao = l.DataHoraSincronizacao?.ToString("dd/MM/yyyy HH:mm:ss"),
                    IdentificadorDispositivo = l.IdentificadorDispositivo,
                    ClientEventId = l.ClientEventId
                }).ToList();
                var velocidadeMediaKmH = CalcularVelocidadeMediaKmH(locais);

                double[]? ultimaPos = null;
                if (locais.Any())
                {
                    var ultima = locais.Last();
                    ultimaPos = new[] { ParseCoord(ultima.Latitude), ParseCoord(ultima.Longitude) };
                }

                var queryParadasExec = @"
                    SELECT
                        p.Endereco as nome,
                        p.Link as link,
                        COALESCE(ev.Latitude, p.Latitude) as Latitude,
                        COALESCE(ev.Longitude, p.Longitude) as Longitude,
                        ev.Entregue,
                        ev.DataHoraEvento as ConcluidoEm,
                        ev.RegistradoOffline,
                        ev.DataHoraRegistroLocal,
                        ev.DataHoraSincronizacao,
                        ev.IdentificadorDispositivo,
                        ev.ClientEventId
                    FROM paradarota p
                    LEFT JOIN rotaexecucaoevento ev ON ev.Id = (
                        SELECT ev2.Id
                        FROM rotaexecucaoevento ev2
                        WHERE ev2.RotaExecucaoId = @ExecId
                          AND ev2.TipoEvento = 3
                          AND ev2.ParadaRotaId = p.Id
                        ORDER BY ev2.DataHoraEvento DESC, ev2.Id DESC
                        LIMIT 1
                    )
                    WHERE p.RotaId = @RotaId
                    ORDER BY p.Ordem ASC, p.Id ASC";

                var paradasRaw = _repositorio.ConsultaDapper<ParadaMonitoramentoRowDTO>(queryParadasExec, new
                {
                    ExecId = exec.ExecucaoId,
                    RotaId = exec.RotaId
                }).ToList();

                var paradas = paradasRaw
                    .Where(p => !string.IsNullOrEmpty(p.Latitude) && !string.IsNullOrEmpty(p.Longitude))
                    .Select(p => new MonitoramentoParadaDTO
                    {
                        Nome = p.Nome,
                        Link = p.Link,
                        Latitude = ParseCoord(p.Latitude),
                        Longitude = ParseCoord(p.Longitude),
                        Entregue = p.Entregue ?? false,
                        ConcluidoEm = p.ConcluidoEm != null ? ((DateTime)p.ConcluidoEm).ToString("dd/MM/yyyy HH:mm:ss") : null,
                        RegistradoOffline = p.RegistradoOffline,
                        DataHoraRegistroLocal = p.DataHoraRegistroLocal?.ToString("dd/MM/yyyy HH:mm:ss"),
                        DataHoraSincronizacao = p.DataHoraSincronizacao?.ToString("dd/MM/yyyy HH:mm:ss"),
                        IdentificadorDispositivo = p.IdentificadorDispositivo,
                        ClientEventId = p.ClientEventId
                    }).ToList();

                if (ultimaPos == null && paradas.Any())
                {
                    ultimaPos = new[] { paradas.First().Latitude, paradas.First().Longitude };
                }

                var sofreuDesvio = _repositorio.ConsultaDapper<int>(
                    "SELECT COUNT(1) FROM rotaexecucaodesvio WHERE RotaExecucaoId = @ExecId",
                    new { ExecId = exec.ExecucaoId }).FirstOrDefault() > 0;

                var queryPausas = @"
                    SELECT Motivo, DataHoraInicio, DataHoraFim, LatitudeInicio, LongitudeInicio, LatitudeFim, LongitudeFim,
                           RegistradoOffline, DataHoraRegistroLocal, DataHoraSincronizacao, IdentificadorDispositivo, ClientEventId
                    FROM rotaexecucaopausa
                    WHERE RotaExecucaoId = @ExecId
                    ORDER BY DataHoraInicio";

                var pausas = _repositorio.ConsultaDapper<PausaExecucaoRowDTO>(queryPausas, new { ExecId = exec.ExecucaoId })
                    .Select(p => new MonitoramentoPausaDTO
                    {
                        Motivo = p.Motivo,
                        DataHoraInicio = p.DataHoraInicio,
                        DataHoraFim = p.DataHoraFim,
                        LatInicio = TryParseNullableCoord(p.LatitudeInicio),
                        LngInicio = TryParseNullableCoord(p.LongitudeInicio),
                        LatFim = TryParseNullableCoord(p.LatitudeFim),
                        LngFim = TryParseNullableCoord(p.LongitudeFim),
                        RegistradoOffline = p.RegistradoOffline,
                        DataHoraRegistroLocal = p.DataHoraRegistroLocal?.ToString("dd/MM/yyyy HH:mm:ss"),
                        DataHoraSincronizacao = p.DataHoraSincronizacao?.ToString("dd/MM/yyyy HH:mm:ss"),
                        IdentificadorDispositivo = p.IdentificadorDispositivo,
                        ClientEventId = p.ClientEventId
                    }).ToList();

                var referenciaComunicacao = exec.DataHoraUltimaComunicacaoApp ?? (locais.Any() ? locais.Last().DataHora : exec.DataHoraInicio);
                var minutosSemComunicacao = (int)Math.Floor((DateTime.Now - referenciaComunicacao).TotalMinutes);
                var possivelmenteOffline = exec.Status is StatusExecucaoEmAndamento or StatusExecucaoPausada && minutosSemComunicacao >= 5;
                presencasPorExecucao.TryGetValue(exec.ExecucaoId, out var presencasExecucao);
                presencasExecucao ??= new MonitoramentoPresencasExecucaoDTO();
                var eventosRecentes = eventosPorExecucao.ContainsKey(exec.ExecucaoId) ? eventosPorExecucao[exec.ExecucaoId] : new List<MonitoramentoEventoDTO>();
                MonitoramentoManutencaoDTO? proximaManutencao = null;
                if (exec.VeiculoId.HasValue && manutencoesPorVeiculo.ContainsKey(exec.VeiculoId.Value))
                    proximaManutencao = manutencoesPorVeiculo[exec.VeiculoId.Value];

                var rotaMonitorada = new MonitoramentoRotaDTO
                {
                    ExecucaoId = exec.ExecucaoId,
                    RotaId = exec.RotaId,
                    Descricao = exec.Descricao,
                    DataParaExecucao = exec.DataParaExecucao != null ? exec.DataParaExecucao.Value.ToString("dd/MM/yyyy") : "",
                    NomePaciente = exec.NomePaciente,
                    MedicoResponsavel = exec.MedicoResponsavel,
                    ObservacaoRota = exec.ObservacaoRota,
                    HoraInicio = exec.DataHoraInicio.ToString("dd/MM/yyyy HH:mm:ss"),
                    HoraFim = exec.DataHoraFim?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Em Execução",
                    MotoristaId = exec.MotoristaId,
                    MotoristaNome = exec.MotoristaNome,
                    MotoristaPapel = ObterPapelMotorista(exec),
                    VeiculoId = exec.VeiculoId,
                    ChecklistExecucaoId = exec.ChecklistExecucaoId,
                    PlacaModelo = $"{exec.Placa} - {exec.Modelo}",
                    TipoVeiculo = exec.TipoVeiculo,
                    VelocidadeMediaKmH = velocidadeMediaKmH,
                    UltimaLocalizacao = ultimaPos,
                    HistoricoLocalizacoes = historico,
                    HistoricoLocalizacoesDetalhado = historicoDetalhado,
                    UltimaAtualizacao = locais.Any() ? locais.Last().DataHora.ToString("dd/MM/yyyy HH:mm:ss") : exec.DataHoraInicio.ToString("dd/MM/yyyy HH:mm:ss"),
                    Paradas = paradas,
                    Pausas = pausas,
                    Finalizada = exec.DataHoraFim.HasValue,
                    SujeitoADesvio = sofreuDesvio,
                    PossuiRegistroOffline = exec.PossuiRegistroOffline,
                    ExecucaoOfflineCompleta = exec.ExecucaoOfflineCompleta,
                    ClassificacaoOffline = ObterClassificacaoOffline(exec.PossuiRegistroOffline, exec.ExecucaoOfflineCompleta),
                    PossivelmenteOffline = possivelmenteOffline,
                    MinutosSemComunicacao = minutosSemComunicacao < 0 ? 0 : minutosSemComunicacao,
                    UltimaComunicacaoApp = referenciaComunicacao.ToString("dd/MM/yyyy HH:mm:ss"),
                    StatusExecucao = exec.Status,
                    StatusDescricao = ObterStatusDescricao(exec.Status, pausas),
                    Pacientes = presencasExecucao.Pacientes,
                    Acompanhantes = presencasExecucao.Acompanhantes,
                    Profissionais = presencasExecucao.Profissionais,
                    EventosRecentes = eventosRecentes,
                    ProximaManutencao = proximaManutencao,
                    UnidadeOrigem = MontarUnidadeRota(exec.UnidadeOrigemId, exec.NomeUnidadeOrigem, exec.EnderecoUnidadeOrigem, exec.OrigemLatitudeRota, exec.OrigemLongitudeRota),
                    UnidadeDestino = MontarUnidadeRota(exec.UnidadeDestinoId, exec.NomeUnidadeDestino, exec.EnderecoUnidadeDestino, exec.DestinoLatitudeRota, exec.DestinoLongitudeRota)
                };

                rotaMonitorada.Alertas = MontarAlertas(rotaMonitorada);
                resultado.Add(rotaMonitorada);
            }

            return new MonitoramentoRotasResultadoDTO
            {
                Rotas = resultado,
                ManutencoesVeiculos = ObterManutencoesVeiculosMonitoramento(organizacaoId, resultado)
            };
        }

        public ChecklistExecucaoRotaDTO? ObtenhaChecklistExecucao(int organizacaoId, int execucaoId)
        {
            var sqlCabecalho = @"
                SELECT
                    ce.Id as ChecklistExecucaoId,
                    re.Id as ExecucaoId,
                    r.Id as RotaId,
                    r.Descricao as RotaDescricao,
                    ce.VeiculoId,
                    CONCAT_WS(' - ', NULLIF(v.Placa, ''), NULLIF(v.Modelo, '')) as VeiculoDescricao,
                    ce.MotoristaId,
                    p.Nome as MotoristaNome,
                    ce.DataHora
                FROM rotaexecucao re
                INNER JOIN checklistexecucao ce ON ce.Id = re.ChecklistExecucaoId
                INNER JOIN rota r ON r.Id = re.RotaId
                INNER JOIN veiculo v ON v.Id = ce.VeiculoId
                INNER JOIN motorista m ON m.Id = ce.MotoristaId
                INNER JOIN servidor s ON s.Id = m.ServidorId
                INNER JOIN pessoa p ON p.Id = s.PessoaId
                WHERE re.OrganizacaoId = @OrganizacaoId
                  AND re.Id = @ExecucaoId
                  AND re.ChecklistExecucaoId IS NOT NULL
                LIMIT 1";

            var cabecalho = _repositorio.ConsultaDapper<ChecklistExecucaoRotaRowDTO>(sqlCabecalho, new
            {
                OrganizacaoId = organizacaoId,
                ExecucaoId = execucaoId
            }).FirstOrDefault();

            if (cabecalho == null) return null;

            var sqlItens = @"
                SELECT
                    ci.Id as ChecklistItemId,
                    ci.Descricao,
                    COALESCE(cei.Marcado, 0) as Marcado
                FROM checklistitem ci
                LEFT JOIN checklistexecucaoitem cei ON cei.ChecklistItemId = ci.Id
                    AND cei.ChecklistExecucaoId = @ChecklistExecucaoId
                WHERE ci.OrganizacaoId = @OrganizacaoId
                  AND ci.VeiculoId = @VeiculoId
                  AND ci.Ativo = 1
                ORDER BY ci.Descricao";

            var itens = _repositorio.ConsultaDapper<ChecklistExecucaoRotaItemDTO>(sqlItens, new
            {
                OrganizacaoId = organizacaoId,
                cabecalho.ChecklistExecucaoId,
                cabecalho.VeiculoId
            }).ToList();

            return new ChecklistExecucaoRotaDTO
            {
                ChecklistExecucaoId = cabecalho.ChecklistExecucaoId,
                ExecucaoId = cabecalho.ExecucaoId,
                RotaId = cabecalho.RotaId,
                RotaDescricao = cabecalho.RotaDescricao,
                VeiculoId = cabecalho.VeiculoId,
                VeiculoDescricao = cabecalho.VeiculoDescricao,
                MotoristaId = cabecalho.MotoristaId,
                MotoristaNome = cabecalho.MotoristaNome,
                DataHora = cabecalho.DataHora.ToString("dd/MM/yyyy HH:mm:ss"),
                TotalItens = itens.Count,
                TotalMarcados = itens.Count(i => i.Marcado),
                Itens = itens
            };
        }

        private Dictionary<int, MonitoramentoPresencasExecucaoDTO> ObterPresencasPorExecucao(int organizacaoId, List<int> execucaoIds)
        {
            if (!execucaoIds.Any()) return new Dictionary<int, MonitoramentoPresencasExecucaoDTO>();

            var sql = @"
                SELECT RotaExecucaoId as ExecucaoId, TipoParticipante, COALESCE(PacienteId, ServidorId, 0) as Id,
                       Nome, Presente, CriadoPeloApp
                FROM rotaexecucaopresenca
                WHERE OrganizacaoId = @OrganizacaoId
                  AND RotaExecucaoId IN @ExecucaoIds
                ORDER BY TipoParticipante, Nome";

            var resultado = new Dictionary<int, MonitoramentoPresencasExecucaoDTO>();

            foreach (var presenca in _repositorio.ConsultaDapper<PresencaMonitoramentoRowDTO>(sql, new { OrganizacaoId = organizacaoId, ExecucaoIds = execucaoIds }))
            {
                if (!resultado.ContainsKey(presenca.ExecucaoId))
                    resultado[presenca.ExecucaoId] = new MonitoramentoPresencasExecucaoDTO();

                var pessoa = new MonitoramentoPessoaDTO
                {
                    Id = presenca.Id,
                    Nome = string.IsNullOrWhiteSpace(presenca.Nome) ? "Não informado" : presenca.Nome,
                    Complemento = presenca.Presente ? "Presente" : "Ausente"
                };

                if (presenca.TipoParticipante == 1)
                    resultado[presenca.ExecucaoId].Pacientes.Add(pessoa);
                else if (presenca.TipoParticipante == 2)
                    resultado[presenca.ExecucaoId].Acompanhantes.Add(pessoa);
                else if (presenca.TipoParticipante == 3)
                    resultado[presenca.ExecucaoId].Profissionais.Add(pessoa);
            }

            return resultado;
        }

        private Dictionary<int, List<MonitoramentoEventoDTO>> ObterEventosRecentesPorExecucao(int organizacaoId, List<int> execucaoIds)
        {
            if (!execucaoIds.Any()) return new Dictionary<int, List<MonitoramentoEventoDTO>>();

            var sql = @"
                SELECT RotaExecucaoId as ExecucaoId, TipoEvento, Observacao, DataHoraEvento, RegistradoOffline, Latitude, Longitude
                FROM rotaexecucaoevento
                WHERE OrganizacaoId = @OrganizacaoId
                  AND RotaExecucaoId IN @ExecucaoIds
                ORDER BY DataHoraEvento DESC, Id DESC";

            return _repositorio.ConsultaDapper<EventoMonitoramentoRowDTO>(sql, new { OrganizacaoId = organizacaoId, ExecucaoIds = execucaoIds })
                .GroupBy(e => e.ExecucaoId)
                .ToDictionary(g => g.Key, g => g.Take(5).Select(e => new MonitoramentoEventoDTO
                {
                    TipoEvento = e.TipoEvento,
                    TipoDescricao = ObterTipoEventoDescricao(e.TipoEvento),
                    DataHora = e.DataHoraEvento.ToString("dd/MM/yyyy HH:mm:ss"),
                    Observacao = e.Observacao,
                    RegistradoOffline = e.RegistradoOffline,
                    Latitude = TryParseNullableCoord(e.Latitude),
                    Longitude = TryParseNullableCoord(e.Longitude)
                }).ToList());
        }

        private Dictionary<int, MonitoramentoManutencaoDTO> ObterManutencoesPorVeiculo(int organizacaoId, List<int> veiculoIds)
        {
            if (!veiculoIds.Any()) return new Dictionary<int, MonitoramentoManutencaoDTO>();

            var sql = @"
                SELECT mv.VeiculoId, mv.Descricao, mv.Categoria, mv.KmProximaManutencao, mv.DataManutencao, mv.DataVencimentoManutencao,
                       mv.DataAgendamento, mv.DataConclusao, mv.GarantiaAte, mv.LocalExecucaoServico, mv.Fornecedor,
                       mv.ResponsavelServico, mv.ContatoFornecedor, mv.NumeroDocumento, mv.CustoPrevisto,
                       mv.ValorMaximoAutorizado, mv.ValorTotalGasto, mv.Observacao, mv.Situacao, v.QuilometragemAtual
                FROM manutencaoveiculo mv
                INNER JOIN veiculo v ON v.Id = mv.VeiculoId
                WHERE mv.OrganizacaoId = @OrganizacaoId
                  AND mv.VeiculoId IN @VeiculoIds
                  AND mv.Situacao <> @SituacaoExecutada
                ORDER BY COALESCE(mv.DataVencimentoManutencao, mv.DataManutencao, '2999-12-31'), COALESCE(mv.KmProximaManutencao, 2147483647)";

            var hoje = DateTime.Now.Date;
            return _repositorio.ConsultaDapper<ManutencaoMonitoramentoRowDTO>(sql, new
                {
                    OrganizacaoId = organizacaoId,
                    VeiculoIds = veiculoIds,
                    SituacaoExecutada = (int)eSituacaoManutencao.Executada
                })
                .GroupBy(m => m.VeiculoId)
                .ToDictionary(g => g.Key, g =>
                {
                    var m = g.First();
                    var dataReferencia = m.DataVencimentoManutencao ?? m.DataManutencao;
                    var vencidaPorData = dataReferencia.HasValue && dataReferencia.Value.Date < hoje;
                    var vencidaPorKm = m.KmProximaManutencao.HasValue && m.QuilometragemAtual >= m.KmProximaManutencao.Value;
                    var proximaPorData = dataReferencia.HasValue && dataReferencia.Value.Date >= hoje && dataReferencia.Value.Date <= hoje.AddDays(30);
                    var proximaPorKm = m.KmProximaManutencao.HasValue && m.KmProximaManutencao.Value - m.QuilometragemAtual <= 500;

                    return new MonitoramentoManutencaoDTO
                    {
                        Descricao = m.Descricao,
                        Categoria = m.Categoria,
                        DataVencimento = dataReferencia?.ToString("dd/MM/yyyy"),
                        DataManutencao = m.DataManutencao?.ToString("dd/MM/yyyy"),
                        DataAgendamento = m.DataAgendamento?.ToString("dd/MM/yyyy"),
                        DataConclusao = m.DataConclusao?.ToString("dd/MM/yyyy"),
                        GarantiaAte = m.GarantiaAte?.ToString("dd/MM/yyyy"),
                        KmProximaManutencao = m.KmProximaManutencao,
                        QuilometragemAtual = m.QuilometragemAtual,
                        LocalExecucaoServico = m.LocalExecucaoServico,
                        Fornecedor = m.Fornecedor,
                        ResponsavelServico = m.ResponsavelServico,
                        ContatoFornecedor = m.ContatoFornecedor,
                        NumeroDocumento = m.NumeroDocumento,
                        CustoPrevisto = m.CustoPrevisto,
                        ValorMaximoAutorizado = m.ValorMaximoAutorizado,
                        ValorTotalGasto = m.ValorTotalGasto,
                        Observacao = m.Observacao,
                        Vencida = vencidaPorData || vencidaPorKm,
                        Proxima = proximaPorData || proximaPorKm,
                        SituacaoDescricao = ((eSituacaoManutencao)m.Situacao).DescricaoDoEnumerador()
                    };
                });
        }

        private List<MonitoramentoManutencaoVeiculoDTO> ObterManutencoesVeiculosMonitoramento(int organizacaoId, List<MonitoramentoRotaDTO> rotas)
        {
            var sql = @"
                SELECT mv.Id as ManutencaoId, mv.VeiculoId, v.Placa, v.Modelo, mv.Descricao, mv.Categoria, mv.KmProximaManutencao,
                       mv.DataManutencao, mv.DataVencimentoManutencao, mv.DataAgendamento, mv.DataConclusao, mv.GarantiaAte,
                       mv.LocalExecucaoServico, mv.Fornecedor, mv.ResponsavelServico, mv.ContatoFornecedor, mv.NumeroDocumento,
                       mv.CustoPrevisto, mv.ValorMaximoAutorizado, mv.ValorTotalGasto, mv.Observacao, mv.Situacao, v.QuilometragemAtual
                FROM manutencaoveiculo mv
                INNER JOIN veiculo v ON v.Id = mv.VeiculoId
                WHERE mv.OrganizacaoId = @OrganizacaoId
                  AND mv.Situacao <> @SituacaoExecutada
                ORDER BY COALESCE(mv.DataVencimentoManutencao, mv.DataManutencao, '2999-12-31'), COALESCE(mv.KmProximaManutencao, 2147483647)";

            var hoje = DateTime.Now.Date;
            var limite = hoje.AddDays(30);
            var rotasPorVeiculo = rotas
                .Where(r => r.VeiculoId.HasValue)
                .GroupBy(r => r.VeiculoId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(r => new MonitoramentoRotaVinculadaDTO
                {
                    RotaId = r.RotaId,
                    ExecucaoId = r.ExecucaoId,
                    Descricao = r.Descricao,
                    MotoristaNome = r.MotoristaNome,
                    StatusDescricao = r.StatusDescricao,
                    DataParaExecucao = r.DataParaExecucao,
                    Finalizada = r.Finalizada
                }).ToList());

            return _repositorio.ConsultaDapper<ManutencaoMonitoramentoRowDTO>(sql, new
                {
                    OrganizacaoId = organizacaoId,
                    SituacaoExecutada = (int)eSituacaoManutencao.Executada
                })
                .Select(m =>
                {
                    var dataReferencia = m.DataVencimentoManutencao ?? m.DataManutencao;
                    var vencidaPorData = dataReferencia.HasValue && dataReferencia.Value.Date < hoje;
                    var vencidaPorKm = m.KmProximaManutencao.HasValue && m.QuilometragemAtual >= m.KmProximaManutencao.Value;
                    var proximaPorData = dataReferencia.HasValue && dataReferencia.Value.Date >= hoje && dataReferencia.Value.Date <= limite;
                    var proximaPorKm = m.KmProximaManutencao.HasValue && m.KmProximaManutencao.Value - m.QuilometragemAtual <= 500;
                    var vencida = vencidaPorData || vencidaPorKm;
                    var proxima = proximaPorData || proximaPorKm;

                    if (!vencida && !proxima) return null;

                    rotasPorVeiculo.TryGetValue(m.VeiculoId, out var rotasVinculadas);

                    return new MonitoramentoManutencaoVeiculoDTO
                    {
                        ManutencaoId = m.ManutencaoId,
                        VeiculoId = m.VeiculoId,
                        Placa = m.Placa,
                        Modelo = m.Modelo,
                        VeiculoDescricao = $"{m.Placa} - {m.Modelo}",
                        Descricao = m.Descricao,
                        Categoria = m.Categoria,
                        DataVencimento = dataReferencia?.ToString("dd/MM/yyyy"),
                        DataManutencao = m.DataManutencao?.ToString("dd/MM/yyyy"),
                        DataAgendamento = m.DataAgendamento?.ToString("dd/MM/yyyy"),
                        DataConclusao = m.DataConclusao?.ToString("dd/MM/yyyy"),
                        GarantiaAte = m.GarantiaAte?.ToString("dd/MM/yyyy"),
                        KmProximaManutencao = m.KmProximaManutencao,
                        QuilometragemAtual = m.QuilometragemAtual,
                        LocalExecucaoServico = m.LocalExecucaoServico,
                        Fornecedor = m.Fornecedor,
                        ResponsavelServico = m.ResponsavelServico,
                        ContatoFornecedor = m.ContatoFornecedor,
                        NumeroDocumento = m.NumeroDocumento,
                        CustoPrevisto = m.CustoPrevisto,
                        ValorMaximoAutorizado = m.ValorMaximoAutorizado,
                        ValorTotalGasto = m.ValorTotalGasto,
                        Observacao = m.Observacao,
                        Vencida = vencida,
                        Proxima = proxima,
                        SituacaoDescricao = ((eSituacaoManutencao)m.Situacao).DescricaoDoEnumerador(),
                        RotasVinculadas = rotasVinculadas ?? new List<MonitoramentoRotaVinculadaDTO>()
                    };
                })
                .Where(m => m != null)
                .Select(m => m!)
                .OrderByDescending(m => m.Vencida)
                .ThenBy(m => ExtrairDataManutencaoOrdenacao(m.DataVencimento))
                .ThenBy(m => m.VeiculoDescricao)
                .ToList();
        }

        private DateTime ExtrairDataManutencaoOrdenacao(string? dataTexto)
        {
            if (DateTime.TryParse(dataTexto, out var data)) return data.Date;
            return DateTime.MaxValue.Date;
        }

        private List<MonitoramentoAlertaDTO> MontarAlertas(MonitoramentoRotaDTO rota)
        {
            var alertas = new List<MonitoramentoAlertaDTO>();

            if (rota.SujeitoADesvio)
                alertas.Add(new MonitoramentoAlertaDTO { Tipo = "desvio", Severidade = "critico", Titulo = "Ocorrência no percurso", Mensagem = "Desvio detectado durante a execução da rota." });

            if (rota.PossivelmenteOffline)
                alertas.Add(new MonitoramentoAlertaDTO { Tipo = "offline", Severidade = "alto", Titulo = "Sem sinal GPS", Mensagem = $"Sem comunicação há {rota.MinutosSemComunicacao ?? 0} min." });

            if (rota.ProximaManutencao?.Vencida == true)
                alertas.Add(new MonitoramentoAlertaDTO { Tipo = "manutencao", Severidade = "alto", Titulo = "Manutenção vencida", Mensagem = rota.ProximaManutencao.Descricao });
            else if (rota.ProximaManutencao?.Proxima == true)
                alertas.Add(new MonitoramentoAlertaDTO { Tipo = "manutencao", Severidade = "medio", Titulo = "Manutenção próxima", Mensagem = rota.ProximaManutencao.Descricao });

            if (rota.PossuiRegistroOffline)
                alertas.Add(new MonitoramentoAlertaDTO { Tipo = "sync", Severidade = "medio", Titulo = "Registros offline", Mensagem = rota.ClassificacaoOffline ?? "A rota possui registros sincronizados offline." });

            if (rota.StatusExecucao == StatusExecucaoPausada)
                alertas.Add(new MonitoramentoAlertaDTO { Tipo = "pausa", Severidade = "medio", Titulo = "Rota em almoço/pausa", Mensagem = "A execução está pausada no momento." });

            if (rota.Finalizada)
                alertas.Add(new MonitoramentoAlertaDTO { Tipo = "finalizada", Severidade = "baixo", Titulo = "Rota finalizada", Mensagem = $"Finalizada em {rota.HoraFim}." });

            return alertas;
        }

        private string ObterStatusDescricao(int status, List<MonitoramentoPausaDTO> pausas)
        {
            if (status == StatusExecucaoFinalizada) return "Finalizada";
            if (status == StatusExecucaoPausada) return "Em almoço";
            if (pausas.Any(p => !p.DataHoraFim.HasValue)) return "Em almoço";
            if (status == StatusExecucaoEmAndamento) return "Em rota";
            return "Não informado";
        }

        private string ObterPapelMotorista(RotaExecucaoDapperDTO exec)
        {
            if (exec.MotoristaId == exec.MotoristaPrincipalId) return "Principal";
            if (exec.MotoristaSecundarioId.HasValue && exec.MotoristaId == exec.MotoristaSecundarioId.Value) return "Secundário";
            return "Motorista";
        }

        private MonitoramentoUnidadeRotaDTO? MontarUnidadeRota(int? id, string? nome, string? endereco, string? latitude, string? longitude)
        {
            if (!id.HasValue && string.IsNullOrWhiteSpace(nome)) return null;

            return new MonitoramentoUnidadeRotaDTO
            {
                Id = id,
                Nome = nome,
                Endereco = endereco,
                Latitude = TryParseNullableCoord(latitude),
                Longitude = TryParseNullableCoord(longitude)
            };
        }

        private string ObterTipoEventoDescricao(int tipoEvento)
        {
            return tipoEvento switch
            {
                TipoEventoInicioRota => "Rota iniciada",
                TipoEventoOrigem => "Origem confirmada",
                TipoEventoParada => "Parada confirmada",
                TipoEventoDestino => "Chegada ao destino",
                TipoEventoFimRota => "Rota finalizada",
                _ => "Evento"
            };
        }

        private double ParseCoord(object valor)
        {
            if (valor == null) return 0;
            var s = valor.ToString()?.Replace("\"", "").Replace(" ", "").Replace(",", ".");
            if (string.IsNullOrEmpty(s)) return 0;
            return double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        }

        private double? TryParseNullableCoord(object valor)
        {
            if (valor == null) return null;
            var s = valor.ToString()?.Replace("\"", "").Replace(" ", "").Replace(",", ".");
            if (string.IsNullOrEmpty(s)) return null;
            return double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private double? CalcularVelocidadeMediaKmH(List<LocalizacaoDapperDTO> locais)
        {
            var velocidades = locais
                .Where(l => l.VelocidadeMetrosPorSegundo.HasValue && l.VelocidadeMetrosPorSegundo.Value > 0)
                .Select(l => l.VelocidadeMetrosPorSegundo.Value)
                .ToList();

            if (!velocidades.Any())
                return null;

            return velocidades.Average() * 3.6;
        }

        private string ObterClassificacaoOffline(bool possuiRegistroOffline, bool execucaoOfflineCompleta)
        {
            if (!possuiRegistroOffline) return "";
            return execucaoOfflineCompleta
                ? "Rota executada completamente offline"
                : "Rota executada parcialmente offline";
        }
    }

    internal class MonitoramentoPresencasExecucaoDTO
    {
        public List<MonitoramentoPessoaDTO> Pacientes { get; set; } = new List<MonitoramentoPessoaDTO>();
        public List<MonitoramentoPessoaDTO> Acompanhantes { get; set; } = new List<MonitoramentoPessoaDTO>();
        public List<MonitoramentoPessoaDTO> Profissionais { get; set; } = new List<MonitoramentoPessoaDTO>();
    }

    internal class PresencaMonitoramentoRowDTO
    {
        public int ExecucaoId { get; set; }
        public int TipoParticipante { get; set; }
        public int Id { get; set; }
        public string? Nome { get; set; }
        public bool Presente { get; set; }
        public bool CriadoPeloApp { get; set; }
    }

    internal class EventoMonitoramentoRowDTO
    {
        public int ExecucaoId { get; set; }
        public int TipoEvento { get; set; }
        public string? Observacao { get; set; }
        public DateTime DataHoraEvento { get; set; }
        public bool RegistradoOffline { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }

    internal class ManutencaoMonitoramentoRowDTO
    {
        public int ManutencaoId { get; set; }
        public int VeiculoId { get; set; }
        public string? Placa { get; set; }
        public string? Modelo { get; set; }
        public string Descricao { get; set; }
        public string? Categoria { get; set; }
        public int? KmProximaManutencao { get; set; }
        public DateTime? DataManutencao { get; set; }
        public DateTime? DataVencimentoManutencao { get; set; }
        public DateTime? DataAgendamento { get; set; }
        public DateTime? DataConclusao { get; set; }
        public DateTime? GarantiaAte { get; set; }
        public string? LocalExecucaoServico { get; set; }
        public string? Fornecedor { get; set; }
        public string? ResponsavelServico { get; set; }
        public string? ContatoFornecedor { get; set; }
        public string? NumeroDocumento { get; set; }
        public decimal? CustoPrevisto { get; set; }
        public decimal? ValorMaximoAutorizado { get; set; }
        public decimal? ValorTotalGasto { get; set; }
        public string? Observacao { get; set; }
        public int Situacao { get; set; }
        public int QuilometragemAtual { get; set; }
    }
}
