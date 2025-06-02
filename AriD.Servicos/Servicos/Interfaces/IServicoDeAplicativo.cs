using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeAplicativo
    {
        AutenticacaoAppDTO AutenticarUsuario(CredenciaisDTO credenciais);
        List<TelaHorarioDeTrabalhoAppDTO> ObtenhaHorariosDoServidor(int servidorId);
        List<EventoAppDTO> ObtenhaListaDeEventos(int organizacaoId);
        List<CodigoDescricaoDTO> ObtenhaListaDeVinculos(int servidorId);
        List<CodigoDescricaoDTO> ObtenhaListaDeLotacoes(int vinculoId);
    }
}