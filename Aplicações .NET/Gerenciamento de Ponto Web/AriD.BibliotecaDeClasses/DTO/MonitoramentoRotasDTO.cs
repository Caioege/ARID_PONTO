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
        public string ObservacaoRota { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public int Status { get; set; }
        public int MotoristaId { get; set; }
        public string MotoristaNome { get; set; }
        public int MotoristaPrincipalId { get; set; }
        public int? MotoristaSecundarioId { get; set; }
        public int? VeiculoId { get; set; }
        public int? ChecklistExecucaoId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public int? TipoVeiculo { get; set; }
        public bool PossuiRegistroOffline { get; set; }
        public bool ExecucaoOfflineCompleta { get; set; }
        public DateTime? DataHoraUltimaComunicacaoApp { get; set; }
        public int? UnidadeOrigemId { get; set; }
        public string? NomeUnidadeOrigem { get; set; }
        public string? EnderecoUnidadeOrigem { get; set; }
        public string? OrigemLatitudeRota { get; set; }
        public string? OrigemLongitudeRota { get; set; }
        public int? UnidadeDestinoId { get; set; }
        public string? NomeUnidadeDestino { get; set; }
        public string? EnderecoUnidadeDestino { get; set; }
        public string? DestinoLatitudeRota { get; set; }
        public string? DestinoLongitudeRota { get; set; }
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
        public string? MotoristaPapel { get; set; }
        public int? VeiculoId { get; set; }
        public int? ChecklistExecucaoId { get; set; }
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
        public int StatusExecucao { get; set; }
        public string? StatusDescricao { get; set; }
        public string? ObservacaoRota { get; set; }
        public List<MonitoramentoPessoaDTO> Pacientes { get; set; } = new List<MonitoramentoPessoaDTO>();
        public List<MonitoramentoPessoaDTO> Acompanhantes { get; set; } = new List<MonitoramentoPessoaDTO>();
        public List<MonitoramentoPessoaDTO> Profissionais { get; set; } = new List<MonitoramentoPessoaDTO>();
        public List<MonitoramentoEventoDTO> EventosRecentes { get; set; } = new List<MonitoramentoEventoDTO>();
        public List<MonitoramentoAlertaDTO> Alertas { get; set; } = new List<MonitoramentoAlertaDTO>();
        public MonitoramentoManutencaoDTO? ProximaManutencao { get; set; }
        public MonitoramentoUnidadeRotaDTO? UnidadeOrigem { get; set; }
        public MonitoramentoUnidadeRotaDTO? UnidadeDestino { get; set; }
    }

    public class MonitoramentoRotasResultadoDTO
    {
        public List<MonitoramentoRotaDTO> Rotas { get; set; } = new List<MonitoramentoRotaDTO>();
        public List<MonitoramentoManutencaoVeiculoDTO> ManutencoesVeiculos { get; set; } = new List<MonitoramentoManutencaoVeiculoDTO>();
    }

    public class ChecklistExecucaoRotaDTO
    {
        public int ChecklistExecucaoId { get; set; }
        public int ExecucaoId { get; set; }
        public int RotaId { get; set; }
        public string RotaDescricao { get; set; }
        public int VeiculoId { get; set; }
        public string VeiculoDescricao { get; set; }
        public int MotoristaId { get; set; }
        public string MotoristaNome { get; set; }
        public string DataHora { get; set; }
        public int TotalItens { get; set; }
        public int TotalMarcados { get; set; }
        public List<ChecklistExecucaoRotaItemDTO> Itens { get; set; } = new List<ChecklistExecucaoRotaItemDTO>();
    }

    public class ChecklistExecucaoRotaRowDTO
    {
        public int ChecklistExecucaoId { get; set; }
        public int ExecucaoId { get; set; }
        public int RotaId { get; set; }
        public string RotaDescricao { get; set; }
        public int VeiculoId { get; set; }
        public string VeiculoDescricao { get; set; }
        public int MotoristaId { get; set; }
        public string MotoristaNome { get; set; }
        public DateTime DataHora { get; set; }
    }

    public class ChecklistExecucaoRotaItemDTO
    {
        public int ChecklistItemId { get; set; }
        public string Descricao { get; set; }
        public bool Marcado { get; set; }
    }

    public class MonitoramentoUnidadeRotaDTO
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public string? Endereco { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class MonitoramentoPessoaDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Complemento { get; set; }
    }

    public class MonitoramentoEventoDTO
    {
        public int TipoEvento { get; set; }
        public string TipoDescricao { get; set; }
        public string DataHora { get; set; }
        public string? Observacao { get; set; }
        public bool RegistradoOffline { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class MonitoramentoAlertaDTO
    {
        public string Tipo { get; set; }
        public string Severidade { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
    }

    public class MonitoramentoManutencaoDTO
    {
        public string Descricao { get; set; }
        public string? Categoria { get; set; }
        public string? DataVencimento { get; set; }
        public string? DataManutencao { get; set; }
        public string? DataAgendamento { get; set; }
        public string? DataConclusao { get; set; }
        public string? GarantiaAte { get; set; }
        public int? KmProximaManutencao { get; set; }
        public int QuilometragemAtual { get; set; }
        public string? LocalExecucaoServico { get; set; }
        public string? Fornecedor { get; set; }
        public string? ResponsavelServico { get; set; }
        public string? ContatoFornecedor { get; set; }
        public string? NumeroDocumento { get; set; }
        public decimal? CustoPrevisto { get; set; }
        public decimal? ValorMaximoAutorizado { get; set; }
        public decimal? ValorTotalGasto { get; set; }
        public string? Observacao { get; set; }
        public bool Vencida { get; set; }
        public bool Proxima { get; set; }
        public string SituacaoDescricao { get; set; }
    }

    public class MonitoramentoManutencaoVeiculoDTO : MonitoramentoManutencaoDTO
    {
        public int ManutencaoId { get; set; }
        public int VeiculoId { get; set; }
        public string? Placa { get; set; }
        public string? Modelo { get; set; }
        public string? VeiculoDescricao { get; set; }
        public List<MonitoramentoRotaVinculadaDTO> RotasVinculadas { get; set; } = new List<MonitoramentoRotaVinculadaDTO>();
    }

    public class MonitoramentoRotaVinculadaDTO
    {
        public int RotaId { get; set; }
        public int? ExecucaoId { get; set; }
        public string? Descricao { get; set; }
        public string? MotoristaNome { get; set; }
        public string? StatusDescricao { get; set; }
        public string? DataParaExecucao { get; set; }
        public bool Finalizada { get; set; }
    }
}
