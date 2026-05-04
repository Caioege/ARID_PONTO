using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    public enum eItemDePermissao_MotivoDeDemissao
    {
        Visualizar = 0,
        [Description("Cadastrar/Alterar")]
        CadastrarOuAlterar = 1,
        Excluir = 2
    }
}