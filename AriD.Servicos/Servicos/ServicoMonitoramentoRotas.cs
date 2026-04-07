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
                    COALESCE(re.MotoristaId, r.MotoristaId) as MotoristaId,
                    p.Nome as MotoristaNome,
                    re.VeiculoId,
                    v.Placa,
                    v.Modelo,
                    v.TipoVeiculo
                FROM rotaexecucao re
                INNER JOIN rota r ON r.Id = re.RotaId
                INNER JOIN motorista m ON m.Id = COALESCE(re.MotoristaId, r.MotoristaId)
                INNER JOIN servidor s ON s.Id = m.ServidorId
                INNER JOIN pessoa p ON p.Id = s.PessoaId
                LEFT JOIN veiculo v ON v.Id = re.VeiculoId
                WHERE re.OrganizacaoId = @OrganizacaoId
                  AND re.DataHoraInicio >= @DataBase
                  AND (re.DataHoraFim IS NULL OR re.DataHoraFim <= @DataFim)
            ";

            if (!exibirFinalizadas)
            {
                queryExecucoes += " AND re.DataHoraFim IS NULL ";
            }

            var execucoes = _repositorio.ConsultaDapper<RotaExecucaoDapperDTO>(queryExecucoes, new
            {
                OrganizacaoId = organizacaoId,
                DataBase = dataBase.Date,
                DataFim = dataFim
            }).ToList();

            if (!execucoes.Any()) return new List<MonitoramentoRotaDTO>();

            var veiculoIds = execucoes.Where(e => e.VeiculoId != null).Select(e => (int)e.VeiculoId).Distinct().ToList();

            var queryLocalizacoes = @"
                SELECT 
                    VeiculoId,
                    DataHora,
                    Latitude,
                    Longitude
                FROM localizacaoveiculo
                WHERE OrganizacaoId = @OrganizacaoId
                  AND VeiculoId IN @VeiculoIds
                  AND DataHora >= @DataBase
                  AND DataHora <= @DataFim
                ORDER BY DataHora ASC
            ";

            var localizacoesAll = _repositorio.ConsultaDapper<LocalizacaoDapperDTO>(queryLocalizacoes, new
            {
                OrganizacaoId = organizacaoId,
                VeiculoIds = veiculoIds,
                DataBase = dataBase.Date,
                DataFim = dataFim
            });

            var rotaIds = execucoes.Select(e => (int)e.RotaId).Distinct().ToList();

            var queryParadas = @"
                SELECT 
                    RotaId,
                    Endereco as nome,
                    Link as link,
                    Latitude,
                    Longitude,
                    Entregue as entregue,
                    ConcluidoEm as concluidoEm
                FROM paradarota
                WHERE RotaId IN @RotaIds
            ";

            var paradasAll = _repositorio.ConsultaDapper<ParadaDapperDTO>(queryParadas, new
            {
                RotaIds = rotaIds
            });

            var resultado = new List<MonitoramentoRotaDTO>();

            foreach (var exec in execucoes)
            {
                DateTime? hdFim = exec.DataHoraFim != null ? (DateTime?)exec.DataHoraFim : null;
                var dtFimFiltro = hdFim ?? DateTime.Now;

                var idVeiculo = exec.VeiculoId ?? 0;
                var dtInicio = (DateTime)exec.DataHoraInicio;

                var locais = localizacoesAll
                    .Where(l => l.VeiculoId == idVeiculo && l.DataHora >= dtInicio && l.DataHora <= dtFimFiltro)
                    .ToList();

                if (!locais.Any()) continue;

                var ultima = locais.Last();

                var historico = locais.Select(l => new double[] { 
                    Convert.ToDouble(l.Latitude.Replace("\"", "").Replace(" ", "").Replace(".", ",")), 
                    Convert.ToDouble(l.Longitude.Replace("\"", "").Replace(" ", "").Replace(".", ",")) 
                }).ToList();

                var idRota = exec.RotaId;
                var paradas = paradasAll
                    .Where(p => p.RotaId == idRota && !string.IsNullOrEmpty(p.Latitude) && !string.IsNullOrEmpty(p.Longitude))
                    .Select(p => new MonitoramentoParadaDTO {
                        Nome = p.Nome,
                        Link = p.Link,
                        Latitude = Convert.ToDouble(p.Latitude.Replace("\"", "").Replace(" ", "").Replace(".", ",")),
                        Longitude = Convert.ToDouble(p.Longitude.Replace("\"", "").Replace(" ", "").Replace(".", ",")),
                        Entregue = p.Entregue,
                        ConcluidoEm = p.ConcluidoEm != null ? p.ConcluidoEm.Value.ToString("dd/MM/yy HH:mm") : null
                    }).ToList();

                resultado.Add(new MonitoramentoRotaDTO
                {
                    ExecucaoId = exec.ExecucaoId,
                    RotaId = exec.RotaId,
                    Descricao = exec.Descricao,
                    DataParaExecucao = exec.DataParaExecucao != null ? exec.DataParaExecucao.Value.ToString("dd/MM/yyyy") : "",
                    NomePaciente = exec.NomePaciente,
                    MedicoResponsavel = exec.MedicoResponsavel,
                    HoraInicio = dtInicio.ToString("dd/MM/yy HH:mm"),
                    HoraFim = hdFim?.ToString("dd/MM/yy HH:mm") ?? "Em Execução",
                    MotoristaId = exec.MotoristaId,
                    MotoristaNome = exec.MotoristaNome,
                    VeiculoId = exec.VeiculoId,
                    PlacaModelo = $"{exec.Placa} - {exec.Modelo}",
                    TipoVeiculo = exec.TipoVeiculo,
                    UltimaLocalizacao = new[] { 
                        Convert.ToDouble(ultima.Latitude.Replace("\"", "").Replace(" ", "").Replace(".", ",")), 
                        Convert.ToDouble(ultima.Longitude.Replace("\"", "").Replace(" ", "").Replace(".", ",")) 
                    },
                    HistoricoLocalizacoes = historico,
                    UltimaAtualizacao = ultima.DataHora.ToString("dd/MM/yyyy HH:mm:ss"),
                    Paradas = paradas,
                    Finalizada = hdFim.HasValue
                });
            }

            return resultado;
        }
    }
}
