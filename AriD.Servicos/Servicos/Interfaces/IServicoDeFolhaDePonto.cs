using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeFolhaDePonto : IServico<PontoDoDia>
    {
        (List<CodigoDescricaoDTO> Horarios, List<CodigoDescricaoDTO> Funcoes, List<CodigoDescricaoDTO> Departamentos) ObtenhaFiltrosPontoDia(
            int organizacaoId,
            int unidadeOrganizacionalId);

        List<PontoDoDia> ObtenhaPontosDoDia(
            DateTime data,
            int organizacaoId,
            int unidadeOrganizacionalId,
            int horarioDeTrabalhoId,
            int? funcaoId,
            int? departamentoId);

        PontoDoDia ObtenhaPontoDoDia(int vinculoDeTrabalhoId, DateTime data);

        PontoDoDia AtualizePontoDoDia(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            DateTime data,
            TimeSpan? valorHora,
            int? justificativaId,
            string acao,
            string motivoAcao,
            bool desconsideraRegistroAtual);

        List<CodigoDescricaoDTO> ObtenhaServidoresLotadosNaUnidade(
            int organizacaoId,
            int unidadeId,
            int? departamentoId);

        List<CodigoDescricaoDTO> ObtenhaVinculosDeTrabalhoDoServido(
            int organizacaoId,
            int servidorId,
            int unidadeId,
            int? departamentoId);

        List<PontoDoDia> CarregueFolhaDePonto(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            int unidadeLotacaoId,
            MesAno mesAno);

        List<EventoAnual> EventosDaFolhaDePonto(
            int organizacaoId,
            DateTime inicio,
            DateTime fim);

        void FecharOuAbrirFolhaDePonto(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            MesAno mesAno,
            int unidadeLotacaoId,
            bool fechar);

        List<CodigoDescricaoDTO> ObtenhaListaDeUnidadesLotadasNoDepartamento(
            int organizacaoId,
            int departamentoId);

        void MovimentarRegistro(
            int id,
            string classe,
            bool avancar);

        void ResetarFolhaDePonto(
            int organizacaoId,
            int vinculoDeTrabalhoId,
            int unidadeLotacaoId,
            MesAno mesAno);

        List<RegistroAplicativo> ObtenhaRegistrosDeAplicativo(int vinculoId, MesAno periodo);

        public void AprovarRegistroAplicativo(int registroId, int unidadeLotacaoId, MesAno mesAno);
        void ReprovarRegistroAplicativo(int registroId);

        string ObtenhaObservacaoDoServidorNaFolhaDePonto(int vinculoDeTrabalhoId);

        List<PontoDoDiaHoraExtra> ObtenhaHorasExtrasDoDia(int pontoDoDiaId);
        void AprovarHoraExtra(int horaExtraId, int minutosAprovados);
        void ReprovarHoraExtra(int horaExtraId);

        List<PontoDoDiaHoraExtra> HorasExtrasDaFolhaDePonto(int organizacaoId, int vinculoDeTrabalhoId, DateTime inicio, DateTime fim);

        List<LogAuditoriaPonto> ObtenhaAuditoriaDaFolha(int organizacaoId, int vinculoDeTrabalhoId, MesAno mesAno);
        List<LogAuditoriaPonto> ObtenhaAuditoriaDoDia(int organizacaoId, int pontoDoDiaId);

        void ReconsiderarRegistroDePonto(int organizacaoId, int pontoDoDiaId, int registroDePontoId);
    }
}