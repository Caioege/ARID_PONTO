using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
                var model = usuarioId == 0 ?
                    new Usuario { Ativo = true } :
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
                usuario.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                if (!string.IsNullOrEmpty(usuario.Senha))
                    usuario.Senha = Criptografia.CriptografarSenha(usuario.Senha);

                if (usuario.Id == 0)
                    id = _servico.Adicionar(usuario);
                else
                    _servico.Atualizar(usuario);

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

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            var dados = _servico.ObtenhaListaPaginada(
                c => c.OrganizacaoId == parametros.OrganizacaoId, 
                listaPaginada.Pagina, 
                listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}