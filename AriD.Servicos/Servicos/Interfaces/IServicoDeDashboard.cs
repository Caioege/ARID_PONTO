using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeDashboard
    {
        DashboardDTO ObtenhaDashboardDTO(
            int organizacaoId,
            int? unidadeId);
    }
}