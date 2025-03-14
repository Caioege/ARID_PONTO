using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class SessaoDTO
    {
        public SessaoDTO(
            int usuarioId,
            string usuarioNome,
            ePerfilDeAcesso perfil,
            int organizacaoId,
            string organizacaoNome,
            List<int> unidades,
            List<KeyValuePair<string, int>> permissoes,
            bool usuarioAdministradorAutenticado = false)
        {
            UsuarioId = usuarioId;
            UsuarioNome = usuarioNome;
            Perfil = perfil;
            OrganizacaoId = organizacaoId;
            UnidadeOrganizacionais = unidades;
            OrganizacaoNome = organizacaoNome;
            Permissoes = permissoes;
            UsuarioAdministradorAutenticado = usuarioAdministradorAutenticado;
        }

        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }

        public ePerfilDeAcesso Perfil { get; set; }

        public int OrganizacaoId { get; set; }
        public string OrganizacaoNome { get; set; }

        public bool UsuarioAdministradorAutenticado { get; set; }

        public int? DepartamentoId { get; set; }

        public List<int> UnidadeOrganizacionais { get; set; } = new();
        public List<KeyValuePair<string, int>> Permissoes { get; set; } = new();
    }
}