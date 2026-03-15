using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;

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

        List<RelatorioDemitidosDTO> ObtenhaServidoresDemitidosPorPeriodo(
            int organizacaoId,
            int? unidadeId,
            DateTime? inicio,
            DateTime? fim,
            int? motivoDeDemissaoId,
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

        List<VinculoDeTrabalho> ObtenhaListaDeVinculos(
            int organizacaoId,
            int unidadeId,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? departamentoId);

        List<EventoAnual> ObtenhaListaDeEventosDaOrganizacao(int organizacaoId);

        List<RelatorioConferenciaDePontoDTO> ObtenhaListaDeDadosParaConferenciaDePonto(
            int organizacaoId,
            int unidadeOrganizacionalId,
            DateTime data,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? departamentoId);

        List<RelatorioServidorPorLotacaoDTO> ObtenhaListaDeDadosPorLotacao(
            int organizacaoId,
            int? unidadeOrganizacionalId,
            DateTime? entrada,
            int? horarioDeTrabalhoId,
            int? tipoDeVinculoDeTrabalhoId,
            int? departamentoId);

        List<RelatorioAbsenteismoDTO> ObtenhaRelatorioDeAbsenteismo(
            int organizacaoId,
            int? unidadeId,
            DateTime inicio,
            DateTime fim,
            int? departamentoId);

        List<RelatorioAuditoriaDeAusenciasDTO> ObtenhaRelatorioDeAuditoriaDeAusencias(
            int organizacaoId,
            DateTime? inicio,
            DateTime? fim,
            int? unidadeLotacaoId);
    }
}