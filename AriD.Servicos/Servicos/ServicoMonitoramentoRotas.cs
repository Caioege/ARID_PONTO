using System;
using System.Collections.Generic;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using System.Linq;

namespace AriD.Servicos.Servicos
{
    public class ServicoMonitoramentoRotas : IServicoMonitoramentoRotas
    {
        private readonly IRepositorio<Rota> _repositorio;

        public ServicoMonitoramentoRotas(IRepositorio<Rota> repositorio)
        {
            _repositorio = repositorio;
        }

        public IEnumerable<MonitoramentoRotaDTO> ObtenhaMonitoramento(int organizacaoId, DateTime dataBase, bool exibirFinalizadas)
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
                    re.DataHoraInicio,
                    re.DataHoraFim,
                    re.Status,
                    re.MotoristaId,
                    p.Nome as MotoristaNome,
                    re.VeiculoId,
                    v.Placa,
                    v.Modelo,
                    v.TipoVeiculo
                FROM rotaexecucao re
                INNER JOIN rota r ON r.Id = re.RotaId
                INNER JOIN motorista m ON m.Id = re.MotoristaId
                INNER JOIN servidor s ON s.Id = m.ServidorId
                INNER JOIN pessoa p ON p.Id = s.PessoaId
                LEFT JOIN veiculo v ON v.Id = re.VeiculoId
                WHERE re.OrganizacaoId = @OrganizacaoId
                  AND re.DataHoraInicio >= @DataBase
                  AND re.DataHoraInicio <= @DataFim";

            if (!exibirFinalizadas)
            {
                queryExecucoes += " AND re.Status IN (1, 2) ";
            }

            var execucoes = _repositorio.ConsultaDapper<RotaExecucaoDapperDTO>(queryExecucoes, new
            {
                OrganizacaoId = organizacaoId,
                DataBase = dataBase.Date,
                DataFim = dataFim
            }).ToList();

            if (!execucoes.Any()) return new List<MonitoramentoRotaDTO>();

            var execucaoIds = execucoes.Select(e => e.ExecucaoId).Distinct().ToList();

            var queryLocalizacoes = @"
                SELECT
                    RotaExecucaoId as ExecucaoId,
                    DataHoraCaptura as DataHora,
                    Latitude,
                    Longitude
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
                        ev.DataHoraEvento as ConcluidoEm
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
                        ConcluidoEm = p.ConcluidoEm != null ? ((DateTime)p.ConcluidoEm).ToString("dd/MM/yy HH:mm") : null
                    }).ToList();

                var sofreuDesvio = _repositorio.ConsultaDapper<int>(
                    "SELECT COUNT(1) FROM rotaexecucaodesvio WHERE RotaExecucaoId = @ExecId",
                    new { ExecId = exec.ExecucaoId }).FirstOrDefault() > 0;

                var queryPausas = @"
                    SELECT Motivo, DataHoraInicio, DataHoraFim, LatitudeInicio, LongitudeInicio, LatitudeFim, LongitudeFim
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
                        LngFim = TryParseNullableCoord(p.LongitudeFim)
                    }).ToList();

                resultado.Add(new MonitoramentoRotaDTO
                {
                    ExecucaoId = exec.ExecucaoId,
                    RotaId = exec.RotaId,
                    Descricao = exec.Descricao,
                    DataParaExecucao = exec.DataParaExecucao != null ? exec.DataParaExecucao.Value.ToString("dd/MM/yyyy") : "",
                    NomePaciente = exec.NomePaciente,
                    MedicoResponsavel = exec.MedicoResponsavel,
                    HoraInicio = exec.DataHoraInicio.ToString("dd/MM/yy HH:mm"),
                    HoraFim = exec.DataHoraFim?.ToString("dd/MM/yy HH:mm") ?? "Em Execucao",
                    MotoristaId = exec.MotoristaId,
                    MotoristaNome = exec.MotoristaNome,
                    VeiculoId = exec.VeiculoId,
                    PlacaModelo = $"{exec.Placa} - {exec.Modelo}",
                    TipoVeiculo = exec.TipoVeiculo,
                    UltimaLocalizacao = ultimaPos,
                    HistoricoLocalizacoes = historico,
                    UltimaAtualizacao = locais.Any() ? locais.Last().DataHora.ToString("dd/MM/yyyy HH:mm:ss") : exec.DataHoraInicio.ToString("dd/MM/yyyy HH:mm:ss"),
                    Paradas = paradas,
                    Pausas = pausas,
                    Finalizada = exec.DataHoraFim.HasValue,
                    SujeitoADesvio = sofreuDesvio
                });
            }

            return resultado;
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
    }
}
