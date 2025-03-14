using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeRelatorios : IDisposable
    {
        List<RelatorioAfastamentODTO> ObtenhaAfastamentosParaRelatorio(
            int redeDeEnsinoId,
            int? escolaId,
            DateTime? inicio,
            DateTime? fim,
            int? justificativaId);

        List<ItemRelatorioServidorPorHorarioDTO> ObtenhaServidoresPorHorario(
            int redeDeEnsinoId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId);
    }
}
