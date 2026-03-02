namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class RotaExecucaoDTO
    {
        public int Id { get; set; }
        public int RotaId { get; set; }
        public string Descricao { get; set; }
        public bool EmAndamento { get; set; }

        public List<ParadaRotaDTO> Paradas { get; set; } = [];
    }
}