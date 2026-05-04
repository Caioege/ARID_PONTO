namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class RotaCheckListDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool RotaFinalizada { get; set; }
        public int? RotaExecucaoFinalizadaId { get; set; }
        public bool PermiteIniciarSemPacienteAcompanhante { get; set; } = true;
        public bool PermiteIniciarSemProfissional { get; set; } = true;
        public List<RotaPacienteAppDTO> Pacientes { get; set; } = new List<RotaPacienteAppDTO>();
        public List<RotaProfissionalAppDTO> Profissionais { get; set; } = new List<RotaProfissionalAppDTO>();
        public List<PacienteDisponivelAppDTO> PacientesDisponiveis { get; set; } = new List<PacienteDisponivelAppDTO>();
    }

    public class RotaPacienteAppDTO
    {
        public int PacienteId { get; set; }
        public string Nome { get; set; }
        public string? CPF { get; set; }
        public string? Telefone { get; set; }
        public bool PossuiAcompanhante { get; set; }
        public string? AcompanhanteNome { get; set; }
        public string? AcompanhanteCPF { get; set; }
    }

    public class PacienteDisponivelAppDTO
    {
        public int PacienteId { get; set; }
        public string Nome { get; set; }
        public string? CPF { get; set; }
        public string? Telefone { get; set; }
        public string? AcompanhanteNome { get; set; }
        public string? AcompanhanteCPF { get; set; }
    }

    public class RotaProfissionalAppDTO
    {
        public int ServidorId { get; set; }
        public string Nome { get; set; }
        public string? Funcao { get; set; }
        public string? Observacao { get; set; }
    }
}
