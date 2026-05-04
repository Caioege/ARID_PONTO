namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoNotificacao
    {
        /// <summary>
        /// Envia uma notificação push para um usuário específico.
        /// </summary>
        Task<bool> EnviarNotificacaoPush(string pushToken, string titulo, string mensagem, object dados = null);

        /// <summary>
        /// Envia uma notificação push para vários tokens simultaneamente.
        /// </summary>
        Task<bool> EnviarNotificacaoPush(List<string> pushTokens, string titulo, string mensagem, object dados = null);
    }
}
