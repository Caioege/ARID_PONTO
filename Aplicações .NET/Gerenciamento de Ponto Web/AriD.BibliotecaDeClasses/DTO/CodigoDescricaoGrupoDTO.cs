namespace AriD.BibliotecaDeClasses.DTO
{
    public class CodigoDescricaoGrupoDTO : CodigoDescricaoDTO
    {
        public CodigoDescricaoGrupoDTO() { }

        public CodigoDescricaoGrupoDTO(int codigo, string descricao, string grupo)
            : base(codigo, descricao)
        {
            Grupo = grupo;
        }

        public string Grupo { get; set; }
    }
}