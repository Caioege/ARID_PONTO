using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eTurno
    {
        [Description("Matutino")]
        Matutino,
        [Description("Vespertino")]
        Vespertino,
        [Description("Integral")]
        Integral,
        [Description("Noturno")]
        Noturno
    }
}