namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class ConfirmarParadaAppDTO
    {
        public int RotaExecucaoId { get; set; }
        public int ParadaId { get; set; }
        public bool? Entregue { get; set; }
        public string Observacao { get; set; }
    }
}
