namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class ChecklistPostDTO
    {
        public int RotaId { get; set; }
        public int VeiculoId { get; set; }
        public List<int> Itens { get; set; }
    }
}
