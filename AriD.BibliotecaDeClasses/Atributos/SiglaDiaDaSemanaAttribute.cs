namespace AriD.BibliotecaDeClasses.Atributos
{
    public class SiglaDiaDaSemanaAttribute : Attribute
    {
        private readonly string _sigla;

        public SiglaDiaDaSemanaAttribute(string sigla)
        {
            _sigla = sigla;
        }

        public string Sigla
        {
            get
            {
                return _sigla;
            }
        }
    }
}