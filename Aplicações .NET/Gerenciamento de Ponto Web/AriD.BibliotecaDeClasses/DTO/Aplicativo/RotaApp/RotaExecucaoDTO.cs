namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class RotaAgendaAppDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public bool Recorrente { get; set; }
        public DateTime? DataParaExecucao { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? DiasSemana { get; set; }
        public bool PermiteIniciarSemPacienteAcompanhante { get; set; } = true;
        public bool PermiteIniciarSemProfissional { get; set; } = true;
    }

    public class UltimaLocalizacaoRotaDTO
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime DataHora { get; set; }
    }

    public class VeiculoResumoAppDTO
    {
        public int Id { get; set; }
        public string Modelo { get; set; }
        public string Placa { get; set; }
        public int Cor { get; set; }
    }

    public class RotaExecucaoResumoDTO
    {
        public int Id { get; set; }
        public int RotaId { get; set; }
        public int Status { get; set; }
        public int VeiculoId { get; set; }
        public int? ChecklistExecucaoId { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public string Descricao { get; set; }
        public bool PermitePausa { get; set; }
        public int QuantidadePausas { get; set; }
        public bool PossuiRegistroOffline { get; set; }
        public bool ExecucaoOfflineCompleta { get; set; }
        public DateTime? DataHoraUltimaComunicacaoApp { get; set; }
        public int? UnidadeOrigemId { get; set; }
        public int? UnidadeDestinoId { get; set; }
        public string? NomeUnidadeOrigem { get; set; }
        public string? NomeUnidadeDestino { get; set; }
        public string? OrigemLatitudeRota { get; set; }
        public string? OrigemLongitudeRota { get; set; }
        public string? DestinoLatitudeRota { get; set; }
        public string? DestinoLongitudeRota { get; set; }
    }

    public class RotaExecucaoEventoResumoDTO
    {
        public bool? Entregue { get; set; }
        public string? Observacao { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public DateTime? DataHoraEvento { get; set; }
        public bool RegistradoOffline { get; set; }
    }

    public class RotaExecucaoPermissaoPausaDTO
    {
        public bool PermitePausa { get; set; }
        public int QuantidadePausas { get; set; }
    }

    public class RotaExecucaoLookupDTO
    {
        public int Id { get; set; }
        public int MotoristaId { get; set; }
    }

    public class ExecucaoValidacaoDTO
    {
        public int Id { get; set; }
        public int OrganizacaoId { get; set; }
        public int RotaId { get; set; }
        public int Status { get; set; }
        public int MotoristaId { get; set; }
        public string? UltimaLatitude { get; set; }
        public string? UltimaLongitude { get; set; }
        public bool GpsSimuladoUltimaLeitura { get; set; }
        public bool PossuiRegistroOffline { get; set; }
        public bool ExecucaoOfflineCompleta { get; set; }
        public DateTime? DataHoraUltimaComunicacaoApp { get; set; }
        public int? UnidadeOrigemId { get; set; }
        public int? UnidadeDestinoId { get; set; }
    }

    public class RotaExecucaoDTO
    {
        public int Id { get; set; }
        public int RotaId { get; set; }
        public string Descricao { get; set; }
        public bool EmAndamento { get; set; }
        public bool PossuiRegistroOffline { get; set; }
        public bool ExecucaoOfflineCompleta { get; set; }
        public DateTime? DataHoraUltimaComunicacaoApp { get; set; }
        public DateTime? DataHoraPrimeiroRegistroOffline { get; set; }
        public DateTime? DataHoraUltimoRegistroOffline { get; set; }
        
        public bool PermitePausa { get; set; }
        public int QuantidadePausas { get; set; }
        public int QuantidadePausasRealizadas { get; set; }
        public bool EstaPausada { get; set; }
        public string? NomeUnidadeOrigem { get; set; }
        public string? OrigemLatitudeRota { get; set; }
        public string? OrigemLongitudeRota { get; set; }
        public bool? OrigemEntregue { get; set; }
        public string? OrigemObservacao { get; set; }
        public string? OrigemConcluidaEm { get; set; }
        public string? OrigemLatitude { get; set; }
        public string? OrigemLongitude { get; set; }
        
        public string? NomeUnidadeDestino { get; set; }
        public string? DestinoLatitudeRota { get; set; }
        public string? DestinoLongitudeRota { get; set; }
        public bool? DestinoEntregue { get; set; }
        public string? DestinoObservacao { get; set; }
        public string? DestinoConcluidoEm { get; set; }
        public string? DestinoLatitude { get; set; }
        public string? DestinoLongitude { get; set; }

        public int? VeiculoId { get; set; }
        public int? ChecklistExecucaoId { get; set; }

        public List<ParadaRotaDTO> Paradas { get; set; } = [];
    }
}
