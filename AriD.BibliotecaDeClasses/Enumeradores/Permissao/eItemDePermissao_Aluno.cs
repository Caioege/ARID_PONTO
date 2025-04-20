using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    public enum eItemDePermissao_Aluno
    {
        Visualizar = 0,
        [Description("Cadastrar/Alterar")]
        CadastrarOuAlterar = 1,
        Excluir = 2,
        [Description("Matricular na Escola")]
        MatricularNaEscola = 3,
        [Description("Desalocar da Escola")]
        DesalocarDaEscola = 4
    }
}
