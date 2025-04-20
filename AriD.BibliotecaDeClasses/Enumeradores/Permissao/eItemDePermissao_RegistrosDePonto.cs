using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    [Description("Registros de Frequência")]
    public enum eItemDePermissao_RegistrosDePonto
    {
        Visualizar = 0,
        [Description("Exportar CSV")]
        ExportarCSV = 1
    }
}
