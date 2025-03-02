using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    public enum eItemDePermissao_Servidor
    {
        Visualizar = 0,
        [Description("Cadastrar/Alterar")]
        CadastrarOuAlterar = 1,
        Excluir = 2,
        [Description("Gerenciar Lotações")]
        GerenciarLotacoes = 3,
        [Description("Gerenciar Afastamentos")]
        GerenciarAfastamentos = 4
    }
}