using AriD.BibliotecaDeClasses.Comum;
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
    public class EquipamentoDePontoController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IServico<EquipamentoDePonto> _equipamentoServico;
        private readonly IServico<UnidadeOrganizacional> _unidadeServico;
        private readonly IServicoDeArquivoFonteDeDados _servicoDeArquivoFonteDeDados;

        public EquipamentoDePontoController(
            IServico<EquipamentoDePonto> equipamentoServico,
            IServico<UnidadeOrganizacional> unidadeServico,
            IConfiguration configuration,
            IServicoDeArquivoFonteDeDados servicoDeArquivoFonteDeDados)
        {
            _equipamentoServico = equipamentoServico;
            _unidadeServico = unidadeServico;
            _configuration = configuration;
            _servicoDeArquivoFonteDeDados = servicoDeArquivoFonteDeDados;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<EquipamentoDePonto> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<EquipamentoDePonto> listaPaginada)
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
            var model = equipamentoId == 0 ?
                    new EquipamentoDePonto { Ativo = true } :
                    _equipamentoServico.Obtenha(equipamentoId);

            if (equipamentoId == 0)
                ViewBag.Unidades = new SelectList(_unidadeServico.ObtenhaLista(c => c.OrganizacaoId == this.DadosDaSessao().OrganizacaoId).OrderBy(c => c.Nome),
                    "Id", "Nome");

            var html = await RenderizarComoString("_Modal", model);

            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public async Task<IActionResult> Salvar(EquipamentoDePonto equipamento)
        {
            try
            {
                int id = equipamento.Id;
                equipamento.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

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

                throw ex;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Remova(int equipamentoId)
        {
            var equipamento = _equipamentoServico.Obtenha(equipamentoId);
            var numeroDeSerie = equipamento.NumeroDeSerie;

            _equipamentoServico.Remover(equipamento);

            EnvieNotificacaoParaGerenciadorDeEquipamento(numeroDeSerie, true);

            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        [HttpGet]
        public ActionResult ImportarAFD()
        {
            var dadosDaSessao = this.DadosDaSessao();
            if (dadosDaSessao.Perfil == ePerfilDeAcesso.Organizacao)
            {
                var equipamentos = _equipamentoServico
                    .ObtenhaLista(c => c.OrganizacaoId == dadosDaSessao.OrganizacaoId && c.Ativo)
                    .OrderBy(c => c.UnidadeOrganizacional.Nome)
                    .ThenBy(c => c.Descricao);

                ViewBag.Equipamentos = new SelectList(equipamentos, "Id", "Descricao", null, "UnidadeOrganizacional.Nome");
            }
            else
            {
                var unidadeId = dadosDaSessao.UnidadeOrganizacionais.First();
                var equipamentos = _equipamentoServico
                    .ObtenhaLista(c => c.UnidadeOrganizacionalId == unidadeId && c.Ativo)
                    .OrderBy(c => c.Descricao);

                ViewBag.Equipamentos = new SelectList(equipamentos, "Id", "Descricao");
            }

            return View();
        }

        [HttpPost]
        public ActionResult ImportarAFD(
            int equipamentoId, 
            int ultimoNSRInformado, 
            IFormFile arquivo)
        {
            if (arquivo == null || Path.GetExtension(arquivo.FileName) != ".txt")
                throw new ApplicationException("Arquivo inválido.");

            var qtd = _servicoDeArquivoFonteDeDados
                .ImportarArquivoAFD(equipamentoId, ultimoNSRInformado, this.DadosDaSessao(), arquivo.OpenReadStream());

            return Json(new 
            { 
                sucesso = true, 
                mensagem = qtd == 0 
                    ? "O arquivo foi recebido mas nenhum registro foi importado." 
                    : qtd == 1 
                        ? "Um registro de ponto foi importado." 
                        : $"{qtd} registros de ponto foram imporatdos." 
            });
        }

        [HttpGet]
        public ActionResult ObtenhaNSR(int equipamentoId)
        {
            var equipamento = _equipamentoServico.Obtenha(equipamentoId);
            return Json(new { sucesso = true, nsr = equipamento?.UltimoNSRLido ?? 0 });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<EquipamentoDePonto> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            var dadosDaSessao = this.HttpContext.DadosDaSessao();
            parametros.OrganizacaoId = dadosDaSessao.OrganizacaoId;

            Expression<Func<EquipamentoDePonto, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                        c.UnidadeOrganizacional.Nome.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            if (dadosDaSessao.UnidadeOrganizacionais.Any())
            {
                filtro = ConcatenadorDeExpressao.Concatenar(filtro,
                    c => dadosDaSessao.UnidadeOrganizacionais.Contains(c.UnidadeOrganizacionalId));
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
                    Server = "ARIDPONTO"
                });
            }
        }
    }
}