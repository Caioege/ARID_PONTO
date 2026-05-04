using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.ParametrosDeConsulta
{
    public class ParametrosConsultaUnidadesOrganizacionais
    {
        public int OrganizacaoId { get; set; }
        public string? Nome { get; set; }
        public eTipoUnidadeOrganizacional? Tipo { get; set; }
    }
}