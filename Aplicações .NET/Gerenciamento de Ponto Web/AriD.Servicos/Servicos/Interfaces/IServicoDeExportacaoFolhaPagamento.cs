using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeExportacaoFolhaPagamento
    {
        List<CodigoDescricaoDTO> ObtenhaLayouts(int organizacaoId);
        LayoutExportacaoFolhaPagamento ObtenhaLayoutCompleto(int organizacaoId, int layoutId);
        int SalvarLayout(int organizacaoId, LayoutExportacaoFolhaPagamento layout, List<LayoutExportacaoFolhaPagamentoCampo> campos);

        List<MapeamentoEventoFolhaPagamento> ObtenhaMapeamentos(int organizacaoId);
        void SalvarMapeamentos(int organizacaoId, List<MapeamentoEventoFolhaPagamento> mapeamentos);

        ResultadoExportacaoFolhaPagamentoDTO GerarDadosPacote(
            int organizacaoId,
            int unidadeId,
            MesAno mesAno,
            int layoutId,
            eFormatoArquivoExportacao formatoArquivo,
            bool agruparPorMatricula,
            bool somenteServidoresHabilitados);
    }
}