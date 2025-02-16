using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly IServico<Usuario> _servico;

        public UsuarioController(
            IServico<Usuario> funcaoServico)
        {
            _servico = funcaoServico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Usuario> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Usuario> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View("_TabelaPaginada", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Modal(int usuarioId)
        {
            try
            {
                var dadosDaSessao = HttpContext.DadosDaSessao();
                var model = usuarioId == 0 ?
                    new Usuario { Ativo = true, PerfilDeAcesso = dadosDaSessao.Perfil } :
                    _servico.Obtenha(usuarioId);

                var html = await RenderizarComoString("_Modal", model);

                return Json(new { sucesso = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Salvar(Usuario usuario)
        {
            try
            {
                int id = usuario.Id;
                var dadosDaSessao = HttpContext.DadosDaSessao();

                if (usuario.PerfilDeAcesso != ePerfilDeAcesso.AdministradorDeSistema)
                    usuario.OrganizacaoId = dadosDaSessao.OrganizacaoId;
                else
                    usuario.OrganizacaoId = null;

                usuario.PerfilDeAcesso = dadosDaSessao.Perfil;

                if (!string.IsNullOrEmpty(usuario.Senha))
                    usuario.Senha = Criptografia.CriptografarSenha(usuario.Senha);

                if (usuario.Id == 0)
                    id = _servico.Adicionar(usuario);
                else
                {
                    var persistido = _servico.Obtenha(usuario.Id);

                    persistido.NomeDaPessoa = usuario.NomeDaPessoa;
                    persistido.UsuarioDeAcesso = usuario.UsuarioDeAcesso;
                    persistido.PerfilDeAcesso = usuario.PerfilDeAcesso;
                    persistido.Ativo = usuario.Ativo;

                    if (!string.IsNullOrEmpty(usuario.Senha))
                        persistido.Senha = usuario.Senha;

                    _servico.Atualizar(persistido);
                }

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = true, mensagem = "Ocorreu um erro." });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Usuario> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            var dadosDaSessao = HttpContext.DadosDaSessao();
            parametros.OrganizacaoId = dadosDaSessao.OrganizacaoId;

            Expression<Func<Usuario, bool>> filtro = c => true;

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.AdministradorDeSistema)
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro, 
                    c => c.PerfilDeAcesso == ePerfilDeAcesso.AdministradorDeSistema && !c.OrganizacaoId.HasValue);
            }
            else if (dadosDaSessao.Perfil == ePerfilDeAcesso.Organizacao)
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.PerfilDeAcesso != ePerfilDeAcesso.AdministradorDeSistema && c.OrganizacaoId == dadosDaSessao.OrganizacaoId);
            }
            else
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.PerfilDeAcesso == ePerfilDeAcesso.UnidadeOrganizacional && c.OrganizacaoId == dadosDaSessao.OrganizacaoId);
            }

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => (c.NomeDaPessoa.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) || c.UsuarioDeAcesso.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower())));

            var dados = _servico.ObtenhaListaPaginada(
                filtro, 
                listaPaginada.Pagina, 
                listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}