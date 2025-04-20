using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    [Description("Rede de Ensino")]
    public enum eItemDePermissao_RedeDeEnsino
    {
        Visualizar = 0,
        [Description("Alterar dados")]
        AlterarDados = 1,
        [Description("Cadastrar Escolas")]
        CadastrarEscolas = 2,
        [Description("Alterar Escolas")]
        AlterarEscolas = 3,
        [Description("Remover Escolas")]
        RemoverEscolas = 4,
        [Description("Imprimir lista de Escolas")]
        Imprimir = 5
    }
}