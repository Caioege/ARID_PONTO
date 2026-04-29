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
    }
}
