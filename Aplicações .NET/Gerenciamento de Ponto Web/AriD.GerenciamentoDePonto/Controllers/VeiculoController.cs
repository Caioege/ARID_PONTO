using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores.Permissao;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq.Expressions;
using AriD.Servicos.Extensao;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class VeiculoController : BaseController
    {
        private readonly IServico<Veiculo> _veiculoServico;
        private readonly IServico<ChecklistItem> _checklistServico;
        private readonly IServico<ManutencaoVeiculo> _manutencaoServico;

        public VeiculoController(
            IServico<Veiculo> veiculoServico, 
            IServico<ChecklistItem> checklistServico,
            IServico<ManutencaoVeiculo> manutencaoServico)
        {
            _veiculoServico = veiculoServico;
            _checklistServico = checklistServico;
            _manutencaoServico = manutencaoServico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Veiculo> listaPaginada)
        {
            try
            {
                if (!HttpContext.PossuiPermissao(eItemDePermissao_Veiculo.Visualizar))
                    return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View(listaPaginada);
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Veiculo> listaPaginada)
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
        public IActionResult Adicionar()
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Veiculo.CadastrarOuAlterar))
                return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            var model = new Veiculo 
            { 
                Status = AriD.BibliotecaDeClasses.Enumeradores.eStatusVeiculo.Disponivel, 
                AnoFabricacao = DateTime.Now.Year, 
                AnoModelo = DateTime.Now.Year, 
                VencimentoLicenciamento = DateTime.Today.AddYears(1) 
            };
            
            PrepareViewBags(model);
            return View(model);
        }

        [HttpGet]
        public IActionResult Alterar(int id)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Veiculo.Visualizar))
                return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

            var model = _veiculoServico.Obtenha(id);
            if (model == null) return RedirectToAction("Index");

            PrepareViewBags(model);
            
            // Load history for the view if needed via ViewBag or similar
            ViewBag.HistoricoManutencao = _manutencaoServico.ObtenhaLista(m => m.VeiculoId == id).OrderByDescending(m => m.DataManutencao).ToList();

            return View(model);
        }

        private void PrepareViewBags(Veiculo model)
        {
            var tiposVeiculo = Enum.GetValues(typeof(AriD.BibliotecaDeClasses.Enumeradores.eTipoVeiculo))
                .Cast<AriD.BibliotecaDeClasses.Enumeradores.eTipoVeiculo>()
                .Select(v => new { Id = (int)v, Descricao = v.DescricaoDoEnumerador() })
                .ToList();
            ViewBag.TiposVeiculo = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tiposVeiculo, "Id", "Descricao", (int)model.TipoVeiculo);

            var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
            ViewBag.OrganizacaoNome = this.HttpContext.DadosDaSessao().OrganizacaoNome;
        }

        [HttpPost]
        public IActionResult Salvar(Veiculo veiculo)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Veiculo.CadastrarOuAlterar))
                throw new ApplicationException("Você não tem permissão para salvar.");

            int id = veiculo.Id;
            veiculo.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            // Validação de unicidade
            var jaExiste = _veiculoServico.ObtenhaLista(v => v.OrganizacaoId == veiculo.OrganizacaoId && v.Id != veiculo.Id &&
                (v.Placa.ToUpper() == veiculo.Placa.ToUpper() || 
                 v.Renavam == veiculo.Renavam || 
                 v.Chassi == veiculo.Chassi)).Any();

            if (jaExiste)
                throw new ApplicationException("Já existe um outro veículo cadastrado com esta mesma Placa, Renavam ou Chassi.");

            if (veiculo.Id == 0)
                id = _veiculoServico.Adicionar(veiculo);
            else
                _veiculoServico.Atualizar(veiculo);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public IActionResult Remova(int veiculoId)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Veiculo.Excluir))
                throw new ApplicationException("Você não tem permissão para remover.");

            var item = _veiculoServico.Obtenha(veiculoId);
            _veiculoServico.Remover(item);

            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        [HttpGet]
        public IActionResult ObtenhaChecklist(int veiculoId)
        {
            var itens = _checklistServico.ObtenhaLista(c => c.VeiculoId == veiculoId && c.Ativo)
                .Select(c => new { c.Id, c.Descricao });
            return Json(new { sucesso = true, itens = itens });
        }

        [HttpPost]
        public IActionResult SalvarItemChecklist(int veiculoId, string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return Json(new { sucesso = false, mensagem = "Descrição é obrigatória." });

            var item = new ChecklistItem
            {
                OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId,
                VeiculoId = veiculoId,
                Descricao = descricao,
                Ativo = true
            };

            _checklistServico.Adicionar(item);
            return Json(new { sucesso = true, id = item.Id });
        }

        [HttpPost]
        public IActionResult RemoverItemChecklist(int id)
        {
            var item = _checklistServico.Obtenha(id);
            if (item != null)
            {
                item.Ativo = false;
                _checklistServico.Atualizar(item);
            }
            return Json(new { sucesso = true });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Veiculo> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaVeiculo>(listaPaginada.Adicional);
            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<Veiculo, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (parametros.Situacao.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => c.Status == parametros.Situacao.Value);

            if (parametros.TipoCombustivel.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => c.TipoCombustivel == parametros.TipoCombustivel.Value);

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Placa.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.Marca.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.Modelo.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _veiculoServico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}
