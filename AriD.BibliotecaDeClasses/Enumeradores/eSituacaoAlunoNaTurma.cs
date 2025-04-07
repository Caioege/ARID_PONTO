using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eSituacaoAlunoNaTurma
    {
        [Description("Cursando")]
        Cursando,

        [Description("Transferido")]
        Transferido,

        [Description("Evadido")]
        Evadido,

        [Description("Aprovado")]
        Aprovado,

        [Description("Reprovado")]
        Reprovado,

        [Description("Progressão Parcial")]
        ProgressaoParcial,

        [Description("Concluído")]
        Concluido
    }
}