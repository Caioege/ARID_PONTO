using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.ParametrosDeConsulta
{
    public class ParametrosConsultaMotorista : ParametrosConsultaUnidadesOrganizacionais
    {
        public eStatusMotorista? Situacao { get; set; }
        public eCategoriaCNH? CategoriaCNH { get; set; }
    }
}
