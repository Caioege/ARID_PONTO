using AriD.BibliotecaDeClasses.Entidades;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Extensao;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class FiltroRelatorioController : BaseController
    {
        private readonly IServico<FiltroRelatorio> _servico;

        public FiltroRelatorioController(IServico<FiltroRelatorio> servico)
        {
            _servico = servico;
        }

        [HttpPost]
        public IActionResult Salvar(string nome, string urlRelatorio, string jsonFiltros, bool compartilhado)
        {
            try
            {
                var sessao = HttpContext.DadosDaSessao();
                
                var filtro = new FiltroRelatorio
                {
                    OrganizacaoId = sessao.OrganizacaoId,
                    UsuarioCriadorId = sessao.UsuarioId,
                    Nome = nome,
                    UrlRelatorio = urlRelatorio,
                    JsonParametros = jsonFiltros,
                    Compartilhado = compartilhado
                };

                _servico.Adicionar(filtro);
                _servico.Commit();

                return Json(new { sucesso = true, mensagem = "Filtro salvo com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ObtenhaFiltros(string urlRelatorio)
        {
            try
            {
                var sessao = HttpContext.DadosDaSessao();
                var filtros = _servico.ObtenhaLista(f => 
                    f.OrganizacaoId == sessao.OrganizacaoId &&
                    f.UrlRelatorio == urlRelatorio &&
                    (f.Compartilhado || f.UsuarioCriadorId == sessao.UsuarioId))
                    .OrderBy(f => f.Nome)
                    .Select(f => new { f.Id, f.Nome, f.JsonParametros })
                    .ToList();

                return Json(new { sucesso = true, dados = filtros });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Excluir(int id)
        {
            try
            {
                var sessao = HttpContext.DadosDaSessao();
                var filtro = _servico.Obtenha(id);

                if (filtro == null) throw new Exception("Filtro não encontrado.");
                if (filtro.UsuarioCriadorId != sessao.UsuarioId && sessao.Perfil != AriD.BibliotecaDeClasses.Enumeradores.ePerfilDeAcesso.AdministradorDeSistema)
                    throw new Exception("Você não tem permissão para excluir este filtro.");

                _servico.Remover(filtro);
                _servico.Commit();

                return Json(new { sucesso = true, mensagem = "Filtro excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }
    }
}
