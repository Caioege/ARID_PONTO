using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores.Permissao
{
    public enum eItemDePermissao_FolhaDePonto
    {
        Gerenciar = 0,

        [Description("Mover registros")]
        MoverRegistros = 1,

        [Description("Resetar folha")]
        Resetar = 2,

        [Description("Gerenciar registros do aplicativo")]
        GerenciarRegistrosDoAplicativo = 3,

        [Description("Visualizar Histórico")]
        VisualizarHistorico = 4,

        [Description("Desconsiderar Registros")]
        DesconsiderarRegistros = 5,

        [Description("Aprovação em Lote")]
        AprovacaoEmLote = 6
    }
}