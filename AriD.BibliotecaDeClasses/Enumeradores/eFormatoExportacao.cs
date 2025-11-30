using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eFormatoExportacao
    {
        [Description("Excel (.xlsx)")]
        Excel = 0,
        [Description("Texto (.txt)")]
        Texto = 1,
        [Description("CSV (.csv)")]
        CSV = 2
    }
}