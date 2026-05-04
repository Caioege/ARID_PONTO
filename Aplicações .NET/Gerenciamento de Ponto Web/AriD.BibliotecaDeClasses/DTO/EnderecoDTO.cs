using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class EnderecoDTO
    {
        public string CEP { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Localidade { get; set; }
        public string UF { get; set; }
        public eEstadosDoBrasil? UFEnum => string.IsNullOrEmpty(UF) ? null : (eEstadosDoBrasil)Enum.Parse(typeof(eEstadosDoBrasil), UF);
    }
}