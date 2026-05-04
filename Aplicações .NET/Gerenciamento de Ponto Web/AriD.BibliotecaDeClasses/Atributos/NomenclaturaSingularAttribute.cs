namespace AriD.BibliotecaDeClasses.Atributos
{
    public class NomenclaturaSingularAttribute : Attribute
    {
        private readonly string _descricao;

        public NomenclaturaSingularAttribute(string descricao)
        {
            _descricao = descricao;
        }

        public string Descricao
        {
            get
            {
                return _descricao;
            }
        }
    }
}