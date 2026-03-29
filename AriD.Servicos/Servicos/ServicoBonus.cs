using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using System.Text.Json;

namespace AriD.Servicos.Servicos
{
    public class ServicoBonus : Servico<BonusCalculado>, IServicoBonus
    {
        private readonly IRepositorio<BonusCalculado> _repositorio;
        private readonly IRepositorio<ConfiguracaoBonus> _repositorioConfiguracaoBonus;
        private readonly IRepositorio<PontoDoDia> _repositorioPontoDoDia;
        private readonly IRepositorio<VinculoDeTrabalho> _repositorioVinculo;

        public ServicoBonus(
            IRepositorio<BonusCalculado> repositorio,
            IRepositorio<ConfiguracaoBonus> repositorioConfiguracaoBonus,
            IRepositorio<PontoDoDia> repositorioPontoDoDia,
            IRepositorio<VinculoDeTrabalho> repositorioVinculo
        ) : base(repositorio)
        {
            _repositorio = repositorio;
            _repositorioConfiguracaoBonus = repositorioConfiguracaoBonus;
            _repositorioPontoDoDia = repositorioPontoDoDia;
            _repositorioVinculo = repositorioVinculo;
        }

        public void GerarBonusDoMes(int organizacaoId, int configuracaoBonusId, string mesReferencia, List<int> vinculosIds, bool forcarRecalculo = false)
        {
            var config = _repositorioConfiguracaoBonus.Obtenha(configuracaoBonusId);
            if (config == null || !config.Ativo) throw new Exception("Configuração de Bônus não encontrada ou inativa.");

            var partes = mesReferencia.Split('/');
            int mes = int.Parse(partes[0]);
            int ano = int.Parse(partes[1]);
            DateTime primeiroDia = new DateTime(ano, mes, 1);
            DateTime ultimoDia = primeiroDia.AddMonths(1).AddDays(-1);

            foreach (var vinculoId in vinculosIds)
            {
                var vinculo = _repositorioVinculo.Obtenha(vinculoId);
                if (vinculo == null) continue;

                if (config.Funcoes.Any() && !config.Funcoes.Any(f => f.FuncaoId == vinculo.FuncaoId))
                    continue;

                var pontos = _repositorioPontoDoDia.ObtenhaLista(p => p.VinculoDeTrabalhoId == vinculoId && p.Data >= primeiroDia && p.Data <= ultimoDia);
                
                // Trava Folha Fechada
                if (!forcarRecalculo && pontos.Any(p => p.PontoFechado))
                    continue;

                int diasTrabalhados = 0;
                int diasIntercalados = 0;
                int diasComFaltaInjustificada = 0;
                double totalMinutosFaltaMes = 0;
                var logs = new List<string>();

                for (DateTime dia = primeiroDia; dia <= ultimoDia; dia = dia.AddDays(1))
                {
                    var ponto = pontos.FirstOrDefault(p => p.Data.Date == dia.Date) ?? new PontoDoDia { Data = dia };
                    bool temCargaHoraria = ponto.CargaHoraria.HasValue && ponto.CargaHoraria.Value.TotalMinutes > 0;

                    if (config.TipoBonus == eTipoBonus.Diario)
                    {
                        if (config.ApenasDiasComCargaHoraria && !temCargaHoraria)
                            continue;

                        double minutosFaltaNoDia = ponto.HorasNegativas?.TotalMinutes ?? 0;
                        bool temRegistroPonto = ponto.Entrada1.HasValue || ponto.Saida1.HasValue;
                        bool faltaInjustificada = temCargaHoraria && !temRegistroPonto && !ponto.JustificativaPeriodo1Id.HasValue;

                        if (faltaInjustificada)
                        {
                            logs.Add($"{ponto.Data:dd/MM}: Falta injustificada detectada. Bônus diário não concedido.");
                        }
                        else if (temRegistroPonto && config.MinutosFaltaDesconto > 0 && minutosFaltaNoDia >= config.MinutosFaltaDesconto)
                        {
                            logs.Add($"{ponto.Data:dd/MM}: Bônus não concedido por atraso/falta excessiva ({minutosFaltaNoDia} min).");
                        }
                        else
                        {
                            diasTrabalhados++;
                            logs.Add($"{ponto.Data:dd/MM}: Dia contabilizado para bônus.");

                            if (config.TurnoIntercaladoPagaDobrado && temRegistroPonto)
                            {
                                if (ponto.Saida1.HasValue && ponto.Entrada2.HasValue)
                                {
                                    var diff = ponto.Entrada2.Value.Subtract(ponto.Saida1.Value);
                                    if (diff.TotalMinutes >= config.MinutosIntervaloTurnoIntercalado)
                                    {
                                        diasIntercalados++;
                                        logs.Add($"{ponto.Data:dd/MM}: Turno Intercalado (intervalo >= {config.MinutosIntervaloTurnoIntercalado} min). Bônus em dobro.");
                                    }
                                }
                            }
                        }
                    }
                    else if (config.TipoBonus == eTipoBonus.Mensal)
                    {
                        if (ponto.HorasNegativas.HasValue)
                            totalMinutosFaltaMes += ponto.HorasNegativas.Value.TotalMinutes;

                        if (!ponto.Entrada1.HasValue && !ponto.Saida1.HasValue && temCargaHoraria && !ponto.JustificativaPeriodo1Id.HasValue)
                        {
                            diasComFaltaInjustificada++;
                        }
                    }
                }

                var jaCalculado = _repositorio.Obtenha(b => b.ConfiguracaoBonusId == configuracaoBonusId && b.VinculoDeTrabalhoId == vinculoId && b.MesReferencia == mesReferencia);
                
                decimal valorTotal = 0;
                if (config.TipoBonus == eTipoBonus.Diario)
                {
                    int fatorMultiplicacao = diasTrabalhados + diasIntercalados;
                    valorTotal = fatorMultiplicacao * config.ValorDiario;
                }
                else if (config.TipoBonus == eTipoBonus.Mensal)
                {
                    if (config.PerdeIntegralmenteComFalta && diasComFaltaInjustificada > 0)
                    {
                        valorTotal = 0;
                        logs.Add($"Bônus zerado devido à regra de Perda Integral por Falta ({diasComFaltaInjustificada} dia(s) contabilizados).");
                    }
                    else if (config.MinutosFaltaDescontoMensal > 0 && totalMinutosFaltaMes >= config.MinutosFaltaDescontoMensal)
                    {
                        valorTotal = 0;
                        logs.Add($"Bônus zerado: Total de faltas no mês ({totalMinutosFaltaMes} min) atingiu o limite de {config.MinutosFaltaDescontoMensal} min.");
                    }
                    else
                    {
                        valorTotal = config.ValorDiario; 
                        logs.Add($"Bônus de assiduidade creditado integralmente.");
                    }
                }

                if (jaCalculado != null)
                {
                    jaCalculado.DiasEfetivosTrabalhados = diasTrabalhados;
                    jaCalculado.DiasTurnoIntercalado = diasIntercalados;
                    jaCalculado.ValorTotal = valorTotal;
                    jaCalculado.DetalhesDoCalculoJson = JsonSerializer.Serialize(logs);
                    jaCalculado.DataCalculo = DateTime.Now;
                    _repositorio.Atualizar(jaCalculado);
                }
                else
                {
                    var novoBonus = new BonusCalculado
                    {
                        OrganizacaoId = organizacaoId,
                        ConfiguracaoBonusId = configuracaoBonusId,
                        VinculoDeTrabalhoId = vinculoId,
                        MesReferencia = mesReferencia,
                        DiasEfetivosTrabalhados = diasTrabalhados,
                        DiasTurnoIntercalado = diasIntercalados,
                        ValorTotal = valorTotal,
                        DetalhesDoCalculoJson = JsonSerializer.Serialize(logs),
                        DataCalculo = DateTime.Now
                    };
                    _repositorio.Add(novoBonus);
                }
            }
            _repositorio.Commit();
        }
        public List<BonusCalculado> ObterOuCalcularBonusFolha(int organizacaoId, int vinculoId, string mesReferencia, bool salvarNoBanco, bool forcarRecalculo = false)
        {
            var configsAtivas = _repositorioConfiguracaoBonus.ObtenhaLista(c => c.OrganizacaoId == organizacaoId && c.Ativo);
            var vinculo = _repositorioVinculo.Obtenha(vinculoId);
            if (vinculo == null) return new List<BonusCalculado>();

            var resultados = new List<BonusCalculado>();

            var partes = mesReferencia.Split('/');
            int mes = int.Parse(partes[0]);
            int ano = int.Parse(partes[1]);
            DateTime primeiroDia = new DateTime(ano, mes, 1);
            DateTime ultimoDia = primeiroDia.AddMonths(1).AddDays(-1);

            var pontos = _repositorioPontoDoDia.ObtenhaLista(p => p.VinculoDeTrabalhoId == vinculoId && p.Data >= primeiroDia && p.Data <= ultimoDia);
            bool folhaFechada = pontos.Any(p => p.PontoFechado);

            foreach (var config in configsAtivas)
            {
                if (config.Funcoes.Any() && !config.Funcoes.Any(f => f.FuncaoId == vinculo.FuncaoId))
                    continue;

                var jaCalculado = _repositorio.Obtenha(b => b.ConfiguracaoBonusId == config.Id && b.VinculoDeTrabalhoId == vinculoId && b.MesReferencia == mesReferencia);
                
                // Se salvarNoBanco é true, mas a folha está fechada e não forçamos, usamos o que está no banco (ou retornamos vazio se não houver)
                if (salvarNoBanco && folhaFechada && !forcarRecalculo)
                {
                    if (jaCalculado != null) 
                    {
                        jaCalculado.ConfiguracaoBonus = config;
                        resultados.Add(jaCalculado);
                    }
                    continue;
                }

                // Se a folha está fechada, não estamos forçando, e apenas queremos VER (salvarNoBanco = false)
                // Retornamos o que já foi salvo para evitar discrepância visual
                if (!salvarNoBanco && folhaFechada && !forcarRecalculo && jaCalculado != null)
                {
                    jaCalculado.ConfiguracaoBonus = config;
                    resultados.Add(jaCalculado);
                    continue;
                }

                int diasTrabalhados = 0;
                int diasIntercalados = 0;
                int diasComFaltaInjustificada = 0;
                double totalMinutosFaltaMes = 0;
                var logs = new List<string>();

                for (DateTime dia = primeiroDia; dia <= ultimoDia; dia = dia.AddDays(1))
                {
                    var ponto = pontos.FirstOrDefault(p => p.Data.Date == dia.Date) ?? new PontoDoDia { Data = dia };
                    bool temCargaHoraria = ponto.CargaHoraria.HasValue && ponto.CargaHoraria.Value.TotalMinutes > 0;

                    if (config.TipoBonus == eTipoBonus.Diario)
                    {
                        if (config.ApenasDiasComCargaHoraria && !temCargaHoraria)
                            continue;

                        double minutosFaltaNoDia = ponto.HorasNegativas?.TotalMinutes ?? 0;
                        bool temRegistroPonto = ponto.Entrada1.HasValue || ponto.Saida1.HasValue;
                        bool faltaInjustificada = temCargaHoraria && !temRegistroPonto && !ponto.JustificativaPeriodo1Id.HasValue;

                        if (faltaInjustificada)
                        {
                            logs.Add($"{ponto.Data:dd/MM}: Falta injustificada detectada. Bônus diário não concedido.");
                        }
                        else if (temRegistroPonto && config.MinutosFaltaDesconto > 0 && minutosFaltaNoDia >= config.MinutosFaltaDesconto)
                        {
                            logs.Add($"{ponto.Data:dd/MM}: Bônus não concedido por atraso/falta excessiva ({minutosFaltaNoDia} min).");
                        }
                        else
                        {
                            diasTrabalhados++;
                            logs.Add($"{ponto.Data:dd/MM}: Dia contabilizado para bônus.");

                            if (config.TurnoIntercaladoPagaDobrado && temRegistroPonto)
                            {
                                if (ponto.Saida1.HasValue && ponto.Entrada2.HasValue)
                                {
                                    var diff = ponto.Entrada2.Value.Subtract(ponto.Saida1.Value);
                                    if (diff.TotalMinutes >= config.MinutosIntervaloTurnoIntercalado)
                                    {
                                        diasIntercalados++;
                                        logs.Add($"{ponto.Data:dd/MM}: Turno Intercalado (intervalo >= {config.MinutosIntervaloTurnoIntercalado} min). Bônus em dobro.");
                                    }
                                }
                            }
                        }
                    }
                    else if (config.TipoBonus == eTipoBonus.Mensal)
                    {
                        if (ponto.HorasNegativas.HasValue)
                            totalMinutosFaltaMes += ponto.HorasNegativas.Value.TotalMinutes;

                        if (!ponto.Entrada1.HasValue && !ponto.Saida1.HasValue && temCargaHoraria && !ponto.JustificativaPeriodo1Id.HasValue)
                        {
                            diasComFaltaInjustificada++;
                        }
                    }
                }

                decimal valorTotal = 0;
                if (config.TipoBonus == eTipoBonus.Diario)
                {
                    int fatorMultiplicacao = diasTrabalhados + diasIntercalados;
                    valorTotal = fatorMultiplicacao * config.ValorDiario;
                }
                else if (config.TipoBonus == eTipoBonus.Mensal)
                {
                    if (config.PerdeIntegralmenteComFalta && diasComFaltaInjustificada > 0)
                    {
                        valorTotal = 0;
                        logs.Add($"Bônus zerado devido à regra de Perda Integral por Falta.");
                    }
                    else if (config.MinutosFaltaDescontoMensal > 0 && totalMinutosFaltaMes >= config.MinutosFaltaDescontoMensal)
                    {
                        valorTotal = 0;
                        logs.Add($"Bônus zerado: Total de faltas no mês ({totalMinutosFaltaMes} min) atingiu o limite.");
                    }
                    else
                    {
                        valorTotal = config.ValorDiario;
                        logs.Add($"Bônus de assiduidade creditado integralmente.");
                    }
                }

                if (jaCalculado != null)
                {
                    jaCalculado.DiasEfetivosTrabalhados = diasTrabalhados;
                    jaCalculado.DiasTurnoIntercalado = diasIntercalados;
                    jaCalculado.ValorTotal = valorTotal;
                    jaCalculado.DetalhesDoCalculoJson = JsonSerializer.Serialize(logs);
                    jaCalculado.DataCalculo = DateTime.Now;
                    jaCalculado.ConfiguracaoBonus = config;

                    if (salvarNoBanco) _repositorio.Atualizar(jaCalculado);

                    resultados.Add(jaCalculado);
                }
                else
                {
                    var novoBonus = new BonusCalculado
                    {
                        OrganizacaoId = organizacaoId,
                        ConfiguracaoBonusId = config.Id,
                        ConfiguracaoBonus = config,
                        VinculoDeTrabalhoId = vinculoId,
                        MesReferencia = mesReferencia,
                        DiasEfetivosTrabalhados = diasTrabalhados,
                        DiasTurnoIntercalado = diasIntercalados,
                        ValorTotal = valorTotal,
                        DetalhesDoCalculoJson = JsonSerializer.Serialize(logs),
                        DataCalculo = DateTime.Now
                    };
                    if (salvarNoBanco) _repositorio.Add(novoBonus);

                    resultados.Add(novoBonus);
                }
            }

            if (salvarNoBanco) _repositorio.Commit();

            return resultados;
        }
    }
}
