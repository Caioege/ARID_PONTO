namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class RotaExecucaoDTO
    {
        public int Id { get; set; }
        public int RotaId { get; set; }
        public string Descricao { get; set; }
        public bool EmAndamento { get; set; }
        
        public bool PermitePausa { get; set; }
        public int QuantidadePausas { get; set; }
        public int QuantidadePausasRealizadas { get; set; }
        public bool EstaPausada { get; set; }
        public string? NomeUnidadeOrigem { get; set; }
        public bool? OrigemEntregue { get; set; }
        public string? OrigemObservacao { get; set; }
        
        public string? NomeUnidadeDestino { get; set; }
        public bool? DestinoEntregue { get; set; }
        public string? DestinoObservacao { get; set; }

        public List<ParadaRotaDTO> Paradas { get; set; } = [];
    }
}