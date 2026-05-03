using System;
using System.Collections.Generic;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RotaExecucaoDapperDTO
    {
        public int ExecucaoId { get; set; }
        public int RotaId { get; set; }
        public string Descricao { get; set; }
        public DateTime? DataParaExecucao { get; set; }
        public string NomePaciente { get; set; }
        public string MedicoResponsavel { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public int Status { get; set; }
        public int MotoristaId { get; set; }
        public string MotoristaNome { get; set; }
        public int? VeiculoId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public int? TipoVeiculo { get; set; }
        public bool PossuiRegistroOffline { get; set; }
        public bool ExecucaoOfflineCompleta { get; set; }
        public DateTime? DataHoraUltimaComunicacaoApp { get; set; }
    }

    public class LocalizacaoDapperDTO
    {
        public int ExecucaoId { get; set; }
        public DateTime DataHora { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public double? VelocidadeMetrosPorSegundo { get; set; }
        public bool RegistradoOffline { get; set; }
        public DateTime? DataHoraRegistroLocal { get; set; }
        public DateTime? DataHoraSincronizacao { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public string? ClientEventId { get; set; }
    }

    public class ParadaDapperDTO
    {
        public int RotaId { get; set; }
        public string Nome { get; set; }
        public string Link { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool Entregue { get; set; }
        public DateTime? ConcluidoEm { get; set; }
    }

    public class ParadaMonitoramentoRowDTO
    {
        public string Nome { get; set; }
        public string Link { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool? Entregue { get; set; }
        public DateTime? ConcluidoEm { get; set; }
        public bool RegistradoOffline { get; set; }
        public DateTime? DataHoraRegistroLocal { get; set; }
        public DateTime? DataHoraSincronizacao { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public string? ClientEventId { get; set; }
    }

    public class PausaExecucaoRowDTO
    {
        public string Motivo { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public string? LatitudeInicio { get; set; }
        public string? LongitudeInicio { get; set; }
        public string? LatitudeFim { get; set; }
        public string? LongitudeFim { get; set; }
        public bool RegistradoOffline { get; set; }
        public DateTime? DataHoraRegistroLocal { get; set; }
        public DateTime? DataHoraSincronizacao { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public string? ClientEventId { get; set; }
    }

    public class MonitoramentoParadaDTO
    {
        public string Nome { get; set; }
        public string Link { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Entregue { get; set; }
        public string ConcluidoEm { get; set; }
        public bool RegistradoOffline { get; set; }
        public string? DataHoraRegistroLocal { get; set; }
        public string? DataHoraSincronizacao { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public string? ClientEventId { get; set; }
    }

    public class MonitoramentoPausaDTO
    {
        public string Motivo { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public double? LatInicio { get; set; }
        public double? LngInicio { get; set; }
        public double? LatFim { get; set; }
        public double? LngFim { get; set; }
        public bool RegistradoOffline { get; set; }
        public string? DataHoraRegistroLocal { get; set; }
        public string? DataHoraSincronizacao { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public string? ClientEventId { get; set; }
    }

    public class MonitoramentoLocalizacaoDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DataHora { get; set; }
        public bool RegistradoOffline { get; set; }
        public string? DataHoraRegistroLocal { get; set; }
        public string? DataHoraSincronizacao { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public string? ClientEventId { get; set; }
    }

    public class MonitoramentoRotaDTO
    {
        public int ExecucaoId { get; set; }
        public int RotaId { get; set; }
        public string Descricao { get; set; }
        public string DataParaExecucao { get; set; }
        public string NomePaciente { get; set; }
        public string MedicoResponsavel { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFim { get; set; }
        public int MotoristaId { get; set; }
        public string MotoristaNome { get; set; }
        public int? VeiculoId { get; set; }
        public string PlacaModelo { get; set; }
        public double[] UltimaLocalizacao { get; set; }
        public List<double[]> HistoricoLocalizacoes { get; set; }
        public List<MonitoramentoLocalizacaoDTO> HistoricoLocalizacoesDetalhado { get; set; }
        public string UltimaAtualizacao { get; set; }
        public List<MonitoramentoParadaDTO> Paradas { get; set; }
        public List<MonitoramentoPausaDTO> Pausas { get; set; }
        public int? TipoVeiculo { get; set; }
        public double? VelocidadeMediaKmH { get; set; }
        public bool Finalizada { get; set; }
        public bool SujeitoADesvio { get; set; }
        public bool PossuiRegistroOffline { get; set; }
        public bool ExecucaoOfflineCompleta { get; set; }
        public string? ClassificacaoOffline { get; set; }
        public bool PossivelmenteOffline { get; set; }
        public int? MinutosSemComunicacao { get; set; }
        public string? UltimaComunicacaoApp { get; set; }
    }
}
