using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eColunasDaFolha
    {
        Nenhuma = 0,

        [Description("Horas Trabalhadas")]
        HorasTrabalhadas = 1,

        [Description("Carga Horária")]
        CargaHoraria = 2,

        [Description("Horas Positivas")]
        HorasPositivas = 4,

        [Description("Horas Negativas")]
        HorasNegativas = 8,

        [Description("BH Saldo")]
        BHSaldo = 16,

        [Description("BH Ajuste")]
        BHAjuste = 32,

        Todas = HorasTrabalhadas | CargaHoraria | HorasPositivas | HorasNegativas | BHSaldo | BHAjuste
    }
}