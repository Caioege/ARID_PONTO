using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum ePerfilDeAcesso
    {
        [Description("Administrador")]
        AdministradorDeSistema,

        [Description("Organização")]
        Organizacao,

        [Description("Unidade")]
        UnidadeOrganizacional,

        [Description("Departamento")]
        Departamento,

        [Description("Servidor")]
        Servidor
    }
}