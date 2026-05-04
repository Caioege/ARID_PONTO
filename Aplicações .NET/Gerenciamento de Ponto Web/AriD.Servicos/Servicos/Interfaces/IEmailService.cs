using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IEmailService
    {
        Task EnviarComprovantePontoAsync(Servidor servidor, int nsr, DateTime dataHora);
        Task EnviarNotificacaoConectividadeAsync(string emailDestinatario, string nomeEntidade, List<EquipamentoConectividadeInfo> equipamentos);
    }
}