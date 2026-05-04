using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.ParametrosDeConsulta
{
    public class ParametrosConsultaRota : ParametrosConsultaUnidadesOrganizacionais
    {
        public eStatusRota? Situacao { get; set; }
        public bool? Recorrente { get; set; }
        public int? MotoristaId { get; set; }
    }
}
