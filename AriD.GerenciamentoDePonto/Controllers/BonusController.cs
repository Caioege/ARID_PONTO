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
            Expression<Func<ConfiguracaoBonus, bool>> pesquisa = c => c.OrganizacaoId == organizacaoId;

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
        public IActionResult Adicionar()
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.CadastrarOuAlterar)) 
                return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            int orgId = HttpContext.DadosDaSessao().OrganizacaoId;
            ViewBag.Funcoes = _servicoFuncao.ObtenhaLista(f => f.OrganizacaoId == orgId && f.Ativa).OrderBy(f => f.Descricao).ToList();

            return View("Alterar", new ConfiguracaoBonus() { Funcoes = new List<ConfiguracaoBonusFuncao>() });
        }

        [HttpGet]
        public IActionResult Alterar(int id)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.CadastrarOuAlterar)) 
                return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            var config = _servicoConfiguracaoBonus.Obtenha(id);
            if (config == null) return RedirectToAction("Index");

            int orgId = HttpContext.DadosDaSessao().OrganizacaoId;
            ViewBag.Funcoes = _servicoFuncao.ObtenhaLista(f => f.OrganizacaoId == orgId && f.Ativa).OrderBy(f => f.Descricao).ToList();

            // Verifica se possui cálculos para travar o tipo
            ViewBag.PodeAlterarTipo = !_servicoBonusCalculado.ObtenhaLista(b => b.ConfiguracaoBonusId == id).Any();

            return View(config);
        }

        [HttpPost]
        public IActionResult Salvar(ConfiguracaoBonus config, List<int> FuncoesIds)
        {
            try
            {
                if (!HttpContext.PossuiPermissao(eItemDePermissao_Bonus.CadastrarOuAlterar)) 
                    return Json(new { sucesso = false, mensagem = "Sem permissão para realizar esta operação." });

                config.OrganizacaoId = HttpContext.DadosDaSessao().OrganizacaoId;

                // Sincroniza as funções no objeto do modelo
                config.Funcoes = new List<ConfiguracaoBonusFuncao>();
                if (FuncoesIds != null)
                {
                    foreach (var fid in FuncoesIds)
                        config.Funcoes.Add(new ConfiguracaoBonusFuncao { FuncaoId = fid, ConfiguracaoBonusId = config.Id });
                }

                if (config.Id == 0)
                {
                    _servicoConfiguracaoBonus.Adicionar(config);
                }
                else
                {
                    var configAtual = _servicoConfiguracaoBonus.Obtenha(config.Id);
                    
                    // Validação de dados sensíveis (Bloqueio de Tipo de Bônus se houver cálculos)
                    if (configAtual.TipoBonus != config.TipoBonus)
                    {
                        var possuiCalculos = _servicoBonusCalculado.ObtenhaLista(b => b.ConfiguracaoBonusId == config.Id).Any();
                        if (possuiCalculos)
                            return Json(new { sucesso = false, mensagem = "Não é possível alterar o Tipo do Bônus pois já existem cálculos realizados. Inative esta configuração e crie uma nova." });
                    }

                    // No padrão deste projeto, atualizamos o objeto rastreado ou chamamos Atualizar no objeto do binder dependendo da complexidade das navegações.
                    // Para garantir a persistência das Funções (Many-to-Many), atualizamos a entidade rastreada.
                    configAtual.Descricao = config.Descricao;
                    configAtual.ValorDiario = config.ValorDiario;
                    configAtual.TipoBonus = config.TipoBonus;
                    configAtual.Ativo = config.Ativo;
                    configAtual.ApenasDiasComCargaHoraria = config.ApenasDiasComCargaHoraria;
                    configAtual.MinutosFaltaDesconto = config.MinutosFaltaDesconto;
                    configAtual.MinutosFaltaDescontoMensal = config.MinutosFaltaDescontoMensal;
                    configAtual.TurnoIntercaladoPagaDobrado = config.TurnoIntercaladoPagaDobrado;
                    configAtual.MinutosIntervaloTurnoIntercalado = config.MinutosIntervaloTurnoIntercalado;
                    configAtual.PerdeIntegralmenteComFalta = config.PerdeIntegralmenteComFalta;

                    // Atualiza coleções
                    configAtual.Funcoes.Clear();
                    foreach (var f in config.Funcoes)
                        configAtual.Funcoes.Add(f);

                    _servicoConfiguracaoBonus.Atualizar(configAtual);
                }

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = config.Id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao salvar bônus: " + ex.Message });
            }
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
