using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
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
    public class PacienteController : BaseController
    {
        private readonly IServico<Paciente> _pacienteServico;

        public PacienteController(IServico<Paciente> pacienteServico)
        {
            _pacienteServico = pacienteServico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Paciente> listaPaginada)
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
        public IActionResult TabelaPaginada(ListaPaginada<Paciente> listaPaginada)
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
        public async Task<IActionResult> Modal(int pacienteId)
        {
            var model = pacienteId == 0 ?
                    new Paciente { Ativo = true } :
                    _pacienteServico.Obtenha(pacienteId);

            var html = await RenderizarComoString("_Modal", model);
            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public IActionResult Salvar(Paciente paciente)
        {
            int id = paciente.Id;
            paciente.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (paciente.Id == 0)
                id = _pacienteServico.Adicionar(paciente);
            else
            {
                var original = _pacienteServico.Obtenha(paciente.Id);
                original.Nome = paciente.Nome;
                original.CPF = paciente.CPF;
                original.DataNascimento = paciente.DataNascimento;
                original.Telefone = paciente.Telefone;
                original.AcompanhanteNome = paciente.AcompanhanteNome;
                original.AcompanhanteCPF = paciente.AcompanhanteCPF;
                original.Ativo = paciente.Ativo;
                _pacienteServico.Atualizar(original);
            }

            return Json(new { sucesso = true, mensagem = "Os dados foram salvos.", id = id });
        }

        [HttpPost]
        public IActionResult Remova(int pacienteId)
        {
            var item = _pacienteServico.Obtenha(pacienteId);
            _pacienteServico.Remover(item);
            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Paciente> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaPaciente>(listaPaginada.Adicional) ?? new ParametrosConsultaPaciente();
            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<Paciente, bool>> filtro = c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (parametros.Ativo.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => c.Ativo == parametros.Ativo.Value);

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Nome.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    (c.CPF != null && c.CPF.Contains(listaPaginada.TermoDeBusca)));
            }

            var dados = _pacienteServico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);
            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}
