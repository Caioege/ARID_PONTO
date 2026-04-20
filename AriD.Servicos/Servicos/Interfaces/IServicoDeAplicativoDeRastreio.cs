using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeAplicativoDeRastreio
    {
        AutenticacaoAppDTO AutenticarUsuario(CredenciaisDTO credenciais);
        void RegistrarToken(int servidorId, string token, string plataforma);
        
        List<RotaCheckListDTO> ObterRotasMotorista(int motoristaId);
        List<RotaCheckListDTO> ObterRotasAcompanhante(int servidorId);
        
        dynamic ObterUltimaLocalizacao(int rotaId);
        IEnumerable<dynamic> ObterTrajeto(int rotaId, DateTime data);
        
        List<VeiculoCheckListDTO> ObterVeiculosChecklist(int rotaId);
        void SalvarChecklist(ChecklistPostDTO dto, int motoristaId);
        
        RotaExecucaoDTO IniciarRota(IniciarRotaAppDTO dto, int motoristaId);
        RotaExecucaoDTO? ObterRotaEmAndamento(int motoristaId);
        void EncerrarRota(EncerrarRotaAppDTO dto);
        void ConfirmarParada(ConfirmarParadaAppDTO dto);
        void SalvarPonto(PostLocalizacaoExecucaoDTO dto);
        void ReceberLocalizacao(PostLocalizacaoRotaDTO dto);
        
        void FazerPausa(PausaRotaAppDTO dto);
        void FinalizarPausa(PausaRotaAppDTO dto);
        
        int ObterMotoristaIdPorServidor(int servidorId);
    }
}
