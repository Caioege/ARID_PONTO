namespace AriD.BibliotecaDeClasses.DTO
{
    public class AlertaManutencaoDTO
    {
        public int VeiculoId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public string Motivo { get; set; }
        public string ExpiracaoInfo { get; set; }
        public string TipoAlerta { get; set; } // "Alta" (vencido) ou "Media" (proximo)
    }
}
