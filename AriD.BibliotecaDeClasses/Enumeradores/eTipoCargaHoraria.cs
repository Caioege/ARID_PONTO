using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eTipoCargaHoraria
    {
        [Description("Entrada e Saída")]
        EntradaSaida,
        [Description("Diária Fixa")]
        DiariaFixa,
        [Description("Mensal Fixa")]
        MensalFixa
    }
}