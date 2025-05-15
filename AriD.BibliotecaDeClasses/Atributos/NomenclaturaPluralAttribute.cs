namespace AriD.BibliotecaDeClasses.Atributos
{
    public class NomenclaturaPluralAttribute : Attribute
    {
        private readonly string _descricao;

        public NomenclaturaPluralAttribute(string descricao)
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