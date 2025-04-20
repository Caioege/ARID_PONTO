using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    [Description("Turma")]
    public enum eItemDePermissao_Turma
    {
        Visualizar = 0,
        [Description("Cadastrar/Alterar")]
        CadastrarOuAlterar = 1,
        Excluir = 2,
        [Description("Alocar alunos")]
        AlocarAlunos = 3,
        [Description("Alterar dados de alunos na turma")]
        AlterarDadosAlunoTurma = 4,
        [Description("Gerenciar Horário de Aula")]
        GerenciarHorarioDeAula = 5,
        [Description("Gerenciar Diário de Classe")]
        GerenciarDiarioDeClasse = 6,
        [Description("Imprimir Diário de Classe")]
        ImprimirDiarioDeClasse = 7
    }
}