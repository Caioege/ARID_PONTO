namespace AriD.BibliotecaDeClasses.DTO
{
    public class CodigoDescricaoDTO
    {
        public CodigoDescricaoDTO() { }

        public CodigoDescricaoDTO(int codigo, string descricao)
        {
            Codigo = codigo;
            Descricao = descricao;
        }

        public int Codigo { get; set; }
        public string Descricao { get; set; }
    }
}