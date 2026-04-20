namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class PausaRotaAppDTO
    {
        public int RotaExecucaoId { get; set; }
        public string? Motivo { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public DateTime DataHora { get; set; }
    }

    public class RegistoPausaExecucao
    {
        public DateTime DataHoraInicio { get; set; }
        public string LatInicio { get; set; }
        public string LngInicio { get; set; }
        public string Motivo { get; set; }
        
        public DateTime? DataHoraFim { get; set; }
        public string? LatFim { get; set; }
        public string? LngFim { get; set; }
    }
}
