using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeServidor : IDisposable
    {
        void ExecuteAcaoEmLote(int acao, string motivo, SessaoDTO sessao);
    }
}