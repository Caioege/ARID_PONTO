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
            int? justificativaId,
            int? departamentoId);

        List<ItemRelatorioServidorPorHorarioDTO> ObtenhaServidoresPorHorario(
            int organizacaoId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? unidadeId,
            int? departamentoId);

        List<ItemRelatorioServidorPorEscalaDTO> ObtenhaServidoresPorEscala(
            int organizacaoId,
            int? escalaId,
            int? departamentoId);

        List<RelatorioItemListaServidorDTO> ObtenhaListaDeServidores(
            int organizacaoId,
            int? unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? departamentoId);
    }
}