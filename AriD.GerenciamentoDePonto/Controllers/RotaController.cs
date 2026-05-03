using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.DTO;
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
using AriD.Servicos.Extensao;
using AriD.GerenciamentoDePonto.Models;

namespace AriD.GerenciamentoDePonto.Controllers
{
    public class RotaController : BaseController
    {
        private readonly IServico<Rota> _rotaServico;
        private readonly IServico<Motorista> _motoristaServico;
        private readonly IServico<Veiculo> _veiculoServico;
        private readonly IServico<ParadaRota> _paradaRotaServico;
        private readonly IServico<RotaExecucao> _rotaExecucaoServico;
        private readonly IServico<RotaExecucaoEvento> _rotaExecucaoEventoServico;
        private readonly IServico<RotaExecucaoPausa> _rotaExecucaoPausaServico;
        private readonly IServico<LocalizacaoRota> _localizacaoRotaServico;
        private readonly IServico<Organizacao> _organizacaoServico;
        private readonly IServicoMonitoramentoRotas _servicoMonitoramentoRotas;
        private readonly IServico<Servidor> _servidorServico;
        private readonly IServico<RotaVeiculo> _rotaVeiculoServico;
        private readonly IServico<Paciente> _pacienteServico;
        private readonly IServico<RotaPaciente> _rotaPacienteServico;
        private readonly IServico<RotaProfissional> _rotaProfissionalServico;
        private readonly IServico<UnidadeOrganizacional> _unidadeOrganizacionalServico;
        private readonly IServicoDeRoteirizacao _servicoDeRoteirizacao;

        public RotaController(
            IServico<Rota> rotaServico,
            IServico<Motorista> motoristaServico,
            IServico<Veiculo> veiculoServico,
            IServico<ParadaRota> paradaRotaServico,
            IServico<RotaExecucao> rotaExecucaoServico,
            IServico<RotaExecucaoEvento> rotaExecucaoEventoServico,
            IServico<RotaExecucaoPausa> rotaExecucaoPausaServico,
            IServico<LocalizacaoRota> localizacaoRotaServico,
            IServico<Organizacao> organizacaoServico,
            IServicoMonitoramentoRotas servicoMonitoramentoRotas,
            IServico<Servidor> servidorServico,
            IServico<RotaVeiculo> rotaVeiculoServico,
            IServico<Paciente> pacienteServico,
            IServico<RotaPaciente> rotaPacienteServico,
            IServico<RotaProfissional> rotaProfissionalServico,
            IServico<UnidadeOrganizacional> unidadeOrganizacionalServico,
            IServicoDeRoteirizacao servicoDeRoteirizacao)
        {
            _rotaServico = rotaServico;
            _motoristaServico = motoristaServico;
            _veiculoServico = veiculoServico;
            _paradaRotaServico = paradaRotaServico;
            _rotaExecucaoServico = rotaExecucaoServico;
            _rotaExecucaoEventoServico = rotaExecucaoEventoServico;
            _rotaExecucaoPausaServico = rotaExecucaoPausaServico;
            _localizacaoRotaServico = localizacaoRotaServico;
            _organizacaoServico = organizacaoServico;
            _servicoMonitoramentoRotas = servicoMonitoramentoRotas;
            _servidorServico = servidorServico;
            _rotaVeiculoServico = rotaVeiculoServico;
            _pacienteServico = pacienteServico;
            _rotaPacienteServico = rotaPacienteServico;
            _rotaProfissionalServico = rotaProfissionalServico;
            _unidadeOrganizacionalServico = unidadeOrganizacionalServico;
            _servicoDeRoteirizacao = servicoDeRoteirizacao;
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
        public async Task<IActionResult> Cadastro(int id)
        {
            var model = id == 0 ?
                    new Rota { Situacao = AriD.BibliotecaDeClasses.Enumeradores.eStatusRota.Ativa } :
                    _rotaServico.Obtenha(c => c.Id == id);

            var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
            
            var motoristas = _motoristaServico.ObtenhaLista(m => m.OrganizacaoId == organizacaoId)
                .Where(m => m.Situacao == AriD.BibliotecaDeClasses.Enumeradores.eStatusMotorista.Ativo)
                .OrderBy(m => m.Servidor.Pessoa.Nome)
                .Select(m => new { 
                    m.Id, 
                    Nome = $"{m.Servidor.Pessoa.Nome} - {m.Servidor.Pessoa.Cpf} - CNH: {m.CategoriaCNH} {m.VencimentoCNH:dd/MM/yyyy}" 
                })
                .ToList();
            ViewBag.Motoristas = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(motoristas, "Id", "Nome", model.MotoristaId);
            ViewBag.MotoristaSecundario = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(motoristas, "Id", "Nome", model.MotoristaSecundarioId);

            var veiculos = _veiculoServico.ObtenhaLista(v => v.OrganizacaoId == organizacaoId)
                .Where(v => v.Status == AriD.BibliotecaDeClasses.Enumeradores.eStatusVeiculo.Disponivel)
                .OrderBy(v => v.Placa)
                .Select(v => new { v.Id, Descricao = $"{v.Placa} - {v.Modelo}" })
                .ToList();
            
            var veiculosSelecionados = model.Id != 0 ? _rotaVeiculoServico.ObtenhaLista(rv => rv.RotaId == model.Id).Select(rv => rv.VeiculoId).ToList() : new List<int>();
            ViewBag.Veiculos = new Microsoft.AspNetCore.Mvc.Rendering.MultiSelectList(veiculos, "Id", "Descricao", veiculosSelecionados);

            var unidadesFiltro = _unidadeOrganizacionalServico.ObtenhaLista(u => u.OrganizacaoId == organizacaoId && u.Ativa).OrderBy(u => u.Nome).Select(u => new { u.Id, u.Nome }).ToList();
            ViewBag.UnidadesOrigem = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(unidadesFiltro, "Id", "Nome", model.UnidadeOrigemId);
            ViewBag.UnidadesDestino = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(unidadesFiltro, "Id", "Nome", model.UnidadeDestinoId);

            ViewBag.PacientesDisponiveis = _pacienteServico.ObtenhaLista(p => p.OrganizacaoId == organizacaoId && p.Ativo).OrderBy(p => p.Nome).ToList();
            
            var profissionais = _servidorServico.ObtenhaLista(s => s.OrganizacaoId == organizacaoId)
                .OrderBy(s => s.Pessoa.Nome)
                .Select(s => new ProfissionalDisponivelViewModel
                {
                    Id = s.Id,
                    Nome = s.Pessoa.Nome,
                    Cargo = s.VinculosDeTrabalho.FirstOrDefault() != null ? s.VinculosDeTrabalho.FirstOrDefault().Funcao.Descricao : "Servidor",
                    CRM = s.CodigoCRM,
                    Especialidade = s.EspecialidadeMedica
                }).ToList();
            ViewBag.ProfissionaisDisponiveis = profissionais;

            var medicos = _servidorServico.ObtenhaLista(s => s.OrganizacaoId == organizacaoId && !string.IsNullOrEmpty(s.CodigoCRM))
                .OrderBy(s => s.Pessoa.Nome)
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem 
                { 
                    Text = $"{s.Pessoa.Nome} - CRM: {s.CodigoCRM}", 
                    Value = $"{s.Pessoa.Nome} - CRM: {s.CodigoCRM}" 
                }).ToList();
            ViewBag.Medicos = medicos;
            
            var todasUnidades = _unidadeOrganizacionalServico
                .ObtenhaLista(u => u.OrganizacaoId == organizacaoId && u.Ativa)
                .OrderBy(u => u.Nome)
                .Select(u => new UnidadeParaModalViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Latitude = u.Latitude,
                    Longitude = u.Longitude,
                    Endereco = u.Endereco != null ? u.Endereco.ToString() : "",
                    Tipo = (int)u.Tipo
                })
                .ToList();

