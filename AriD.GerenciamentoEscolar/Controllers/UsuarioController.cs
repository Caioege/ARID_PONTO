using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoEscolar.Helpers;
using AriD.GerenciamentoEscolar.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly IServico<Usuario> _servico;
        private readonly IServico<GrupoDePermissao> _servicoGrupo;
        private readonly IServico<Escola> _servicoEscola;

        public UsuarioController(
            IServico<Usuario> funcaoServico,
            IServico<GrupoDePermissao> servicoGrupo,
            IServico<Escola> servicoEscola)
        {
            _servico = funcaoServico;
            _servicoGrupo = servicoGrupo;
            _servicoEscola = servicoEscola;
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

                if (usuarioId > 0)
                {
                    ViewBag.Grupos = new SelectList(ObtenhaGrupos(model.PerfilDeAcesso), "Codigo", "Descricao");
                }
                else
                {
                    if (dadosDaSessao.Perfil != ePerfilDeAcesso.AdministradorDeSistema)
                        ViewBag.Grupos = new SelectList(ObtenhaGrupos(model.PerfilDeAcesso), "Codigo", "Descricao");

                    ViewBag.Escolas = new SelectList(
                        _servicoEscola
                            .ObtenhaLista(c => c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId && c.Ativa)
                            .OrderBy(c => c.Nome),
                        "Id",
                        "Nome");
                }

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

                if (dadosDaSessao.Perfil != ePerfilDeAcesso.AdministradorDeSistema)
                    usuario.RedeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;
                else
                    usuario.RedeDeEnsinoId = null;

                if (dadosDaSessao.Perfil == ePerfilDeAcesso.Escola)
                {
                    usuario.PerfilDeAcesso = ePerfilDeAcesso.Escola;
                    usuario.EscolaId = dadosDaSessao.EscolaId;
                }

                if (!string.IsNullOrEmpty(usuario.Senha))
                    usuario.Senha = Criptografia.CriptografarSenha(usuario.Senha);

                if (usuario.Id == 0)
                    id = _servico.Adicionar(usuario);
                else
                {
                    var persistido = _servico.Obtenha(usuario.Id);

                    persistido.NomeDaPessoa = usuario.NomeDaPessoa;
                    persistido.UsuarioDeAcesso = usuario.UsuarioDeAcesso;
                    persistido.Ativo = usuario.Ativo;
                    persistido.GrupoDePermissaoId = usuario.GrupoDePermissaoId;

                    if (!string.IsNullOrEmpty(usuario.Senha))
                        persistido.Senha = usuario.Senha;

                    _servico.Atualizar(persistido);
                }

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Ocorreu um erro." });
            }
        }

        [HttpGet]
        public ActionResult CarregueListaDeGruposDePermissao(ePerfilDeAcesso perfil)
        {
            try
            {
                
                var gruposDePermissao = ObtenhaGrupos(perfil);
                return Json(new { sucesso = true, gruposDePermissao });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Ocorreu um erro." });
            }
        }

        [HttpPost]
        public ActionResult Remover(int id)
        {
            var usuario = _servico.Obtenha(id);
            _servico.Remover(usuario);
            return Json(new { sucesso = true, mensagem = "O usuário foi removido." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Usuario> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaescolasOrganizacionais>(listaPaginada.Adicional);

            var dadosDaSessao = HttpContext.DadosDaSessao();
            parametros.RedeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;

            Expression<Func<Usuario, bool>> filtro = c => true;

            if (dadosDaSessao.Perfil == ePerfilDeAcesso.AdministradorDeSistema)
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro, 
                    c => c.PerfilDeAcesso == ePerfilDeAcesso.AdministradorDeSistema && !c.RedeDeEnsinoId.HasValue);
            }
            else if (dadosDaSessao.Perfil == ePerfilDeAcesso.RedeDeEnsino)
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.PerfilDeAcesso != ePerfilDeAcesso.AdministradorDeSistema && c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId);
            }
            else
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.PerfilDeAcesso == ePerfilDeAcesso.Escola && c.RedeDeEnsinoId == dadosDaSessao.RedeDeEnsinoId && c.EscolaId == dadosDaSessao.EscolaId);
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

        private IEnumerable<CodigoDescricaoDTO> ObtenhaGrupos(ePerfilDeAcesso perfil)
        {
            var redeDeEnsinoId = HttpContext.DadosDaSessao().RedeDeEnsinoId;

            return _servicoGrupo
                .ObtenhaLista(c => c.PerfilDeAcesso == perfil && c.RedeDeEnsinoId == redeDeEnsinoId && c.Ativo)
                .OrderBy(c => c.Descricao)
                .Select(c => new CodigoDescricaoDTO(c.Id, c.SiglaComDescricao));
        }
    }
}
