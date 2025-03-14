using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.ParametrosDeConsulta;
using AriD.GerenciamentoEscolar.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class EscolaController : Controller
    {
        private readonly IServico<Escola> _servicoEscola;
        private readonly IServico<RedeDeEnsino> _servicoRedeDeEnsino;

        public EscolaController(
            IServico<Escola> servicoEscola, 
            IServico<RedeDeEnsino> servicoRedeDeEnsino)
        {
            _servicoEscola = servicoEscola;
            _servicoRedeDeEnsino = servicoRedeDeEnsino;
        }

        [HttpGet]
        public IActionResult TabelaPaginada(ListaPaginada<Escola> listaPaginada)
        {
            try
            {
                var parametros = JsonConvert.DeserializeObject<ParametrosConsultaescolasOrganizacionais>(listaPaginada.Adicional);

                var dados = _servicoEscola.ObtenhaListaPaginada(c => c.RedeDeEnsinoId == parametros.RedeDeEnsinoId, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

                listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");

                return View("_TabelaPaginada", listaPaginada);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Adicionar(int redeDeEnsinoId)
        {
            try
            {
                return View(new Escola
                {
                    RedeDeEnsinoId = redeDeEnsinoId,
                    RedeDeEnsino = _servicoRedeDeEnsino.Obtenha(redeDeEnsinoId),
                    Ativa = true
                });
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
                return View(_servicoEscola.Obtenha(id));
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        [HttpPost]
        public IActionResult Salvar(Escola escola)
        {
            int id = escola.Id;

            if (escola.Id == 0)
                id = _servicoEscola.Adicionar(escola);
            else
                _servicoEscola.Atualizar(escola);

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }
    }
}
