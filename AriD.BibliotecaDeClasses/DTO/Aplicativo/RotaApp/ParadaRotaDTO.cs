
namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class ParadaRotaDTO
    {
        public int Id { get; set; }
        public string Endereco { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Link { get; set; }
        public bool? Entregue { get; set; }
        public string? Observacao { get; set; }
        public string? ConcluidoEm { get; set; }
        public string? LatitudeConfirmacao { get; set; }
        public string? LongitudeConfirmacao { get; set; }
    }
}
