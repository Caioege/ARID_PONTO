using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class AutenticacaoController : BaseController
    {
        private readonly IServico<Usuario> _servico;
        private readonly IServicoDeAplicativo _servicoDeAplicativo;

        public AutenticacaoController(
            IServico<Usuario> servico, IServicoDeAplicativo servicoDeAplicativo)
        {
            _servico = servico;
            _servicoDeAplicativo = servicoDeAplicativo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (this.EstaAutenticado())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Entrar([FromBody] CredenciaisDTO credenciais)
        {
            var usuarioAcesso = _servico.Obtenha(c => c.UsuarioDeAcesso.Equals(credenciais.Usuario));
            if (usuarioAcesso != null)
            {
                if (!usuarioAcesso.Ativo ||
                    !usuarioAcesso.Senha.Equals(Criptografia.CriptografarSenha(credenciais.Senha)))
                {
                    throw new ApplicationException("Usuário ou senha incorretos.");
                }

                this.Autenticar(new SessaoDTO(
                    usuarioAcesso.Id,
                    usuarioAcesso.NomeDaPessoa,
                    usuarioAcesso.PerfilDeAcesso,
                    usuarioAcesso.OrganizacaoId ?? 0,
                    usuarioAcesso.Organizacao?.Nome ?? "ADMINISTRAÇÃO DO SISTEMA",
                    usuarioAcesso.PerfilDeAcesso == ePerfilDeAcesso.UnidadeOrganizacional ? [usuarioAcesso.UnidadeOrganizacionalId.Value] : [],
                    usuarioAcesso.DepartamentoId,
                    ObtenhaListaDePermissoes(usuarioAcesso),
                    usuarioAcesso.Organizacao?.NomenclaturaServidor ?? eNomenclaturaServidor.Servidores,
                    usuarioAcesso.PerfilDeAcesso == ePerfilDeAcesso.AdministradorDeSistema,
                    usuarioAcesso.Organizacao?.GestaoMobileAtivo ?? false));

                return Json(new { sucesso = true, mensagem = "O acesso foi feito com sucesso." });
            }

            if (usuarioAcesso == null)
            {
                var acesso = _servicoDeAplicativo.AutenticarUsuario(credenciais);
                if (acesso != null)
                {
                    this.Autenticar(new SessaoDTO(
                        acesso.ServidorId,
                        acesso.ServidorNome,
                        ePerfilDeAcesso.Servidor,
                        acesso.OrganizacaoId,
                        acesso.OrganizacaoNome,
                        [],
                        null,
                        [],
                        eNomenclaturaServidor.Servidores,
                        false));

                    return Json(new { sucesso = true, mensagem = "O acesso foi feito com sucesso." });
                }
            }

            throw new ApplicationException("Usuário ou senha incorretos.");
        }

        [HttpGet]
        public IActionResult Sair()
        {
            this.HttpContext?.Session?.Clear();
            return RedirectToAction("Index");
        }

        private List<KeyValuePair<string, int>> ObtenhaListaDePermissoes(Usuario usuario)
        {
            if (usuario.GrupoDePermissaoId.HasValue)
            {
                Assembly assembly = Assembly.Load("AriD.BibliotecaDeClasses");

                return usuario
                    .GrupoDePermissao
                    .ListaDePermissao
                    .Where(c => c.PermissaoAtiva)
                    .Select(c => new KeyValuePair<string, int>(c.EnumeradorNome, c.ValorDoEnumerador))
                    .ToList();
            }
            else
                return new();
        }
    }
}