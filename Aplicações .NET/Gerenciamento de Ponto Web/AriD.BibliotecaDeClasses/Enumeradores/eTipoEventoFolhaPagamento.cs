using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eTipoEventoFolhaPagamento
    {
        [Description("Hora Extra")]
        HoraExtra = 1,

        [Description("Horas Negativas (Faltas/Atrasos)")]
        HorasNegativas = 2,

        [Description("Abono")]
        Abono = 3,

        [Description("Banco de Horas (Crédito)")]
        BancoHorasCredito = 4,

        [Description("Banco de Horas (Débito)")]
        BancoHorasDebito = 5,

        [Description("Bônus (VT/VA) em Dinheiro")]
        Bonus = 6
    }
}