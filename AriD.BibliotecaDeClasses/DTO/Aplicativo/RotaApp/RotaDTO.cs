namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class RotaDTO
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public string Veiculo { get; set; }
        public bool EmAndamento { get; set; }

        public List<ParadaRotaDTO> Paradas { get; set; } = [];
    }
}