            ViewBag.UnidadesParaModal = todasUnidades;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Salvar(Rota rota, List<ParadaRota> paradas, List<int> veiculosSelecionados, string pacientesJson, string profissionaisJson)
        {
            if (!HttpContext.PossuiPermissao(eItemDePermissao_Rota.CadastrarOuAlterar))
                throw new ApplicationException("Você não tem permissão para salvar.");

            if (veiculosSelecionados == null || !veiculosSelecionados.Any())
                throw new ApplicationException("Pelo menos um veículo deve ser informado na rota.");

            int id = rota.Id;
            var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
            rota.OrganizacaoId = organizacaoId;

            var pacientes = string.IsNullOrEmpty(pacientesJson) ? new List<RotaPaciente>() : JsonConvert.DeserializeObject<List<RotaPaciente>>(pacientesJson);
            var profissionais = string.IsNullOrEmpty(profissionaisJson) ? new List<RotaProfissional>() : JsonConvert.DeserializeObject<List<RotaProfissional>>(profissionaisJson);

            if (rota.Id == 0)
            {
                if (paradas != null)
                {
                    rota.Paradas = new List<ParadaRota>();
                    foreach (var p in paradas) { p.OrganizacaoId = organizacaoId; rota.Paradas.Add(p); }
                }
                
                rota.VeiculosDaRota = veiculosSelecionados?.Select(vId => new RotaVeiculo { VeiculoId = vId, OrganizacaoId = organizacaoId }).ToList() ?? new List<RotaVeiculo>();
                rota.ListaDePacientes = pacientes;
                rota.ListaDeProfissionais = profissionais;

                // Call intelligent routing
                rota = await _servicoDeRoteirizacao.OtimizarRotaAsync(rota, rota.Paradas.ToList());

                id = _rotaServico.Adicionar(rota);
            }
            else
            {
                var original = _rotaServico.Obtenha(c => c.Id == rota.Id);
                original.MotoristaId = rota.MotoristaId;
                original.MotoristaSecundarioId = rota.MotoristaSecundarioId;
                original.Descricao = rota.Descricao;
                original.Situacao = rota.Situacao;
                original.Recorrente = rota.Recorrente;
                
                original.PermitePausa = rota.PermitePausa;
                original.QuantidadePausas = rota.QuantidadePausas;

                original.DataParaExecucao = rota.DataParaExecucao;
                original.DataInicio = rota.DataInicio;
                original.DataFim = rota.DataFim;
                original.DiasSemana = rota.DiasSemana;
                original.UnidadeOrigemId = rota.UnidadeOrigemId;
                original.UnidadeDestinoId = rota.UnidadeDestinoId;
                original.NomePaciente = rota.NomePaciente;
                original.MedicoResponsavel = rota.MedicoResponsavel;
                original.Observacao = rota.Observacao;

                // Update paradas preserving existing IDs because execution history references paradarota.
                var paradasAtuais = _paradaRotaServico.ObtenhaLista(p => p.RotaId == original.Id);
                var paradasRecebidas = paradas ?? new List<ParadaRota>();
                var idsRecebidos = paradasRecebidas.Where(p => p.Id > 0).Select(p => p.Id).ToHashSet();
                var paradasRemovidas = paradasAtuais.Where(p => !idsRecebidos.Contains(p.Id)).ToList();
                var idsParadasRemovidas = paradasRemovidas.Select(p => p.Id).ToList();

                if (idsParadasRemovidas.Any())
                {
                    var paradasComHistorico = _rotaExecucaoEventoServico
                        .ObtenhaLista(ev => ev.ParadaRotaId.HasValue && idsParadasRemovidas.Contains(ev.ParadaRotaId.Value))
                        .Select(ev => ev.ParadaRotaId.Value)
                        .Distinct()
                        .ToList();

                    if (paradasComHistorico.Any())
                        throw new ApplicationException("Nao e possivel remover pontos da rota que ja possuem historico de execucao. Altere os dados do ponto existente ou crie uma nova rota.");
                }

                foreach (var paradaRemovida in paradasRemovidas)
                {
                    original.Paradas.Remove(paradaRemovida);
                    _paradaRotaServico.Remover(paradaRemovida, false);
                }

                foreach (var paradaRecebida in paradasRecebidas)
                {
                    if (paradaRecebida.Id > 0)
                    {
                        var paradaAtual = paradasAtuais.FirstOrDefault(p => p.Id == paradaRecebida.Id);
                        if (paradaAtual == null)
                            continue;

                        paradaAtual.Endereco = paradaRecebida.Endereco;
                        paradaAtual.Latitude = paradaRecebida.Latitude;
                        paradaAtual.Longitude = paradaRecebida.Longitude;
                        paradaAtual.Link = paradaRecebida.Link;
                        paradaAtual.ObservacaoCadastro = paradaRecebida.ObservacaoCadastro;
                        paradaAtual.UnidadeId = paradaRecebida.UnidadeId;
                        paradaAtual.Ordem = paradaRecebida.Ordem;
                        paradaAtual.OrganizacaoId = organizacaoId;
                    }
                    else
                    {
                        paradaRecebida.Id = 0;
                        paradaRecebida.RotaId = original.Id;
                        paradaRecebida.OrganizacaoId = organizacaoId;
                        original.Paradas.Add(paradaRecebida);
                    }
                }

                // Update Veiculos
                var veiculosAtuais = _rotaVeiculoServico.ObtenhaLista(rv => rv.RotaId == original.Id);
                var idsVeiculosSelecionados = veiculosSelecionados?.ToHashSet() ?? new HashSet<int>();
                foreach (var rv in veiculosAtuais.Where(rv => !idsVeiculosSelecionados.Contains(rv.VeiculoId)).ToList())
                {
                    original.VeiculosDaRota.Remove(rv);
                    _rotaVeiculoServico.Remover(rv, false);
                }

                var idsVeiculosAtuais = veiculosAtuais.Select(rv => rv.VeiculoId).ToHashSet();
                foreach (var vId in idsVeiculosSelecionados.Where(vId => !idsVeiculosAtuais.Contains(vId)))
                    original.VeiculosDaRota.Add(new RotaVeiculo { RotaId = original.Id, VeiculoId = vId, OrganizacaoId = organizacaoId });

                // Update Pacientes
                var pacientesAtuais = _rotaPacienteServico.ObtenhaLista(rp => rp.RotaId == original.Id);
                foreach (var p in pacientesAtuais) _rotaPacienteServico.Remover(p);
                foreach (var p in pacientes) { p.Id = 0; original.ListaDePacientes.Add(p); }

                // Update Profissionais
                var profissionaisAtuais = _rotaProfissionalServico.ObtenhaLista(rp => rp.RotaId == original.Id);
                foreach (var p in profissionaisAtuais) _rotaProfissionalServico.Remover(p);
                foreach (var p in profissionais) { p.Id = 0; original.ListaDeProfissionais.Add(p); }

                // Call intelligent routing
                original = await _servicoDeRoteirizacao.OtimizarRotaAsync(original, original.Paradas.ToList());

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
                    Id = h.Id,
                    DataHoraInicio = h.DataHoraInicio.ToString("dd/MM/yyyy HH:mm"),
                    DataHoraFim = h.DataHoraFim.HasValue ? h.DataHoraFim.Value.ToString("dd/MM/yyyy HH:mm") : "Em andamento",
                    UsuarioInicio = h.UsuarioInicio?.NomeDaPessoa ?? "-",
                    UsuarioFim = h.UsuarioFim?.NomeDaPessoa ?? "-",
                    PossuiRegistroOffline = h.PossuiRegistroOffline,
                    ExecucaoOfflineCompleta = h.ExecucaoOfflineCompleta,
                    ClassificacaoOffline = ObterClassificacaoOffline(h.PossuiRegistroOffline, h.ExecucaoOfflineCompleta),
                    DataHoraPrimeiroRegistroOffline = h.DataHoraPrimeiroRegistroOffline.HasValue ? h.DataHoraPrimeiroRegistroOffline.Value.ToString("dd/MM/yyyy HH:mm") : null,
                    DataHoraUltimoRegistroOffline = h.DataHoraUltimoRegistroOffline.HasValue ? h.DataHoraUltimoRegistroOffline.Value.ToString("dd/MM/yyyy HH:mm") : null,
                    LocalExecucaoId = h.LocalExecucaoId,
                    IdentificadorDispositivo = h.IdentificadorDispositivo
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

            var veiculos = _rotaVeiculoServico.ObtenhaLista(rv => rv.RotaId == rotaId).ToList();
            foreach(var rv in veiculos)
            {
                _rotaVeiculoServico.Remover(rv);
            }

            _rotaServico.Remover(item);

            return Json(new { sucesso = true, mensagem = "A rota foi removida." });
        }

        [HttpGet]
        public IActionResult Monitoramento()
        {
            try
            {
                if (!HttpContext.PossuiPermissao(eItemDePermissao_MonitoramentoMobile.Visualizar))
                    return RedirectToAction("ErroDeAcesso", "ControleDeAcesso");

                var organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                var motoristas = _motoristaServico.ObtenhaLista(m => m.OrganizacaoId == organizacaoId)
                    .Where(m => m.Situacao == AriD.BibliotecaDeClasses.Enumeradores.eStatusMotorista.Ativo)
                    .OrderBy(m => m.Servidor.Pessoa.Nome)
                    .Select(m => new { m.Id, Nome = m.Servidor.Pessoa.Nome })
                    .ToList();
                ViewBag.Motoristas = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(motoristas, "Id", "Nome");

                var veiculos = _veiculoServico.ObtenhaLista(v => v.OrganizacaoId == organizacaoId)
                    .OrderBy(v => v.Placa)
                    .Select(v => new { v.Id, Descricao = $"{v.Placa} - {v.Modelo}" })
                    .ToList();
                ViewBag.Veiculos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(veiculos, "Id", "Descricao");

                var rotas = _rotaServico.ObtenhaLista(r => r.OrganizacaoId == organizacaoId)
                    .OrderBy(r => r.Descricao)
                    .Select(r => new { r.Id, Descricao = r.Descricao })
                    .ToList();
                ViewBag.Rotas = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(rotas, "Id", "Descricao");

                var tiposVeiculo = Enum.GetValues(typeof(AriD.BibliotecaDeClasses.Enumeradores.eTipoVeiculo))
                    .Cast<AriD.BibliotecaDeClasses.Enumeradores.eTipoVeiculo>()
                    .Select(v => new { Id = (int)v, Descricao = v.DescricaoDoEnumerador() })
                    .ToList();
                ViewBag.TiposVeiculo = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tiposVeiculo, "Id", "Descricao");

                var organizacao = _organizacaoServico.Obtenha(organizacaoId);
                ViewBag.LatCentro = !string.IsNullOrEmpty(organizacao.LatitudeCentroide) ? organizacao.LatitudeCentroide : "-15.7942";
                ViewBag.LonCentro = !string.IsNullOrEmpty(organizacao.LongitudeCentroide) ? organizacao.LongitudeCentroide : "-47.8821";

                var unidadesForMap = _unidadeOrganizacionalServico.ObtenhaLista(u => u.OrganizacaoId == organizacaoId && !string.IsNullOrEmpty(u.Latitude) && !string.IsNullOrEmpty(u.Longitude))
                    .Select(u => new {
                        Id = u.Id,
                        Nome = u.Nome,
                        Latitude = ParseCoord(u.Latitude),
                        Longitude = ParseCoord(u.Longitude),
                        TipoId = (int?)u.Tipo,
                        Tipo = u.Tipo.HasValue ? u.Tipo.Value.DescricaoDoEnumerador() : "Unidade"
                    }).ToList();

                ViewBag.UnidadesJson = JsonConvert.SerializeObject(unidadesForMap);


                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }

        private double ParseCoord(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return 0;
            return double.Parse(valor.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
        }

        private string ObterClassificacaoOffline(bool possuiRegistroOffline, bool execucaoOfflineCompleta)
        {
            if (!possuiRegistroOffline) return "";
            return execucaoOfflineCompleta
                ? "Rota executada completamente offline"
                : "Rota executada parcialmente offline";
        }

        [HttpGet]
        public IActionResult ObterDadosMonitoramento(string dataFiltro, bool exibirFinalizadas = false)
        {
            try
            {
                int organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                DateTime dataBase = DateTime.Now.Date;
                if (!string.IsNullOrEmpty(dataFiltro) && DateTime.TryParse(dataFiltro, out DateTime pt))
                {
                    dataBase = pt.Date;
                }

                var resultado = _servicoMonitoramentoRotas.ObtenhaMonitoramento(organizacaoId, dataBase, exibirFinalizadas)
                                    .ToList();

                // MOCK LOGIC for Org 7 as requested for visual tracking
                if (/*organizacaoId == 7*/ false)
                {
                   var random = new Random(DateTime.Now.Minute); // Stable enough for the current minute
                   int[] servidorIds = new int[] { 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34 };
                   double latBase = -15.7942; 
                   double lonBase = -47.8821;

                   var rotasReaisMock = new List<double[][]> {
                       new double[][] { new double[] {-15.794191, -47.88213}, new double[] {-15.793662, -47.881949}, new double[] {-15.793261, -47.883272}, new double[] {-15.795377, -47.883965}, new double[] {-15.795619, -47.883804}, new double[] {-15.795969, -47.882908}, new double[] {-15.796323, -47.882984}, new double[] {-15.796409, -47.883166}, new double[] {-15.796174, -47.884074}, new double[] {-15.796293, -47.884266}, new double[] {-15.798856, -47.885356}, new double[] {-15.799448, -47.885294}, new double[] {-15.7999, -47.884677}, new double[] {-15.800454, -47.884725}, new double[] {-15.803924, -47.886843}, new double[] {-15.80665, -47.889033}, new double[] {-15.810439, -47.892437}, new double[] {-15.812987, -47.895007}, new double[] {-15.815567, -47.89794} },
                       new double[][] { new double[] {-15.794191, -47.88213}, new double[] {-15.793662, -47.881949}, new double[] {-15.793261, -47.883272}, new double[] {-15.795377, -47.883965}, new double[] {-15.796098, -47.88291}, new double[] {-15.79641, -47.883088}, new double[] {-15.796293, -47.884266}, new double[] {-15.798856, -47.885356}, new double[] {-15.800289, -47.88468}, new double[] {-15.801424, -47.885231}, new double[] {-15.801326, -47.885566}, new double[] {-15.801112, -47.88519}, new double[] {-15.802477, -47.882233}, new double[] {-15.803311, -47.878945}, new double[] {-15.803652, -47.878708}, new double[] {-15.809384, -47.883593}, new double[] {-15.809966, -47.883765}, new double[] {-15.811916, -47.881938}, new double[] {-15.816546, -47.881185}, new double[] {-15.824915, -47.876798}, new double[] {-15.826355, -47.875309}, new double[] {-15.827284, -47.873013}, new double[] {-15.828125, -47.872272}, new double[] {-15.830772, -47.871647}, new double[] {-15.831366, -47.872134}, new double[] {-15.831195, -47.872588}, new double[] {-15.830662, -47.872397}, new double[] {-15.830753, -47.871452}, new double[] {-15.831774, -47.869311}, new double[] {-15.833073, -47.867849}, new double[] {-15.83541, -47.86654}, new double[] {-15.83399, -47.86643}, new double[] {-15.828547, -47.856268}, new double[] {-15.827865, -47.856604} },
                       new double[][] { new double[] {-15.801128, -47.876933}, new double[] {-15.798383, -47.876537}, new double[] {-15.797038, -47.876014}, new double[] {-15.79725, -47.875357}, new double[] {-15.795324, -47.874715}, new double[] {-15.792935, -47.882121}, new double[] {-15.785678, -47.880092}, new double[] {-15.773834, -47.881181}, new double[] {-15.76582, -47.882873}, new double[] {-15.748144, -47.888547} }
                   };

                   foreach (var sId in servidorIds)
                   {
                       bool jaExisteReal = resultado.Any(r => r.MotoristaId == sId);

                       if (!jaExisteReal)
                       {
                           var historico = new List<double[]>();
                           int routeIndex = Array.IndexOf(servidorIds, sId);
                           var mockArray = rotasReaisMock[routeIndex % rotasReaisMock.Count];
                           
                           // Move slowly iterating index according to time seconds
                           int timeStepIdx = (int)(((DateTime.Now.Minute * 60 + DateTime.Now.Second) / 2) % mockArray.Length);
                           int endStep = Math.Max(2, timeStepIdx + 1); // at least 2 points for a line
                           if (endStep >= mockArray.Length) endStep = mockArray.Length - 1;

                           bool isFinished = false;
                           // Se for data anterior, o mock é sempre finalizado. Se for hoje, mock é híbrido caso exibirFinalizadas=true
                           if (dataBase < DateTime.Now.Date || (dataBase == DateTime.Now.Date && exibirFinalizadas && sId % 2 == 0)) 
                           {
                               isFinished = true;
                               timeStepIdx = mockArray.Length - 1; 
                               endStep = mockArray.Length - 1;
                           }
                           else if (!exibirFinalizadas && (sId % 2 == 0) && dataBase == DateTime.Now.Date)
                           {
                               // Se não pediu pra exibir finalizadas, e o side é par, ele estaria finalizado então a gente pula
                               continue;
                           }

                           for (int i = 0; i <= endStep; i++)
                           {
                               historico.Add(new[] { mockArray[i][0], mockArray[i][1] });
                           }

                           var mockParadas = new List<MonitoramentoParadaDTO>();
                           int p1Limit = mockArray.Length / 3;
                           int p2Limit = (mockArray.Length / 3) * 2;
                           
                           if (mockArray.Length > p1Limit) {
                               bool ent = endStep >= p1Limit;
                               mockParadas.Add(new MonitoramentoParadaDTO { Nome = "Escola A", Link = "https://maps.google.com/?q=escola+a", Latitude = mockArray[p1Limit][0], Longitude = mockArray[p1Limit][1], Entregue = ent, ConcluidoEm = ent ? dataBase.AddHours(8).ToString("dd/MM/yy HH:mm") : null });
                           }
                           if (mockArray.Length > p2Limit) {
                               bool ent = endStep >= p2Limit;
                               mockParadas.Add(new MonitoramentoParadaDTO { Nome = "Escola B", Link = "https://maps.google.com/?q=escola+b", Latitude = mockArray[p2Limit][0], Longitude = mockArray[p2Limit][1], Entregue = ent, ConcluidoEm = ent ? dataBase.AddHours(9).ToString("dd/MM/yy HH:mm") : null });
                           }

                           resultado.Add(new MonitoramentoRotaDTO
                           {
                               RotaId = 1000 + sId,
                               Descricao = $"Rota Especial (Mock) - ID {sId}",
                               DataParaExecucao = sId % 2 == 0 ? dataBase.ToString("dd/MM/yyyy") : "",
                               NomePaciente = sId % 2 == 0 ? $"Sr./Sra. Silva {sId}" : null,
                               MedicoResponsavel = sId % 2 == 0 ? $"Dr. Especialista {sId} - CRM {random.Next(1000,9999)}" : null,
                               HoraInicio = dataBase.AddHours(7).ToString("dd/MM/yy HH:mm"),
                               HoraFim = isFinished ? dataBase.AddHours(18).ToString("dd/MM/yy HH:mm") : "Em Execução",
                               MotoristaId = sId,
                               MotoristaNome = $"Motorista Teste {sId}",
                               VeiculoId = 500 + sId,
                               PlacaModelo = $"MOC-{random.Next(1000, 9999)} - {((AriD.BibliotecaDeClasses.Enumeradores.eTipoVeiculo)(sId % 5)).DescricaoDoEnumerador()}",
                               TipoVeiculo = sId % 5,
                               UltimaLocalizacao = new[] { mockArray[endStep][0], mockArray[endStep][1] },
                               HistoricoLocalizacoes = historico,
                               UltimaAtualizacao = isFinished ? dataBase.AddHours(18).ToString("dd/MM/yyyy HH:mm:ss") : DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                               Paradas = mockParadas,
                               Finalizada = isFinished
                           });
                       }
                   }
                }

                return Json(new { sucesso = true, dados = resultado });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ObterDadosExecucaoUnica(int execucaoId)
        {
            try
            {
                int organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;
                var exec = _rotaExecucaoServico.Obtenha(execucaoId);
                
                if (exec == null || exec.OrganizacaoId != organizacaoId)
                    return Json(new { sucesso = false, mensagem = "Execução não encontrada." });

                var localizacoes = _localizacaoRotaServico.ObtenhaLista(loc => loc.RotaExecucaoId == exec.Id && loc.DataHoraCaptura >= exec.DataHoraInicio)
                    .OrderBy(loc => loc.DataHoraCaptura)
                    .ToList();

                if (exec.DataHoraFim.HasValue)
                {
                    localizacoes = localizacoes.Where(loc => loc.DataHoraCaptura <= exec.DataHoraFim.Value).ToList();
                }

                double[] ultimaPos = null;
                var historico = localizacoes.Select(l => new double[] { ParseCoord(l.Latitude), ParseCoord(l.Longitude) }).ToList();
                var historicoDetalhado = localizacoes.Select(l => new
                {
                    Latitude = ParseCoord(l.Latitude),
                    Longitude = ParseCoord(l.Longitude),
                    DataHora = l.DataHoraCaptura.ToString("dd/MM/yyyy HH:mm:ss"),
                    l.RegistradoOffline,
                    DataHoraRegistroLocal = l.DataHoraRegistroLocal.HasValue ? l.DataHoraRegistroLocal.Value.ToString("dd/MM/yyyy HH:mm:ss") : null,
                    DataHoraSincronizacao = l.DataHoraSincronizacao.HasValue ? l.DataHoraSincronizacao.Value.ToString("dd/MM/yyyy HH:mm:ss") : null,
                    l.IdentificadorDispositivo,
                    l.ClientEventId
                }).ToList();
                string ultimaAtualizacao = exec.DataHoraInicio.ToString("dd/MM/yyyy HH:mm:ss");

                if (localizacoes.Any())
                {
                    var ultima = localizacoes.Last();
                    ultimaPos = new[] { ParseCoord(ultima.Latitude), ParseCoord(ultima.Longitude) };
                    ultimaAtualizacao = ultima.DataHoraCaptura.ToString("dd/MM/yyyy HH:mm:ss");
                }

                // Buscar paradas reais da execução (joins)
                var paradasRota = _paradaRotaServico.ObtenhaLista(p => p.RotaId == exec.RotaId)
                    .OrderBy(p => p.Ordem)
                    .ThenBy(p => p.Id)
                    .ToList();

                var eventosParada = _rotaExecucaoEventoServico.ObtenhaLista(ev => ev.RotaExecucaoId == exec.Id && ev.TipoEvento == 3)
                    .OrderByDescending(ev => ev.DataHoraEvento)
                    .ThenByDescending(ev => ev.Id)
                    .ToList()
                    .GroupBy(ev => ev.ParadaRotaId)
                    .ToDictionary(g => g.Key, g => g.First());

                var paradasArray = paradasRota
                    .Select(p =>
                    {
                        eventosParada.TryGetValue(p.Id, out var evento);
                        var latitude = evento?.Latitude ?? p.Latitude;
                        var longitude = evento?.Longitude ?? p.Longitude;

                        if (string.IsNullOrWhiteSpace(latitude) || string.IsNullOrWhiteSpace(longitude))
                            return null;

                        return new MonitoramentoParadaDTO
                        {
                            Nome = p.Endereco,
                            Link = p.Link,
                            Latitude = ParseCoord(latitude),
                            Longitude = ParseCoord(longitude),
                            Entregue = evento?.Entregue ?? false,
                            ConcluidoEm = (evento?.DataHoraEvento).HasValue
                                ? evento.DataHoraEvento.ToString("dd/MM/yy HH:mm")
                                : null,
                            RegistradoOffline = evento?.RegistradoOffline ?? false,
                            DataHoraRegistroLocal = evento?.DataHoraRegistroLocal?.ToString("dd/MM/yyyy HH:mm:ss"),
                            DataHoraSincronizacao = evento?.DataHoraSincronizacao?.ToString("dd/MM/yyyy HH:mm:ss"),
                            IdentificadorDispositivo = evento?.IdentificadorDispositivo,
                            ClientEventId = evento?.ClientEventId
                        };
                    })
                    .Where(p => p != null)
                    .ToList();

                var eventosAuditoria = _rotaExecucaoEventoServico.ObtenhaLista(ev => ev.RotaExecucaoId == exec.Id)
                    .OrderBy(ev => ev.DataHoraEvento)
                    .Select(ev => new
                    {
                        ev.TipoEvento,
                        ev.ParadaRotaId,
                        ev.UnidadeId,
                        ev.Observacao,
                        ev.RegistradoOffline,
                        DataHora = ev.DataHoraEvento.ToString("dd/MM/yyyy HH:mm:ss"),
                        DataHoraRegistroLocal = ev.DataHoraRegistroLocal?.ToString("dd/MM/yyyy HH:mm:ss"),
                        DataHoraSincronizacao = ev.DataHoraSincronizacao?.ToString("dd/MM/yyyy HH:mm:ss"),
                        ev.IdentificadorDispositivo,
                        ev.LocalExecucaoId,
                        ev.ClientEventId
                    })
                    .ToList();

                var pausasAuditoria = _rotaExecucaoPausaServico.ObtenhaLista(p => p.RotaExecucaoId == exec.Id)
                    .OrderBy(p => p.DataHoraInicio)
                    .Select(p => new
                    {
                        p.Motivo,
                        DataHoraInicio = p.DataHoraInicio.ToString("dd/MM/yyyy HH:mm:ss"),
                        DataHoraFim = p.DataHoraFim?.ToString("dd/MM/yyyy HH:mm:ss"),
                        p.RegistradoOffline,
                        DataHoraRegistroLocal = p.DataHoraRegistroLocal?.ToString("dd/MM/yyyy HH:mm:ss"),
                        DataHoraSincronizacao = p.DataHoraSincronizacao?.ToString("dd/MM/yyyy HH:mm:ss"),
                        p.IdentificadorDispositivo,
                        p.LocalExecucaoId,
                        p.ClientEventId
                    })
                    .ToList();

                var resultado = new
                {
                    RotaId = exec.RotaId,
                    Descricao = exec.Rota.Descricao,
                    MotoristaId = exec.MotoristaId,
                    MotoristaNome = exec.Motorista?.Servidor.Pessoa.Nome ?? exec.Rota.Motorista.Servidor.Pessoa.Nome,
                    VeiculoId = exec.VeiculoId,
                    PlacaModelo = exec.Veiculo != null ? $"{exec.Veiculo.Placa} - {exec.Veiculo.Modelo}" : "",
                    UltimaLocalizacao = ultimaPos,
                    HistoricoLocalizacoes = historico,
                    HistoricoLocalizacoesDetalhado = historicoDetalhado,
                    UltimaAtualizacao = ultimaAtualizacao,
                    Paradas = paradasArray,
                    EventosAuditoria = eventosAuditoria,
                    PausasAuditoria = pausasAuditoria,
                    Finalizada = exec.DataHoraFim.HasValue,
                    PossuiRegistroOffline = exec.PossuiRegistroOffline,
                    ExecucaoOfflineCompleta = exec.ExecucaoOfflineCompleta,
                    ClassificacaoOffline = ObterClassificacaoOffline(exec.PossuiRegistroOffline, exec.ExecucaoOfflineCompleta),
                    DataHoraPrimeiroRegistroOffline = exec.DataHoraPrimeiroRegistroOffline?.ToString("dd/MM/yyyy HH:mm:ss"),
                    DataHoraUltimoRegistroOffline = exec.DataHoraUltimoRegistroOffline?.ToString("dd/MM/yyyy HH:mm:ss"),
                    exec.LocalExecucaoId,
                    exec.IdentificadorDispositivo
                };

                return Json(new { sucesso = true, dados = new[] { resultado } });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
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

        [HttpPost]
        public async Task<IActionResult> PreviaDeRotaGeo(Rota rota, List<ParadaRota> paradas)
        {
            try
            {
                var safeParadas = paradas ?? new List<ParadaRota>();
                var fakeRota = new Rota { 
                    UnidadeOrigemId = rota.UnidadeOrigemId,
                    UnidadeDestinoId = rota.UnidadeDestinoId 
                };

                if (fakeRota.UnidadeOrigemId.HasValue)
                    fakeRota.UnidadeOrigem = _unidadeOrganizacionalServico.Obtenha(fakeRota.UnidadeOrigemId.Value);
                
                if (fakeRota.UnidadeDestinoId.HasValue)
                    fakeRota.UnidadeDestino = _unidadeOrganizacionalServico.Obtenha(fakeRota.UnidadeDestinoId.Value);

                fakeRota = await _servicoDeRoteirizacao.OtimizarRotaAsync(fakeRota, safeParadas);
                
                return Json(new { 
                    sucesso = true, 
                    polyline = fakeRota.PolylineOficial, 
                    paradas = safeParadas,
                    unidadeOrigem = fakeRota.UnidadeOrigem == null ? null : new { 
                        fakeRota.UnidadeOrigem.Nome, 
                        fakeRota.UnidadeOrigem.Latitude, 
                        fakeRota.UnidadeOrigem.Longitude, 
                        Endereco = fakeRota.UnidadeOrigem.Endereco?.ToString() 
                    },
                    unidadeDestino = fakeRota.UnidadeDestino == null ? null : new { 
                        fakeRota.UnidadeDestino.Nome, 
                        fakeRota.UnidadeDestino.Latitude, 
                        fakeRota.UnidadeDestino.Longitude, 
                        Endereco = fakeRota.UnidadeDestino.Endereco?.ToString() 
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }
    }
}
