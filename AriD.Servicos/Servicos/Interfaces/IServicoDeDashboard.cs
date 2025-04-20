using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeDashboard
    {
        DashboardDTO ObtenhaDashboardDTO(
            int redeDeEnsinoId,
            int? escolaId);
    }
}