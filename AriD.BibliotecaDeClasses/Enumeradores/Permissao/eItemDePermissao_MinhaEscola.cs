using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    [Description("Escola")]
    public enum eItemDePermissao_MinhaEscola
    {
        Visualizar = 0,
        [Description("Alterar dados")]
        AlterarDados = 1
    }
}