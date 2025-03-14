namespace AriD.BibliotecaDeClasses.DTO
{
    public class RegistroEquipamentoDTO
    {
        public string? UsuarioId { get; set; }
        public string? ModelName { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime DataHora { get; set; }
        public string? CurrentIp { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? Mac { get; set; }
        public float? Temperatura { get; set; }
        public int ModoDeAcesso { get; set; }
    }
}