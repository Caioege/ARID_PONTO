using AriD.BibliotecaDeClasses.DTO.Aplicativo;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeAplicativo
    {
        AutenticacaoAppDTO AutenticarUsuario(CredenciaisDTO credenciais);
        List<TelaHorarioDeTrabalhoAppDTO> ObtenhaHorariosDoServidor(int servidorId);
        List<EventoAppDTO> ObtenhaListaDeEventos(int organizacaoId);
        List<CodigoDescricaoDTO> ObtenhaListaDeVinculos(int servidorId);
        List<CodigoDescricaoDTO> ObtenhaListaDeLotacoes(int vinculoId);
        List<KeyValuePair<DateTime, string>> ObtenhaUltimosRegistrosDoServidor(int servidorId);
        List<CodigoDescricaoDTO> ObtenhaListaDeJustificativas(int organizacaoId);
        void ReceptarRegistro(PostRegistroDePontoDTO registro, bool fromApp);
        void RegistrarToken(RegistrarTokenDTO registrarToken);
        void AlterarSenha(int servidorId, string senhaAtual, string novaSenha);
    }
}