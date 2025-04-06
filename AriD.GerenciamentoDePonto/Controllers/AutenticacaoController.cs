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

        public AutenticacaoController(
            IServico<Usuario> servico)
        {
            _servico = servico;
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

            if (usuarioAcesso == null ||
                !usuarioAcesso.Ativo ||
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
                usuarioAcesso.PerfilDeAcesso == ePerfilDeAcesso.UnidadeOrganizacional ?[usuarioAcesso.UnidadeOrganizacionalId.Value] : [],
                usuarioAcesso.DepartamentoId,
                ObtenhaListaDePermissoes(usuarioAcesso),
                usuarioAcesso.PerfilDeAcesso == ePerfilDeAcesso.AdministradorDeSistema));

            return Json(new { sucesso = true, mensagem = "O acesso foi feito com sucesso." });
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