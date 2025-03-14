using AriD.BibliotecaDeClasses.Atributos;
using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eDiaDaSemana
    {
        [Description("Segunda")]
        [SiglaDiaDaSemana("SEG")]
        Segunda = DayOfWeek.Monday,

        [Description("Terça")]
        [SiglaDiaDaSemana("TER")]
        Terca = DayOfWeek.Tuesday,

        [Description("Quarta")]
        [SiglaDiaDaSemana("QUA")]
        Quarta = DayOfWeek.Wednesday,

        [Description("Quinta")]
        [SiglaDiaDaSemana("QUI")]
        Quinta = DayOfWeek.Thursday,

        [Description("Sexta")]
        [SiglaDiaDaSemana("SEX")]
        Sexta = DayOfWeek.Friday,

        [Description("Sábado")]
        [SiglaDiaDaSemana("SAB")]
        Sabado = DayOfWeek.Saturday,

        [Description("Domingo")]
        [SiglaDiaDaSemana("DOM")]
        Domingo = DayOfWeek.Sunday
    }
}
