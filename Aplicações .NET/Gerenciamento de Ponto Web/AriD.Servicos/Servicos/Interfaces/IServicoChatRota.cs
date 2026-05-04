using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoChatRota
    {
        RotaChatResumoDTO ObterChat(int organizacaoId, int rotaExecucaoId);
        RotaChatResumoDTO ObterChatAplicativo(int rotaExecucaoId, int servidorId);
        RotaChatMensagemDTO EnviarMensagemSistema(int organizacaoId, int rotaExecucaoId, int usuarioId, string usuarioNome, string mensagem);
        RotaChatMensagemDTO EnviarMensagemAplicativo(int rotaExecucaoId, int servidorId, string mensagem);
        RotaChatPushDestinoDTO? ObterDestinoPushAplicativo(int organizacaoId, int rotaExecucaoId);
        List<RotaChatNaoLidasDTO> ObterNaoLidasSistema(int organizacaoId, List<int> rotaExecucaoIds);
        int ObterNaoLidasAplicativo(int rotaExecucaoId, int servidorId);
    }
}
