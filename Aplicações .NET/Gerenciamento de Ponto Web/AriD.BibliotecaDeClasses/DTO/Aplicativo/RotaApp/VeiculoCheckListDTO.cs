namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class VeiculoCheckListDTO
    {
        public int Id { get; set; }
        public int RotaId { get; set; }
        public string Nome { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public string Cor { get; set; }

        public List<CheckListItemDTO> Checklist { get; set; } = [];
    }
}