using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeAplicativoDeRastreio
    {
        AutenticacaoAppDTO AutenticarUsuario(CredenciaisDTO credenciais);
        void RegistrarToken(int servidorId, string token, string plataforma);

        List<RotaCheckListDTO> ObterRotasMotorista(int motoristaId);
        PacoteOfflineRastreioDTO ObterPacoteOfflineMotorista(int motoristaId);
        ResultadoSincronizacaoRotaOfflineDTO SincronizarRotaOffline(SincronizarRotaOfflineDTO dto, int motoristaId);
        List<RotaCheckListDTO> ObterRotasAcompanhante(int servidorId);

        UltimaLocalizacaoRotaDTO? ObterUltimaLocalizacao(int rotaId);
        IEnumerable<UltimaLocalizacaoRotaDTO> ObterTrajeto(int rotaId, DateTime data);
        IEnumerable<UltimaLocalizacaoRotaDTO> ObterTrajetoExecucao(int rotaExecucaoId, int motoristaId);

        List<VeiculoCheckListDTO> ObterVeiculosChecklist(int rotaId);
        int SalvarChecklist(ChecklistPostDTO dto, int motoristaId);

        RotaExecucaoDTO IniciarRota(IniciarRotaAppDTO dto, int motoristaId);
        RotaExecucaoDTO? ObterRotaEmAndamento(int motoristaId);
        RotaExecucaoDTO? ObterRotaFinalizadaDoDia(int motoristaId, int rotaId);
        void EncerrarRota(EncerrarRotaAppDTO dto, int motoristaId);
        void ConfirmarParada(ConfirmarParadaAppDTO dto, int motoristaId);
        void SalvarPonto(PostLocalizacaoExecucaoDTO dto, int motoristaId);
        void ReceberLocalizacao(PostLocalizacaoRotaDTO dto);

        void FazerPausa(PausaRotaAppDTO dto, int motoristaId);
        void FinalizarPausa(PausaRotaAppDTO dto, int motoristaId);

        int ObterMotoristaIdPorServidor(int servidorId);
    }
}
