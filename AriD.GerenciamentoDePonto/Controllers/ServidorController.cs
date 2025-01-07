using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class ServidorController : BaseController
    {
        private readonly IServico<Servidor> _servico;
        private readonly IServico<VinculoDeTrabalho> _servicoVinculoDeTrabalho;
        private readonly IServico<TipoDoVinculoDeTrabalho> _servicoTipoDoVinculo;
        private readonly IServico<LotacaoUnidadeOrganizacional> _servicoLotacao;
        private readonly IServico<UnidadeOrganizacional> _servicoUnidadeOrganizacional;

        public ServidorController(
            IServico<Servidor> servico,
            IServico<VinculoDeTrabalho> servicoVinculoDeTrabalho,
            IServico<TipoDoVinculoDeTrabalho> servicoTipoDoVinculo,
            IServico<LotacaoUnidadeOrganizacional> servicoLotacao,
            IServico<UnidadeOrganizacional> servicoUnidadeOrganizacional)
        {
            _servico = servico;
            _servicoVinculoDeTrabalho = servicoVinculoDeTrabalho;
            _servicoTipoDoVinculo = servicoTipoDoVinculo;
            _servicoLotacao = servicoLotacao;
            _servicoUnidadeOrganizacional = servicoUnidadeOrganizacional;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Servidor> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<Servidor> listaPaginada)
        {
            try
            {
                ConfigureDadosDaTabelaPaginada(listaPaginada);
                return View("_Tabela", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Adicionar()
        {
            try
            {
                return View(new Servidor() { Pessoa = new() { Endereco = new() } });
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpGet]
        public IActionResult Alterar(int id)
        {
            try
            {
                return View(_servico.Obtenha(id));
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(Servidor servidor)
        {
            try
            {
                int id = servidor.Id;
                servidor.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
                servidor.Pessoa.OrganizacaoId = servidor.OrganizacaoId;

                if (servidor.Id == 0)
                {
                    servidor.DataDeCadastro = DateTime.Now;
                    id = _servico.Adicionar(servidor);
                }
                else
                    _servico.Atualizar(servidor);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ModalVinculoDeTrabalho(int id)
        {
            try
            {
                var modelo = id == 0 ?
                    new VinculoDeTrabalho() { Inicio = DateTime.Now } :
                    _servicoVinculoDeTrabalho.Obtenha(id);

                var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
                var tipos = _servicoTipoDoVinculo.ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.Ativo);
                ViewBag.Tipos = new SelectList(tipos, "Id", "SiglaComDescricao");

                var html = await RenderizarComoString("_Modal", modelo);

                return Json(new { sucesso = true, html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SalvarVinculoDeTrabalho(VinculoDeTrabalho vinculoDeTrabalho)
        {
            try
            {
                int id = vinculoDeTrabalho.Id;
                vinculoDeTrabalho.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                if (vinculoDeTrabalho.Id == 0)
                    id = _servicoVinculoDeTrabalho.Adicionar(vinculoDeTrabalho);
                else
                    _servicoVinculoDeTrabalho.Atualizar(vinculoDeTrabalho);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DadosEdicaoLotacao(int id)
        {
            try
            {
                var lotacao = id == 0 ?
                    null :
                    _servicoLotacao.Obtenha(id);

                var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                return Json(new
                {
                    sucesso = true,
                    id = id,
                    unidadeId = lotacao?.UnidadeOrganizacionalId,
                    unidades = _servicoUnidadeOrganizacional
                        .ObtenhaLista(c => c.OrganizacaoId == organizacaoId)
                        .OrderBy(c => c.Nome)
                        .Select(c => new CodigoDescricaoDTO(c.Id, c.Nome)),
                    entrada = lotacao?.Entrada.ToShortDateString(),
                    saida = lotacao?.Saida?.ToShortDateString(),
                    matriculaEquipamento = lotacao?.MatriculaEquipamento
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SalvarLotacao(LotacaoUnidadeOrganizacional lotacao)
        {
            try
            {
                int id = lotacao.Id;
                lotacao.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                if (lotacao.Id == 0)
                    id = _servicoLotacao.Adicionar(lotacao);
                else
                    _servicoLotacao.Atualizar(lotacao);

                return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PartialLotacoes(int vinculoId)
        {
            try
            {
                var vinculo = _servicoVinculoDeTrabalho.Obtenha(vinculoId);
                var html = await RenderizarComoString("_Lotacoes", vinculo.Lotacoes);

                return Json(new { sucesso = true, html });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult RemoverLotacao(int id)
        {
            try
            {
                _servicoLotacao.Remover(_servicoLotacao.Obtenha(id));
                return Json(new { sucesso = true, mensagem = "A lotação foi removida." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Servidor> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            var dados = _servico.ObtenhaListaPaginada(c => c.OrganizacaoId == parametros.OrganizacaoId, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}