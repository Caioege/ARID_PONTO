using AriD.BibliotecaDeClasses.Comum;
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
    public class EquipamentoDeFrequenciaController : BaseController
    {
        private readonly IServico<EquipamentoDeFrequencia> _equipamentoServico;
        private readonly IServico<Escola> _escolaServico;
        private readonly IConfiguration _configuration;

        public EquipamentoDeFrequenciaController(
            IServico<EquipamentoDeFrequencia> equipamentoServico, IServico<Escola> escolaServico, 
            IConfiguration configuration)
        {
            _equipamentoServico = equipamentoServico;
            _escolaServico = escolaServico;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<EquipamentoDeFrequencia> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<EquipamentoDeFrequencia> listaPaginada)
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
        public async Task<IActionResult> Modal(int equipamentoId)
        {
            try
            {
                var model = equipamentoId == 0 ?
                    new EquipamentoDeFrequencia { Ativo = true } :
                    _equipamentoServico.Obtenha(equipamentoId);

                if (equipamentoId == 0)
                    ViewBag.escolas = new SelectList(_escolaServico.ObtenhaLista(c => c.RedeDeEnsinoId == this.DadosDaSessao().RedeDeEnsinoId).OrderBy(c => c.Nome),
                        "Id", "Nome");

                var html = await RenderizarComoString("_Modal", model);

                return Json(new { sucesso = true, html = html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Salvar(EquipamentoDeFrequencia equipamento)
        {
            try
            {
                int id = equipamento.Id;
                equipamento.RedeDeEnsinoId = this.HttpContext.DadosDaSessao().RedeDeEnsinoId;

                if (equipamento.Id == 0)
                    id = _equipamentoServico.Adicionar(equipamento);
                else
                    _equipamentoServico.Atualizar(equipamento);

                EnvieNotificacaoParaGerenciadorDeEquipamento(equipamento.NumeroDeSerie, !equipamento.Ativo);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                var duplicateEntryText = "duplicate entry";
                if (ex.Message.ToLower().Contains(duplicateEntryText) || (ex.InnerException != null && ex.InnerException.Message.ToLower().Contains(duplicateEntryText)))
                {
                    return Json(new { sucesso = false, mensagem = "Já existe um outro equipamento cadastrado com esse número de série." });
                }

                return Json(new { sucesso = false, mensagem = "Ocorreu um erro." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Remova(int equipamentoId)
        {
            try
            {
                var equipamento = _equipamentoServico.Obtenha(equipamentoId);
                var numeroDeSerie = equipamento.NumeroDeSerie;

                _equipamentoServico.Remover(equipamento);

                EnvieNotificacaoParaGerenciadorDeEquipamento(numeroDeSerie, true);

                return Json(new { sucesso = true, mensagem = "O registro foi removido." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Ocorreu um erro." });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<EquipamentoDeFrequencia> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaescolasOrganizacionais>(listaPaginada.Adicional);

            var dadosDaSessao = this.HttpContext.DadosDaSessao();
            parametros.RedeDeEnsinoId = dadosDaSessao.RedeDeEnsinoId;

            Expression<Func<EquipamentoDeFrequencia, bool>> filtro =
                c => c.RedeDeEnsinoId == parametros.RedeDeEnsinoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                        c.Escola.Nome.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            if (dadosDaSessao.EscolaId.HasValue)
            {
                filtro = ConcatenadorDeExpressao.Concatenar(filtro,
                    c => c.EscolaId == dadosDaSessao.EscolaId);
            }

            var dados = _equipamentoServico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }

        private async Task EnvieNotificacaoParaGerenciadorDeEquipamento(string numeroDeSerie, bool remover)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("URI_EQUIPAMENTO_SERVIDOR"));
                httpClient.DefaultRequestHeaders.Add(
                    "ARID-TECNOLOGIA-ACTION", 
                    remover ? "REMOVER-EQUIPAMENTO" : "CADASTRO-EQUIPAMENTO");

                await httpClient.PostAsJsonAsync(string.Empty, new
                {
                    SerialNumber = numeroDeSerie,
                    Server = "ARIDFREQUENCIAESCOLAR"
                });
            }
        }
    }
}
