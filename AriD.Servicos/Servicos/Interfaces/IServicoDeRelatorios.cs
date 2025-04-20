using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeRelatorios : IDisposable
    {
        List<RelatorioAlunosDaEscolaDTO> ObtenhaAlunosDaEscola(
            int redeDeEnsinoId,
            int? escolaId);

        List<RelatorioFrequenciaNaDataDTO> ObtenhaFrequenciaNaData(
            int redeDeEnsinoId,
            int escolaId,
            DateTime data);

        List<RelatorioEquipamentoDaEscolaDTO> ObtenhaEquipamentosDaEscola(
            int redeDeEnsinoId,
            int? escolaId);

        List<RegistroDePontoIndexDTO> ObtenhaRegistrosDeFrequencia(
            int redeDeEnsinoId,
            int? escolaId,
            DateTime dataInicio,
            DateTime dataFim);
    }
}