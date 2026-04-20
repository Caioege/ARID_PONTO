using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eSituacaoManutencao
    {
        [Description("A ser executada")]
        ASerExecutada,
        [Description("Executada")]
        Executada,
        [Description("Remarcada")]
        Remarcada,
        [Description("Não executada")]
        NaoExecutada
    }
}
