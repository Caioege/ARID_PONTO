namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class PacoteOfflineRastreioDTO
    {
        public DateTime DataHoraGeracao { get; set; }
        public DateTime ValidoAte { get; set; }
        public int ValidadeEmDias { get; set; } = 3;
        public List<RotaOfflineDTO> Rotas { get; set; } = new List<RotaOfflineDTO>();
    }

    public class RotaOfflineDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool PermitePausa { get; set; }
        public int QuantidadePausas { get; set; }
        public bool PermiteIniciarSemPacienteAcompanhante { get; set; } = true;
        public bool PermiteIniciarSemProfissional { get; set; } = true;
        public int? UnidadeOrigemId { get; set; }
        public int? UnidadeDestinoId { get; set; }
        public string? NomeUnidadeOrigem { get; set; }
        public string? NomeUnidadeDestino { get; set; }
        public string? OrigemLatitudeRota { get; set; }
        public string? OrigemLongitudeRota { get; set; }
        public string? DestinoLatitudeRota { get; set; }
        public string? DestinoLongitudeRota { get; set; }
        public List<VeiculoCheckListDTO> Veiculos { get; set; } = new List<VeiculoCheckListDTO>();
        public List<ParadaRotaDTO> Paradas { get; set; } = new List<ParadaRotaDTO>();
        public List<RotaPacienteAppDTO> Pacientes { get; set; } = new List<RotaPacienteAppDTO>();
        public List<RotaProfissionalAppDTO> Profissionais { get; set; } = new List<RotaProfissionalAppDTO>();
        public List<PacienteDisponivelAppDTO> PacientesDisponiveis { get; set; } = new List<PacienteDisponivelAppDTO>();
    }

    public class SincronizarRotaOfflineDTO
    {
        public string LocalExecucaoId { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public int RotaId { get; set; }
        public int VeiculoId { get; set; }
        public int? ChecklistExecucaoId { get; set; }
        public List<int> ItensChecklist { get; set; } = new List<int>();
        public DateTime DataHoraInicioLocal { get; set; }
        public DateTime? DataHoraFimLocal { get; set; }
        public string? LatitudeInicio { get; set; }
        public string? LongitudeInicio { get; set; }
        public bool GpsSimuladoInicio { get; set; }
        public string? ObservacaoInicio { get; set; }
        public string? ObservacaoFim { get; set; }
        public List<PresencaPacienteRotaAppDTO> PacientesPresenca { get; set; } = new List<PresencaPacienteRotaAppDTO>();
        public List<PresencaProfissionalRotaAppDTO> ProfissionaisPresenca { get; set; } = new List<PresencaProfissionalRotaAppDTO>();
        public List<EventoRotaOfflineDTO> Eventos { get; set; } = new List<EventoRotaOfflineDTO>();
        public List<LocalizacaoRotaOfflineDTO> Localizacoes { get; set; } = new List<LocalizacaoRotaOfflineDTO>();
        public List<PausaRotaOfflineDTO> Pausas { get; set; } = new List<PausaRotaOfflineDTO>();
    }

    public class AuditoriaOfflineDTO
    {
        public bool RegistradoOffline { get; set; } = true;
        public DateTime DataHoraRegistroLocal { get; set; }
        public DateTime? DataHoraSincronizacao { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public string LocalExecucaoId { get; set; }
        public string ClientEventId { get; set; }
    }

    public class EventoRotaOfflineDTO : AuditoriaOfflineDTO
    {
        public int TipoEvento { get; set; }
        public int? ParadaRotaId { get; set; }
        public int? UnidadeId { get; set; }
        public bool? Entregue { get; set; }
        public string? Observacao { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool GpsSimulado { get; set; }
        public DateTime DataHoraEvento { get; set; }
    }

    public class LocalizacaoRotaOfflineDTO : AuditoriaOfflineDTO
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime DataHora { get; set; }
        public bool GpsSimulado { get; set; }
        public decimal? PrecisaoEmMetros { get; set; }
        public decimal? VelocidadeMetrosPorSegundo { get; set; }
        public decimal? DirecaoGraus { get; set; }
        public decimal? AltitudeMetros { get; set; }
        public int? FonteCaptura { get; set; }
    }

    public class PausaRotaOfflineDTO : AuditoriaOfflineDTO
    {
        public string? Motivo { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public string? LatitudeInicio { get; set; }
        public string? LongitudeInicio { get; set; }
        public bool GpsSimuladoInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public string? LatitudeFim { get; set; }
        public string? LongitudeFim { get; set; }
        public bool GpsSimuladoFim { get; set; }
    }

    public class ResultadoSincronizacaoRotaOfflineDTO
    {
        public bool Sucesso { get; set; }
        public int? RotaExecucaoId { get; set; }
        public string LocalExecucaoId { get; set; }
        public string? Mensagem { get; set; }
        public DateTime DataHoraSincronizacao { get; set; }
    }
}
