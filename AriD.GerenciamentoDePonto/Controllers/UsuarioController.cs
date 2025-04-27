using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly IServico<Usuario> _servico;
        private readonly IServico<GrupoDePermissao> _servicoGrupo;
        private readonly IServico<Departamento> _servicoDepartamentos;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidadeOrganizacional;

        public UsuarioController(
            IServico<Usuario> funcaoServico,
            IServico<GrupoDePermissao> servicoGrupo,
            IServico<Departamento> servicoDepartamentos,
            IServico<UnidadeOrganizacional> servicoUnidadeOrganizacional)
        {
            _servico = funcaoServico;
            _servicoGrupo = servicoGrupo;
            _servicoDepartamentos = servicoDepartamentos;
            _servicoUnidadeOrganizacional = servicoUnidadeOrganizacional;
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
            var dadosDaSessao = HttpContext.DadosDaSessao();
            var model = usuarioId == 0 ?
                new Usuario { Ativo = true, PerfilDeAcesso = dadosDaSessao.Perfil } :
                _servico.Obtenha(usuarioId);

            if (usuarioId > 0)
            {
                ViewBag.Grupos = new SelectList(ObtenhaGrupos(model.PerfilDeAcesso), "Codigo", "Descricao", model.GrupoDePermissaoId);
            }
            else
            {
                if (dadosDaSessao.Perfil != ePerfilDeAcesso.AdministradorDeSistema)
                    ViewBag.Grupos = new SelectList(ObtenhaGrupos(model.PerfilDeAcesso), "Codigo", "Descricao");

                ViewBag.Departamentos = new SelectList(
                    _servicoDepartamentos.ObtenhaLista(c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId && c.Ativo).OrderBy(c => c.Descricao),
                    "Id",
                    "SiglaComDescricao");

                ViewBag.Unidades = new SelectList(
                        _servicoUnidadeOrganizacional
                        .ObtenhaLista(c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId && c.Ativa)
                        .OrderBy(c => c.Nome),
                    "Id",
                    "Nome");
            }

            var html = await RenderizarComoString("_Modal", model);

            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public IActionResult Salvar(Usuario usuario)
        {
            int id = usuario.Id;
            var dadosDaSessao = HttpContext.DadosDaSessao();

            if (usuario.PerfilDeAcesso != ePerfilDeAcesso.AdministradorDeSistema)
                usuario.OrganizacaoId = dadosDaSessao.OrganizacaoId;
            else
                usuario.OrganizacaoId = null;

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

        private IEnumerable<CodigoDescricaoDTO> ObtenhaGrupos(ePerfilDeAcesso perfil)
        {
            var organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;

            return _servicoGrupo
                .ObtenhaLista(c => c.PerfilDeAcesso == perfil && c.OrganizacaoId == organizacaoId && c.Ativo)
                .OrderBy(c => c.Descricao)
                .Select(c => new CodigoDescricaoDTO(c.Id, c.SiglaComDescricao));
        }
    }
}