using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeRelatorios : IDisposable
    {
        List<RelatorioAfastamentODTO> ObtenhaAfastamentosParaRelatorio(
            int organizacaoId,
            int? unidadeId,
            DateTime? inicio,
            DateTime? fim,
            int? justificativaId);

        List<ItemRelatorioServidorPorHorarioDTO> ObtenhaServidoresPorHorario(
            int organizacaoId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId);
    }
}