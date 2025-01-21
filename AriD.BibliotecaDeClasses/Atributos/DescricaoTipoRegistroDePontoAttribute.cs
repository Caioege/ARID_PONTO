namespace AriD.BibliotecaDeClasses.Atributos
{
    public class DescricaoTipoRegistroDePontoAttribute : Attribute
    {
        private readonly string _descricao;

        public DescricaoTipoRegistroDePontoAttribute(string descricao)
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