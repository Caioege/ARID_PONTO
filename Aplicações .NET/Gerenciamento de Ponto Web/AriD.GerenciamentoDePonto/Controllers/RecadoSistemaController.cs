using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.Enumeradores.Permissao;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RecadoSistemaController : BaseController
    {
        private readonly IServico<RecadoSistema> _servicoRecadoSistema;
        private readonly IServico<Usuario> _servicoUsuario;

        public RecadoSistemaController(
            IServico<RecadoSistema> servicoRecadoSistema, 
            IServico<Usuario> servicoUsuario)
        {
            _servicoRecadoSistema = servicoRecadoSistema;
            _servicoUsuario = servicoUsuario;
        }

        [HttpGet]
        public async Task<IActionResult> AbrirModalRecados()
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_RecadoSistema.Visualizar))
                throw new Exception("Usuário não possui permissão para visualizar recados.");

            var listaDeRecados = _servicoRecadoSistema.ObtenhaListaPaginada(ObtenhaFiltroRecadoSistema(), 1, 20, c => c.DataHoraCadastro, false);
            var html = await RenderizarComoString("_ModalRecados", listaDeRecados.Itens);

            return Json(new { sucesso = true, html });
        }

        [HttpGet]
        public IActionResult ExistemRecadosNaoLidos()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            var listaDeRecados = _servicoRecadoSistema.ObtenhaListaPaginada(ObtenhaFiltroRecadoSistema(), 1, 20, c => c.DataHoraCadastro, false);
            return Json(new { sucesso = true, temNotificacao = listaDeRecados.Itens.Any(c => !c.ListaDeUsuariosQueLeram.Any(r => r.Id == dadosDaSessao.UsuarioId)) });
        }

        [HttpGet]
        public async Task<IActionResult> CarregarMaisRecados(int pagina)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_RecadoSistema.Visualizar))
                throw new Exception("Usuário não possui permissão para visualizar recados.");

            ViewBag.PaginaAtual = pagina;

            var listaDeRecados = _servicoRecadoSistema.ObtenhaListaPaginada(ObtenhaFiltroRecadoSistema(), pagina, 20, c => c.DataHoraCadastro, false);
            var html = await RenderizarComoString("_PartialRecados", listaDeRecados.Itens);

            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public IActionResult AltereSituacao(int id)
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            var recado = _servicoRecadoSistema.Obtenha(id);
            if (recado.UsuarioId != dadosDaSessao.UsuarioId)
                throw new Exception("Usuário não possui permissão para alterar a situação deste recado.");

            recado.Ativo = !recado.Ativo;
            _servicoRecadoSistema.Atualizar(recado);
            return Json(new { sucesso = true, mensagem = recado.Ativo ? "Recado ativado com sucesso." : "Recado desativado com sucesso." });
        }

        [HttpPost]
        public IActionResult MarqueRecadosComoLido(int pagina)
        {
            var listaDeRecados = _servicoRecadoSistema.ObtenhaListaPaginada(ObtenhaFiltroRecadoSistema(), pagina, 20, c => c.DataHoraCadastro, false);

            var dadosDaSessao = HttpContext.DadosDaSessao();
            var usuario = _servicoUsuario.Obtenha(dadosDaSessao.UsuarioId);

            var alteracoes = false;

            foreach (var recado in listaDeRecados.Itens)
            {
                if (!recado.ListaDeUsuariosQueLeram.Any(r => r.Id == dadosDaSessao.UsuarioId))
                {
                    alteracoes = true;

                    recado.ListaDeUsuariosQueLeram.Add(usuario);
                    _servicoRecadoSistema.Atualizar(recado, false);
                }
            }

            if (alteracoes)
                _servicoUsuario.Commit();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> AbrirModalAdicionarRecado()
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_RecadoSistema.CadastrarOuAlterar))
                throw new Exception("Usuário não possui permissão para adicionar recados.");

            var html = await RenderizarComoString("_ModalAdicionarRecado", null);
            return Json(new { sucesso = true, html });
        }

        [HttpPost]
        public ActionResult Adicionar(string mensagem)
        {
            var recado = new RecadoSistema
            {
                Mensagem = mensagem
            };

            if (string.IsNullOrEmpty(recado.Mensagem) || string.IsNullOrWhiteSpace(recado.Mensagem))
                throw new Exception("A mensagem do recado é obrigatória.");

            var dadosDaSessao = HttpContext.DadosDaSessao();

            recado.DataHoraCadastro = DateTime.Now;
            recado.UsuarioId = dadosDaSessao.UsuarioId;
            recado.OrganizacaoId = dadosDaSessao.OrganizacaoId;
            recado.Ativo = true;
            recado.Mensagem = recado.Mensagem.TrimStart();
            recado.Mensagem = recado.Mensagem.TrimEnd();

            _servicoRecadoSistema.Adicionar(recado);
            return Json(new { sucesso = true, mensagem = "Seu recado foi adicionado." });
        }

        private Expression<Func<RecadoSistema, bool>> ObtenhaFiltroRecadoSistema()
        {
            var dadosDaSessao = HttpContext.DadosDaSessao();
            Expression<Func<RecadoSistema, bool>> filtro = 
                c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId && (c.Ativo || (!c.Ativo && c.UsuarioId == dadosDaSessao.UsuarioId));

            if (dadosDaSessao.DepartamentoId.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => 
                    c.Usuario.PerfilDeAcesso <= ePerfilDeAcesso.Organizacao || c.Usuario.DepartamentoId == dadosDaSessao.DepartamentoId.Value);

            if (dadosDaSessao.UnidadeOrganizacionais.Any())
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c =>
                    c.Usuario.PerfilDeAcesso <= ePerfilDeAcesso.Organizacao && dadosDaSessao.UnidadeOrganizacionais.Contains(c.Usuario.UnidadeOrganizacionalId.Value));

            return filtro;
        }
    }
}