using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eSituacaoTurma
    {
        [Description("Ativa")]
        Ativa,
        [Description("Inativa")]
        Inativa,
        [Description("Cancelada")]
        Cancelada,
        [Description("Finalizada")]
        Finalizada
    }
}