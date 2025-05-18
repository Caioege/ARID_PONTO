using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    public enum eItemDePermissao_FolhaDePonto
    {
        Gerenciar = 0,

        [Description("Mover registros")]
        MoverRegistros = 1,

        [Description("Resetar folha")]
        Resetar = 2
    }
}