using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eStatusMotorista
    {
        Ativo,
        [Description("Em Viagem")]
        EmViagem,
        [Description("De Férias")]
        DeFerias,
        Afastado,
        Inativo
    }
}
