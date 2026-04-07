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

        public VeiculoController(IServico<Veiculo> veiculoServico)
        {
            _veiculoServico = veiculoServico;
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
        public async Task<IActionResult> Modal(int veiculoId)
        {
            var model = veiculoId == 0 ?
                    new Veiculo { Status = AriD.BibliotecaDeClasses.Enumeradores.eStatusVeiculo.Disponivel, AnoFabricacao = DateTime.Now.Year, AnoModelo = DateTime.Now.Year, VencimentoLicenciamento = DateTime.Today.AddYears(1) } :
                    _veiculoServico.Obtenha(veiculoId);

            var tiposVeiculo = Enum.GetValues(typeof(AriD.BibliotecaDeClasses.Enumeradores.eTipoVeiculo))
                .Cast<AriD.BibliotecaDeClasses.Enumeradores.eTipoVeiculo>()
                .Select(v => new { Id = (int)v, Descricao = v.DescricaoDoEnumerador() })
                .ToList();
            ViewBag.TiposVeiculo = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tiposVeiculo, "Id", "Descricao");

            var html = await RenderizarComoString("_Modal", model);
            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public IActionResult Salvar(Veiculo veiculo)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Veiculo.CadastrarOuAlterar))
                throw new ApplicationException("Você não tem permissão para salvar.");

            int id = veiculo.Id;
            veiculo.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

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
