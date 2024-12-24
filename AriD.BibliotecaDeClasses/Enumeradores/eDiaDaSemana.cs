using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eDiaDaSemana
    {
        [Description("Segunda")]
        Segunda = DayOfWeek.Monday,

        [Description("Terça")]
        Terca = DayOfWeek.Tuesday,

        [Description("Quarta")]
        Quarta = DayOfWeek.Wednesday,

        [Description("Quinta")]
        Quinta = DayOfWeek.Thursday,

        [Description("Sexta")]
        Sexta = DayOfWeek.Friday,

        [Description("Sábado")]
        Sabado = DayOfWeek.Saturday,

        [Description("Domingo")]
        Domingo = DayOfWeek.Sunday
    }
}