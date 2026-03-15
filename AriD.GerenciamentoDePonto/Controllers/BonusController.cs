using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.Enumeradores.Permissao;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Extensao;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class BonusController : Controller
    {
        private readonly IServico<ConfiguracaoBonus> _servicoConfiguracaoBonus;
        private readonly IServicoBonus _servicoBonus;
        private readonly IServico<Servidor> _servicoServidor;

        public BonusController(
            IServico<ConfiguracaoBonus> servicoConfiguracaoBonus,
            IServicoBonus servicoBonus,
            IServico<Servidor> servicoServidor)
        {
            _servicoConfiguracaoBonus = servicoConfiguracaoBonus;
            _servicoBonus = servicoBonus;
            _servicoServidor = servicoServidor;
        }

        [HttpGet]
        public IActionResult Index(int pagina = 1)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.Gerenciar)) return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            int organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
            var resultado = _servicoConfiguracaoBonus.ObtenhaListaPaginada(c => c.OrganizacaoId == organizacaoId && c.Ativo, pagina, 50);

            var viewModel = new ListaPaginada<ConfiguracaoBonus>()
            {
                Pagina = pagina,
                QuantidadeDeItensPorPagina = 50,
                TotalDeItens = resultado.Total,
                Itens = resultado.Itens
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.CadastrarOuAlterar)) return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");
            return View(new ConfiguracaoBonus());
        }

        [HttpPost]
        public IActionResult Cadastrar(ConfiguracaoBonus config)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.CadastrarOuAlterar)) return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            config.OrganizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
            config.Ativo = true;
            _servicoConfiguracaoBonus.Adicionar(config);
            _servicoConfiguracaoBonus.Commit();
            return RedirectToAction("Index");
        }

        // Action para rodar o cálculo (pode ser chamada via AJAX de uma tela de "Processar Mês")
        [HttpPost]
        public IActionResult ProcessarCalculoDoMes(int configuracaoBonusId, string mesReferencia)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.Gerenciar)) return Json(new { sucesso = false, mensagem = "Sem permissão." });

            int orgId = HttpContext.DadosDaSessao().OrganizacaoId;
            
            // Pega todos os servidores da org (stub: buscar na base para ter os VinculoDeTrabalhoId)
            var vinculosIds = _servicoServidor.ObtenhaLista(s => s.OrganizacaoId == orgId).Select(s => s.Id).ToList();

            try
            {
                _servicoBonus.GerarBonusDoMes(orgId, configuracaoBonusId, mesReferencia, vinculosIds);
                return Json(new { sucesso = true, mensagem = "Bônus calculado com sucesso!" });
            }
            catch(Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }
    }
}
