using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class SessaoDTO
    {
        public SessaoDTO(
            int usuarioId,
            string usuarioNome,
            ePerfilDeAcesso perfil,
            int redeDeEnsinoId,
            string redeDeEnsinoNome,
            int? escolaId,
            List<KeyValuePair<string, int>> permissoes,
            bool usuarioAdministradorAutenticado = false)
        {
            UsuarioId = usuarioId;
            UsuarioNome = usuarioNome;
            Perfil = perfil;
            RedeDeEnsinoId = redeDeEnsinoId;
            EscolaId = escolaId;
            RedeDeEnsinoNome = redeDeEnsinoNome;
            Permissoes = permissoes;
            UsuarioAdministradorAutenticado = usuarioAdministradorAutenticado;
        }

        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }

        public ePerfilDeAcesso Perfil { get; set; }

        public int RedeDeEnsinoId { get; set; }
        public string RedeDeEnsinoNome { get; set; }

        public bool UsuarioAdministradorAutenticado { get; set; }

        public int? EscolaId { get; set; }
        public List<KeyValuePair<string, int>> Permissoes { get; set; } = new();
    }
}
