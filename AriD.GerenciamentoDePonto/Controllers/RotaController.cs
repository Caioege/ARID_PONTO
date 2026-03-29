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
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RotaSaveDTO
    {
        public Rota Rota { get; set; }
        public List<ParadaRota> Paradas { get; set; }
    }

    public class RotaController : BaseController
    {
        private readonly IServico<Rota> _rotaServico;
        private readonly IServico<Motorista> _motoristaServico;
        private readonly IServico<Veiculo> _veiculoServico;
        private readonly IServico<ParadaRota> _paradaRotaServico;
        private readonly IServico<RotaExecucao> _rotaExecucaoServico;

        public RotaController(
            IServico<Rota> rotaServico,
            IServico<Motorista> motoristaServico,
            IServico<Veiculo> veiculoServico,
            IServico<ParadaRota> paradaRotaServico,
            IServico<RotaExecucao> rotaExecucaoServico)
        {
            _rotaServico = rotaServico;
            _motoristaServico = motoristaServico;
            _veiculoServico = veiculoServico;
            _paradaRotaServico = paradaRotaServico;
            _rotaExecucaoServico = rotaExecucaoServico;
        }

        [HttpGet]
        public IActionResult Index(ListaPaginada<Rota> listaPaginada)
        {
            try
            {
                if (!HttpContext.PossuiPermissao(eItemDePermissao_Rota.Visualizar))
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
        public IActionResult TabelaPaginada(ListaPaginada<Rota> listaPaginada)
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
        public async Task<IActionResult> Modal(int rotaId)
        {
            var model = rotaId == 0 ?
                    new Rota { Situacao = AriD.BibliotecaDeClasses.Enumeradores.eStatusRota.Ativa } :
                    _rotaServico.Obtenha(c => c.Id == rotaId);

            var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
            
            var motoristas = _motoristaServico.ObtenhaLista(m => m.OrganizacaoId == organizacaoId)
                .Where(m => m.Situacao == AriD.BibliotecaDeClasses.Enumeradores.eStatusMotorista.Ativo)
                .OrderBy(m => m.Servidor.Pessoa.Nome)
                .Select(m => new { m.Id, Nome = m.Servidor.Pessoa.Nome })
                .ToList();
            ViewBag.Motoristas = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(motoristas, "Id", "Nome", model.MotoristaId);

            var veiculos = _veiculoServico.ObtenhaLista(v => v.OrganizacaoId == organizacaoId)
                .Where(v => v.Status == AriD.BibliotecaDeClasses.Enumeradores.eStatusVeiculo.Disponivel)
                .OrderBy(v => v.Placa)
                .Select(v => new { v.Id, Descricao = $"{v.Placa} - {v.Modelo}" })
                .ToList();
            ViewBag.Veiculos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(veiculos, "Id", "Descricao", model.VeiculoId);

            var html = await RenderizarComoString("_Modal", model);
            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public IActionResult Salvar(Rota rota, List<ParadaRota> paradas)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Rota.CadastrarOuAlterar))
                throw new ApplicationException("Você não tem permissão para salvar.");

            int id = rota.Id;
            rota.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            if (rota.Id == 0)
            {
                if (paradas != null)
                {
                    rota.Paradas = new List<ParadaRota>();
                    foreach (var p in paradas)
                    {
                        p.OrganizacaoId = rota.OrganizacaoId;
                        rota.Paradas.Add(p);
                    }
                }
                id = _rotaServico.Adicionar(rota);
            }
            else
            {
                var original = _rotaServico.Obtenha(c => c.Id == rota.Id);
                original.MotoristaId = rota.MotoristaId;
                original.VeiculoId = rota.VeiculoId;
                original.Descricao = rota.Descricao;
                original.Situacao = rota.Situacao;
                original.Recorrente = rota.Recorrente;

                // Update paradas
                if (paradas != null)
                {
                    // Update or Add
                    foreach(var p in paradas)
                    {
                        if (p.Id == 0)
                        {
                            p.OrganizacaoId = original.OrganizacaoId;
                            original.Paradas.Add(p);
                        }
                        else
                        {
                            var paradaOriginal = original.Paradas.FirstOrDefault(x => x.Id == p.Id);
                            if (paradaOriginal != null)
                            {
                                paradaOriginal.Endereco = p.Endereco;
                                paradaOriginal.Latitude = p.Latitude;
                                paradaOriginal.Longitude = p.Longitude;
                                paradaOriginal.Link = p.Link;
                                paradaOriginal.Observacao = p.Observacao;
                            }
                        }
                    }

                    // Remove missing
                    var paradasParaRemover = original.Paradas.Where(p => !paradas.Any(x => x.Id == p.Id) && p.Id != 0).ToList();
                    foreach (var pr in paradasParaRemover)
                    {
                        _paradaRotaServico.Remover(pr);
                    }
                }
                else
                {
                    var todasParadas = original.Paradas.ToList();
                    foreach (var pr in todasParadas)
                    {
                        _paradaRotaServico.Remover(pr);
                    }
                }

                _rotaServico.Atualizar(original);
            }

            return Json(new { sucesso = true, mensagem = "Os dados da rota foram salvos.", id = id });
        }

        [HttpGet]
        public IActionResult ObtenhaHistoricoExecucoes(int rotaId)
        {
            var historico = _rotaExecucaoServico.ObtenhaLista(h => h.RotaId == rotaId)
                .OrderByDescending(h => h.DataHoraInicio)
                .Select(h => new {
                    DataHoraInicio = h.DataHoraInicio.ToString("dd/MM/yyyy HH:mm"),
                    DataHoraFim = h.DataHoraFim.HasValue ? h.DataHoraFim.Value.ToString("dd/MM/yyyy HH:mm") : "Em andamento",
                    UsuarioInicio = h.UsuarioInicio?.NomeDaPessoa ?? "-",
                    UsuarioFim = h.UsuarioFim?.NomeDaPessoa ?? "-"
                });
            return Json(new { sucesso = true, historico = historico });
        }

        [HttpPost]
        public IActionResult Remova(int rotaId)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Rota.Excluir))
                throw new ApplicationException("Você não tem permissão para remover.");

            var item = _rotaServico.Obtenha(r => r.Id == rotaId);
            
            // Remove paradas first
            foreach(var parada in item.Paradas.ToList())
            {
                _paradaRotaServico.Remover(parada);
            }

            _rotaServico.Remover(item);

            return Json(new { sucesso = true, mensagem = "A rota foi removida." });
        }

        private void ConfigureDadosDaTabelaPaginada(ListaPaginada<Rota> listaPaginada)
        {
            var parametros = JsonConvert.DeserializeObject<ParametrosConsultaRota>(listaPaginada.Adicional);
            parametros.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

            Expression<Func<Rota, bool>> filtro =
                c => c.OrganizacaoId == parametros.OrganizacaoId;

            if (parametros.Situacao.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => c.Situacao == parametros.Situacao.Value);

            if (parametros.Recorrente.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => c.Recorrente == parametros.Recorrente.Value);

            if (parametros.MotoristaId.HasValue)
                filtro = ConcatenadorDeExpressao.Concatenar(filtro, c => c.MotoristaId == parametros.MotoristaId.Value);

            if (!string.IsNullOrEmpty(listaPaginada.TermoDeBusca))
            {
                filtro = ConcatenadorDeExpressao.Concatenar(
                    filtro,
                    c => c.Descricao.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()) ||
                    c.Motorista.Servidor.Pessoa.Nome.ToLower().Contains(listaPaginada.TermoDeBusca.ToLower()));
            }

            var dados = _rotaServico.ObtenhaListaPaginada(filtro, listaPaginada.Pagina, listaPaginada.QuantidadeDeItensPorPagina);

            listaPaginada.Parametros(this, dados.Itens, dados.Total, "TabelaPaginada");
        }
    }
}
