using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.BibliotecaDeClasses.Enumeradores.Permissao;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.Extensao;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class BonusController : BaseController
    {
        private readonly IServico<ConfiguracaoBonus> _servicoConfiguracaoBonus;
        private readonly IServico<BonusCalculado> _servicoBonusCalculado;
        private readonly IServicoBonus _servicoBonus;
        private readonly IServico<Servidor> _servicoServidor;
        private readonly IServico<Funcao> _servicoFuncao;

        public BonusController(
            IServico<ConfiguracaoBonus> servicoConfiguracaoBonus,
            IServico<BonusCalculado> servicoBonusCalculado,
            IServicoBonus servicoBonus,
            IServico<Servidor> servicoServidor,
            IServico<Funcao> servicoFuncao)
        {
            _servicoConfiguracaoBonus = servicoConfiguracaoBonus;
            _servicoBonusCalculado = servicoBonusCalculado;
            _servicoBonus = servicoBonus;
            _servicoServidor = servicoServidor;
            _servicoFuncao = servicoFuncao;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<ConfiguracaoBonus> listaPaginada)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.Visualizar)) return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            ConfigureDadosDaTabelaPaginada(listaPaginada);
            return View(listaPaginada);
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<ConfiguracaoBonus> listaPaginada)
        {
            ConfigureDadosDaTabelaPaginada(listaPaginada);
            return PartialView("_TabelaPaginada", listaPaginada);
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<ConfiguracaoBonus> listaPaginada)
        {
            int organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
            Expression<Func<ConfiguracaoBonus, bool>> pesquisa = c => c.OrganizacaoId == organizacaoId && c.Ativo;

            if (!string.IsNullOrWhiteSpace(listaPaginada.TermoDeBusca))
            {
                pesquisa = ConcatenadorDeExpressao.Concatenar(pesquisa, c => c.Descricao.Contains(listaPaginada.TermoDeBusca));
            }

            var resultado = _servicoConfiguracaoBonus.ObtenhaListaPaginada(pesquisa, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);
            listaPaginada.Parametros(this, resultado.Itens, resultado.Total, "TabelaPaginada");
        }

        [HttpGet]
        public IActionResult Relatorio(ListaPaginada<BonusCalculado> listaPaginada)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.GerarRelatorio)) return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            ConfigureDadosDaTabelaPaginadaRelatorio(listaPaginada);
            return View(listaPaginada);
        }

        [HttpGet]
        public IActionResult TabelaPaginadaRelatorio(ListaPaginada<BonusCalculado> listaPaginada)
        {
            ConfigureDadosDaTabelaPaginadaRelatorio(listaPaginada);
            return PartialView("_TabelaPaginadaRelatorio", listaPaginada);
        }

        private void ConfigureDadosDaTabelaPaginadaRelatorio(ListaPaginada<BonusCalculado> listaPaginada)
        {
            int organizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
            Expression<Func<BonusCalculado, bool>> pesquisa = c => c.OrganizacaoId == organizacaoId;

            if (!string.IsNullOrWhiteSpace(listaPaginada.TermoDeBusca))
            {
                pesquisa = ConcatenadorDeExpressao.Concatenar(pesquisa, c => c.VinculoDeTrabalho.Servidor.Nome.Contains(listaPaginada.TermoDeBusca) || c.MesReferencia.Contains(listaPaginada.TermoDeBusca));
            }

            var resultado = _servicoBonusCalculado.ObtenhaListaPaginada(pesquisa, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);
            listaPaginada.Parametros(this, resultado.Itens, resultado.Total, "TabelaPaginadaRelatorio");
        }

        [HttpGet]
        public IActionResult Cadastrar(int? id)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.CadastrarOuAlterar)) return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");
            
            int orgId = HttpContext.DadosDaSessao().OrganizacaoId;
            ViewBag.Funcoes = _servicoFuncao.ObtenhaLista(f => f.OrganizacaoId == orgId).OrderBy(f => f.Descricao).ToList();

            if (id.HasValue)
            {
                var config = _servicoConfiguracaoBonus.Obtenha(id.Value);
                if (config != null) return View(config);
            }

            return View(new ConfiguracaoBonus());
        }

        [HttpPost]
        public IActionResult Cadastrar(ConfiguracaoBonus config, List<int> FuncoesIds)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.CadastrarOuAlterar)) return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            config.OrganizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;
            
            if (config.Id > 0)
            {
                var configAtual = _servicoConfiguracaoBonus.Obtenha(config.Id);
                configAtual.Descricao = config.Descricao;
                configAtual.ValorDiario = config.ValorDiario;
                configAtual.TipoBonus = config.TipoBonus;
                configAtual.PerdeIntegralmenteComFalta = config.PerdeIntegralmenteComFalta;
                configAtual.PagaEmFinaisDeSemanaEFeriados = config.PagaEmFinaisDeSemanaEFeriados;
                configAtual.TurnoIntercaladoPagaDobrado = config.TurnoIntercaladoPagaDobrado;
                configAtual.MinutosIntervaloTurnoIntercalado = config.MinutosIntervaloTurnoIntercalado;

                configAtual.Funcoes.Clear();
                if (FuncoesIds != null)
                {
                    foreach (var fid in FuncoesIds)
                        configAtual.Funcoes.Add(new ConfiguracaoBonusFuncao { FuncaoId = fid });
                }

                _servicoConfiguracaoBonus.Atualizar(configAtual);
            }
            else
            {
                if (FuncoesIds != null)
                {
                    foreach (var fid in FuncoesIds)
                        config.Funcoes.Add(new ConfiguracaoBonusFuncao { FuncaoId = fid });
                }

                config.Ativo = true;
                _servicoConfiguracaoBonus.Adicionar(config);
            }
            
            _servicoConfiguracaoBonus.Commit();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProcessarCalculoDoMes(int configuracaoBonusId, string mesReferencia)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.Gerenciar)) return Json(new { sucesso = false, mensagem = "Sem permissão." });

            int orgId = HttpContext.DadosDaSessao().OrganizacaoId;
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

        [HttpGet]
        public IActionResult ObtenhaDetalhes(int id)
        {
            var bonus = _servicoBonusCalculado.Obtenha(id);
            if (bonus == null) return Json(new { sucesso = false });

            return Json(new { sucesso = true, logs = bonus.DetalhesDoCalculoJson });
        }
    }
}
