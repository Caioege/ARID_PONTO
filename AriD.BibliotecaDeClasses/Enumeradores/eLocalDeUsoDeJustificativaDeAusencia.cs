using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eLocalDeUsoDeJustificativaDeAusencia
    {
        [Description("Afasmento")]
        Afastamento,

        [Description("Folha de Ponto")]
        FolhaDePonto,

        [Description("Afastamento e Folha de Ponto")]
        AfastamentoEFolhaDePonto
    }
}