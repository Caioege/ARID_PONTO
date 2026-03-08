using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eFormatoQuantidadeExportacao
    {
        [Description("HH:mm")]
        HHMM = 1,

        [Description("Minutos")]
        Minutos = 2,

        [Description("Horas decimais")]
        HorasDecimais = 3
    }
}