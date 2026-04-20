using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.GerenciamentoDePonto.Helpers;
using AriD.GerenciamentoDePonto.WebGrid;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Mvc;
using AriD.Servicos.Extensao;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class ManutencaoController : BaseController
    {
        private readonly IServico<ManutencaoVeiculo> _manutencaoServico;
        private readonly IServico<Veiculo> _veiculoServico;
        private readonly IServico<Servidor> _servidorServico;
        private readonly IServicoNotificacao _notificacaoServico;

        public ManutencaoController(
            IServico<ManutencaoVeiculo> manutencaoServico, 
            IServico<Veiculo> veiculoServico,
            IServico<Servidor> servidorServico,
            IServicoNotificacao notificacaoServico)
        {
            _manutencaoServico = manutencaoServico;
            _veiculoServico = veiculoServico;
            _servidorServico = servidorServico;
            _notificacaoServico = notificacaoServico;
        }

        [HttpGet]
        public IActionResult Index(int veiculoId)
        {
            var veiculo = _veiculoServico.Obtenha(veiculoId);
            ViewBag.Veiculo = veiculo;
            var manutencoes = _manutencaoServico.ObtenhaLista(m => m.VeiculoId == veiculoId).OrderByDescending(m => m.DataManutencao).ToList();
            return View(manutencoes);
        }

        [HttpGet]
        public async Task<IActionResult> Modal(int maintenanceId, int veiculoId)
        {
            var model = maintenanceId == 0 ?
                    new ManutencaoVeiculo { VeiculoId = veiculoId, DataManutencao = DateTime.Today } :
                    _manutencaoServico.Obtenha(maintenanceId);

            if (maintenanceId == 0)
            {
                var veiculo = _veiculoServico.Obtenha(veiculoId);
                model.KmNaManutencao = veiculo?.QuilometragemAtual ?? 0;
            }

            var html = await RenderizarComoString("_Modal", model);
            return Json(new { sucesso = true, html = html });
        }

        [HttpPost]
        public async Task<IActionResult> Salvar(ManutencaoVeiculo manutencao)
        {
            int id = manutencao.Id;
            manutencao.OrganizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

             if (manutencao.Id == 0)
                id = _manutencaoServico.Adicionar(manutencao);
            else
            {
                var original = _manutencaoServico.Obtenha(manutencao.Id);
                original.DataManutencao = manutencao.DataManutencao;
                original.Descricao = manutencao.Descricao;
                original.KmNaManutencao = manutencao.KmNaManutencao;
                original.KmProximaManutencao = manutencao.KmProximaManutencao;
                original.DataVencimentoManutencao = manutencao.DataVencimentoManutencao;
                original.Observacao = manutencao.Observacao;
                original.Situacao = manutencao.Situacao;
                _manutencaoServico.Atualizar(original);
            }

            var veiculo = _veiculoServico.Obtenha(manutencao.VeiculoId);
            if (veiculo != null && manutencao.KmNaManutencao > veiculo.QuilometragemAtual)
            {
                veiculo.QuilometragemAtual = manutencao.KmNaManutencao;
                _veiculoServico.Atualizar(veiculo);
            }

            _ = Task.Run(() => VerificarAlertasManutencao(manutencao.VeiculoId, manutencao.OrganizacaoId));

            return Json(new { sucesso = true, mensagem = "Os dados da manutenção foram salvos.", id = id });
        }

        private async Task VerificarAlertasManutencao(int veiculoId, int organizacaoId)
        {
            try
            {
                var veiculo = _veiculoServico.Obtenha(veiculoId);
                if (veiculo == null) return;

                var ultimaManutencao = _manutencaoServico.ObtenhaLista(m => m.VeiculoId == veiculoId).OrderByDescending(m => m.DataManutencao).FirstOrDefault();
                if (ultimaManutencao == null) return;

                bool precisaAlerta = false;
                string mensagem = "";

                if (ultimaManutencao.KmProximaManutencao.HasValue && veiculo.QuilometragemAtual > 0)
                {
                    var kmRestante = ultimaManutencao.KmProximaManutencao.Value - veiculo.QuilometragemAtual;
                    if (kmRestante <= 1000 && kmRestante > 0)
                    {
                        precisaAlerta = true;
                        mensagem = $"Aviso: Faltam {kmRestante} KM para a próxima manutenção do veículo {veiculo.Placa}.";
                    }
                    else if (kmRestante <= 0)
                    {
                        precisaAlerta = true;
                        mensagem = $"Atenção: A manutenção do veículo {veiculo.Placa} está atrasada pela KM!";
                    }
                }

                if (!precisaAlerta && ultimaManutencao.DataVencimentoManutencao.HasValue)
                {
                    var diasRestantes = (ultimaManutencao.DataVencimentoManutencao.Value - DateTime.Now.Date).TotalDays;
                    if (diasRestantes <= 15 && diasRestantes > 0)
                    {
                        precisaAlerta = true;
                        mensagem = $"Aviso: Faltam {diasRestantes:0} dias para a manutenção do veículo {veiculo.Placa}.";
                    }
                    else if (diasRestantes <= 0)
                    {
                        precisaAlerta = true;
                        mensagem = $"Atenção: A manutenção do veículo {veiculo.Placa} está atrasada por data!";
                    }
                }

                if (precisaAlerta)
                {
                    var servidoresParaNotificar = _servidorServico.ObtenhaLista(s => s.OrganizacaoId == organizacaoId && !string.IsNullOrEmpty(s.PushToken)).Select(s => s.PushToken).Distinct().ToList();
                    if (servidoresParaNotificar.Any())
                    {
                        await _notificacaoServico.EnviarNotificacaoPush(servidoresParaNotificar, "Alerta de Manutenção", mensagem);
                    }
                }
            }
            catch (Exception) { /* Fire and forget handler */ }
        }

        [HttpPost]
        public IActionResult Remova(int maintenanceId)
        {
            var item = _manutencaoServico.Obtenha(maintenanceId);
            _manutencaoServico.Remover(item);
            return Json(new { sucesso = true, mensagem = "O registro foi removido." });
        }
    }
}
