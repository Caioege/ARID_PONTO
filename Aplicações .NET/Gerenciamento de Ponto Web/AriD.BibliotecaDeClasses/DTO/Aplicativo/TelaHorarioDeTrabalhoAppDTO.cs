namespace AriD.BibliotecaDeClasses.DTO.Aplicativo
{
    public class TelaHorarioDeTrabalhoAppDTO
    {
        public int HorarioId { get; set; }
        public string HorarioDescricao { get; set; }

        public List<ItemHorarioAppDTO> Dias { get; set; } = [];
    }
}