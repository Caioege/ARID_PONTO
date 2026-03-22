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

        public void GerarBonusDoMes(int organizacaoId, int configuracaoBonusId, string mesReferencia, List<int> vinculosIds)
        {
            var config = _repositorioConfiguracaoBonus.Obtenha(configuracaoBonusId);
            if (config == null || !config.Ativo) throw new Exception("Configuração de Bônus não encontrada ou inativa.");

            var partes = mesReferencia.Split('/');
            int mes = int.Parse(partes[0]);
            int ano = int.Parse(partes[1]);
            DateTime primeiroDia = new DateTime(ano, mes, 1);
            DateTime ultimoDia = primeiroDia.AddMonths(1).AddDays(-1);

            foreach(var vinculoId in vinculosIds)
            {
                var vinculo = _repositorioVinculo.Obtenha(vinculoId);
                if (vinculo == null) continue;

                // Filtrar por Função
                if (config.Funcoes.Any() && !config.Funcoes.Any(f => f.FuncaoId == vinculo.FuncaoId))
                {
                    continue; // Pula se a função do servidor não tem acesso ao bônus
                }

                var pontos = _repositorioPontoDoDia.ObtenhaLista(p => p.VinculoDeTrabalhoId == vinculoId && p.Data >= primeiroDia && p.Data <= ultimoDia);
                
                int diasTrabalhados = 0;
                int diasIntercalados = 0;
                int diasComFaltaInjustificada = 0;
                var logs = new List<string>();

                foreach(var ponto in pontos)
                {
                    bool isFDSFeRiado = (ponto.DiaDaSemana == eDiaDaSemana.Sabado || ponto.DiaDaSemana == eDiaDaSemana.Domingo);
                    
                    if (isFDSFeRiado && !config.PagaEmFinaisDeSemanaEFeriados)
                    {
                        continue;
                    }

                    // Se não for registro de entrada/saída E for tipo RegistroManual, ou houver alguma falta (exemplo básico: não tem ponto e não tem entrada)
                    // Na folha de ponto Arid, as faltas normalmente geram "SemRegistro" ou abonos ausentes. Aqui podemos checar se foi falta, 
                    // dependendo de como o DB trata ausência. Simplificado: se for dia de trabalho e não tem ponto.
                    // Para o bônus, precisamos de dias trabalhados
                    if(ponto.Entrada1.HasValue || ponto.Saida1.HasValue) 
                    {
                        diasTrabalhados++;
                        logs.Add($"{ponto.Data:dd/MM}: Ponto presencial detectado.");

                        if(config.TurnoIntercaladoPagaDobrado && config.TipoBonus == eTipoBonus.Diario)
                        {
                            // Regra de turno intercalado: intervalo entre Saida1 e Entrada2 superior à tolerância
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
                    else if (!isFDSFeRiado && (!ponto.JustificativaPeriodo1Id.HasValue))
                    {
                        // Dia útil sem ponto registrado e sem justificativa = Falta injustificada provável
                        diasComFaltaInjustificada++;
                        logs.Add($"{ponto.Data:dd/MM}: Falta possivelmente injustificada ou sem registro de ponto.");
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
                    else
                    {
                        valorTotal = config.ValorDiario; // Aqui ValorDiario age como Valor Mensal Total
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
        public List<BonusCalculado> ObterOuCalcularBonusFolha(int organizacaoId, int vinculoId, string mesReferencia, bool salvarNoBanco)
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

            foreach (var config in configsAtivas)
            {
                if (config.Funcoes.Any() && !config.Funcoes.Any(f => f.FuncaoId == vinculo.FuncaoId))
                    continue;

                int diasTrabalhados = 0;
                int diasIntercalados = 0;
                int diasComFaltaInjustificada = 0;
                var logs = new List<string>();

                foreach(var ponto in pontos)
                {
                    bool isFDSFeRiado = (ponto.DiaDaSemana == eDiaDaSemana.Sabado || ponto.DiaDaSemana == eDiaDaSemana.Domingo);
                    
                    if (isFDSFeRiado && !config.PagaEmFinaisDeSemanaEFeriados)
                        continue;

                    if(ponto.Entrada1.HasValue || ponto.Saida1.HasValue) 
                    {
                        diasTrabalhados++;
                        logs.Add($"{ponto.Data:dd/MM}: Ponto presencial detectado.");

                        if(config.TurnoIntercaladoPagaDobrado && config.TipoBonus == eTipoBonus.Diario)
                        {
                            if (ponto.Saida1.HasValue && ponto.Entrada2.HasValue)
                            {
                                var diff = ponto.Entrada2.Value.Subtract(ponto.Saida1.Value);
                                if (diff.TotalMinutes >= config.MinutosIntervaloTurnoIntercalado)
                                {
                                    diasIntercalados++;
                                    logs.Add($"{ponto.Data:dd/MM}: Turno Intercalado detectado.");
                                }
                            }
                        }
                    } 
                    else if (!isFDSFeRiado && (!ponto.JustificativaPeriodo1Id.HasValue))
                    {
                        diasComFaltaInjustificada++;
                        logs.Add($"{ponto.Data:dd/MM}: Falta possivelmente injustificada.");
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
                    else
                    {
                        valorTotal = config.ValorDiario;
                        logs.Add($"Bônus de assiduidade creditado integralmente.");
                    }
                }

                var jaCalculado = _repositorio.Obtenha(b => b.ConfiguracaoBonusId == config.Id && b.VinculoDeTrabalhoId == vinculoId && b.MesReferencia == mesReferencia);
                
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
