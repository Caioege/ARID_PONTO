namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class IniciarRotaAppDTO
    {
        public int RotaId { get; set; }
        public int VeiculoId { get; set; }
        public int? ChecklistExecucaoId { get; set; }
        public string? LatitudeInicio { get; set; }
        public string? LongitudeInicio { get; set; }
        public bool GpsSimulado { get; set; }
        public string? ObservacaoInicio { get; set; }
        public List<PresencaPacienteRotaAppDTO> PacientesPresenca { get; set; } = new List<PresencaPacienteRotaAppDTO>();
        public List<PresencaProfissionalRotaAppDTO> ProfissionaisPresenca { get; set; } = new List<PresencaProfissionalRotaAppDTO>();
    }

    public class PresencaPacienteRotaAppDTO
    {
        public int? PacienteId { get; set; }
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public string? Telefone { get; set; }
        public bool Presente { get; set; }
        public bool PossuiAcompanhante { get; set; }
        public bool AcompanhantePresente { get; set; }
        public string? AcompanhanteNome { get; set; }
        public string? AcompanhanteCPF { get; set; }
        public bool NovoPaciente { get; set; }
        public bool IncluirNaRota { get; set; }
    }

    public class PresencaProfissionalRotaAppDTO
    {
        public int ServidorId { get; set; }
        public string? Nome { get; set; }
        public bool Presente { get; set; }
    }
}
