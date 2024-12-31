using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class ServidorController : BaseController
    {
        private readonly IServico<Servidor> _servico;

        public ServidorController(
            IServico<Servidor> servico)
        {
            _servico = servico;
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

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Servidor> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaUnidadesOrganizacionais>(listaPaginada.Adicional);

            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            var dados = _servico.ObtenhaListaPaginada(c => c.OrganizacaoId == parametros.OrganizacaoId, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total);
        }
    }
}