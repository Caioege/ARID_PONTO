using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum ePerfilDeAcesso
    {
        [Description("Administrador")]
        AdministradorDeSistema,

        [Description("Rede de Ensino")]
        RedeDeEnsino,

        [Description("Escola")]
        Escola
    }
}