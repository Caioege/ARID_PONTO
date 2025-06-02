using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eSituacaoRegistroAplicativo
    {
        [Description("Aguardando Avaliação")]
        AguardandoAvaliacao,
        Aprovado,
        Reprovado
    }
}