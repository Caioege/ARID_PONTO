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
        private readonly IServico<LocalizacaoRota> _localizacaoRotaServico;
        private readonly IServico<Organizacao> _organizacaoServico;

        public RotaController(
            IServico<Rota> rotaServico,
            IServico<Motorista> motoristaServico,
            IServico<Veiculo> veiculoServico,
            IServico<ParadaRota> paradaRotaServico,
            IServico<RotaExecucao> rotaExecucaoServico,
            IServico<LocalizacaoRota> localizacaoRotaServico,
            IServico<Organizacao> organizacaoServico)
        {
            _rotaServico = rotaServico;
            _motoristaServico = motoristaServico;
            _veiculoServico = veiculoServico;
            _paradaRotaServico = paradaRotaServico;
            _rotaExecucaoServico = rotaExecucaoServico;
            _localizacaoRotaServico = localizacaoRotaServico;
            _organizacaoServico = organizacaoServico;
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

                var organizacao = _organizacaoServico.Obtenha(organizacaoId);
                ViewBag.LatCentro = !string.IsNullOrEmpty(organizacao.LatitudeCentroide) ? organizacao.LatitudeCentroide : "-15.7942";
                ViewBag.LonCentro = !string.IsNullOrEmpty(organizacao.LongitudeCentroide) ? organizacao.LongitudeCentroide : "-47.8821";

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

        [HttpGet]
        public IActionResult ObterDadosMonitoramento()
        {
            try
            {
                int organizacaoId = this.HttpContext.DadosDaSessao().OrganizacaoId;

                var execucoesAtivas = _rotaExecucaoServico.ObtenhaLista(re => re.OrganizacaoId == organizacaoId && re.DataHoraFim == null);

                var resultado = new List<object>();
                foreach (var exec in execucoesAtivas)
                {
                    var localizacoes = _localizacaoRotaServico.ObtenhaLista(loc => loc.RotaId == exec.RotaId && loc.DataHora >= exec.DataHoraInicio)
                        .OrderBy(loc => loc.DataHora)
                        .ToList();

                    if (localizacoes.Any())
                    {
                        var ultima = localizacoes.Last();
                        double lat = ParseCoord(ultima.Latitude);
                        double lon = ParseCoord(ultima.Longitude);

                        var historico = localizacoes.Select(l => new double[] { ParseCoord(l.Latitude), ParseCoord(l.Longitude) }).ToList();

                        var paradasDoBanco = _paradaRotaServico.ObtenhaLista(p => p.RotaId == exec.RotaId).ToList();
                        var paradasArray = paradasDoBanco.Where(p => !string.IsNullOrEmpty(p.Latitude) && !string.IsNullOrEmpty(p.Longitude))
                            .Select(p => new {
                                nome = p.Endereco,
                                link = p.Link,
                                latitude = ParseCoord(p.Latitude),
                                longitude = ParseCoord(p.Longitude),
                                entregue = p.Entregue,
                                concluidoEm = p.ConcluidoEm?.ToString("dd/MM/yy HH:mm")
                            }).ToList();

                        resultado.Add(new
                        {
                            RotaId = exec.RotaId,
                            Descricao = exec.Rota.Descricao,
                            MotoristaId = exec.Rota.MotoristaId,
                            MotoristaNome = exec.Rota.Motorista.Servidor.Pessoa.Nome,
                            VeiculoId = exec.Rota.VeiculoId,
                            PlacaModelo = $"{exec.Rota.Veiculo.Placa} - {exec.Rota.Veiculo.Modelo}",
                            UltimaLocalizacao = new[] { lat, lon },
                            HistoricoLocalizacoes = historico,
                            UltimaAtualizacao = ultima.DataHora.ToString("dd/MM/yyyy HH:mm:ss"),
                            Paradas = paradasArray
                        });
                    }
                }

                // MOCK LOGIC for Org 7 as requested for visual tracking
                if (organizacaoId == 7)
                {
                   var random = new Random(DateTime.Now.Minute); // Slightly dynamic but stable during the same minute
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
                       bool jaExisteReal = false;
                       try { jaExisteReal = resultado.Any(r => (int)((dynamic)r).MotoristaId == sId); } catch { }

                       if (!jaExisteReal)
                       {
                           var historico = new List<object>();
                           int routeIndex = Array.IndexOf(servidorIds, sId);
                           var mockArray = rotasReaisMock[routeIndex % rotasReaisMock.Count];
                           
                           // Move slowly iterating index according to time seconds
                           int timeStepIdx = (int)(((DateTime.Now.Minute * 60 + DateTime.Now.Second) / 2) % mockArray.Length);
                           int endStep = Math.Max(2, timeStepIdx + 1); // at least 2 points for a line
                           if (endStep >= mockArray.Length) endStep = mockArray.Length - 1;

                           for (int i = 0; i <= endStep; i++)
                           {
                               historico.Add(new[] { mockArray[i][0], mockArray[i][1] });
                           }

                           var mockParadas = new List<object>();
                           int p1Limit = mockArray.Length / 3;
                           int p2Limit = (mockArray.Length / 3) * 2;
                           
                           if (mockArray.Length > p1Limit) {
                               bool ent = endStep >= p1Limit;
                               mockParadas.Add(new { nome = "Escola A", link = "https://maps.google.com/?q=escola+a", latitude = mockArray[p1Limit][0], longitude = mockArray[p1Limit][1], entregue = ent, concluidoEm = ent ? DateTime.Now.AddHours(-1).ToString("dd/MM/yy HH:mm") : null });
                           }
                           if (mockArray.Length > p2Limit) {
                               bool ent = endStep >= p2Limit;
                               mockParadas.Add(new { nome = "Escola B", link = "https://maps.google.com/?q=escola+b", latitude = mockArray[p2Limit][0], longitude = mockArray[p2Limit][1], entregue = ent, concluidoEm = ent ? DateTime.Now.AddHours(-3).ToString("dd/MM/yy HH:mm") : null });
                           }

                           resultado.Add(new
                           {
                               RotaId = 1000 + sId,
                               Descricao = $"Rota Escolar (Mock) - ID {sId}",
                               MotoristaId = sId,
                               MotoristaNome = $"Motorista Teste {sId}",
                               VeiculoId = 500 + sId,
                               PlacaModelo = $"MOC-{random.Next(1000, 9999)} - VAN",
                               UltimaLocalizacao = new[] { mockArray[endStep][0], mockArray[endStep][1] },
                               HistoricoLocalizacoes = historico,
                               UltimaAtualizacao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                               Paradas = mockParadas
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
