using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.ParametrosDeConsulta
{
    public class ParametrosConsultaVeiculo : ParametrosConsultaUnidadesOrganizacionais
    {
        public eStatusVeiculo? Situacao { get; set; }
        public eTipoCombustivel? TipoCombustivel { get; set; }
    }
}
