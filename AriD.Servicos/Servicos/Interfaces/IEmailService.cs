using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IEmailService
    {
        Task EnviarComprovantePontoAsync(Servidor servidor, int nsr, DateTime dataHora);
    }
}