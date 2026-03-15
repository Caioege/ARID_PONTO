using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    public enum eItemDePermissao_Bonus
    {
        Gerenciar = 0,

        [Description("Visualizar")]
        Visualizar = 1,

        [Description("Cadastrar ou Alterar")]
        CadastrarOuAlterar = 2,

        [Description("Excluir")]
        Excluir = 3,

        [Description("Gerar Relatório")]
        GerarRelatorio = 4
    }
}
