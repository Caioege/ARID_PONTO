using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IWhatsappService
    {
        Task SendMessageAsync(string phone, string message);

        Task EnviarComprovantePontoAsync(
            Servidor servidor,
            int nsr,
            DateTime dataHora);
    }
}