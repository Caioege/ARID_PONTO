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
        public string HistoricoPausas { get; set; }
        public int MotoristaId { get; set; }
        public string MotoristaNome { get; set; }
        public int? VeiculoId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public int? TipoVeiculo { get; set; }
    }

    public class LocalizacaoDapperDTO
    {
        public int RotaId { get; set; }
        public DateTime DataHora { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
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

    public class MonitoramentoParadaDTO
    {
        public string Nome { get; set; }
        public string Link { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Entregue { get; set; }
        public string ConcluidoEm { get; set; }
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
        public string UltimaAtualizacao { get; set; }
        public List<MonitoramentoParadaDTO> Paradas { get; set; }
        public List<MonitoramentoPausaDTO> Pausas { get; set; }
        public int? TipoVeiculo { get; set; }
        public bool Finalizada { get; set; }
        public bool SujeitoADesvio { get; set; }
    }
}
