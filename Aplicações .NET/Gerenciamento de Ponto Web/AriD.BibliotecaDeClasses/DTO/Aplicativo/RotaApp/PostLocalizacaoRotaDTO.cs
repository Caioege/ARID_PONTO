namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class PostLocalizacaoRotaDTO
    {
        public int RotaId { get; set; }
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
}
