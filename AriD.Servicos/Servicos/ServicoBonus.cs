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

        public ServicoBonus(
            IRepositorio<BonusCalculado> repositorio,
            IRepositorio<ConfiguracaoBonus> repositorioConfiguracaoBonus,
            IRepositorio<PontoDoDia> repositorioPontoDoDia
        ) : base(repositorio)
        {
            _repositorio = repositorio;
            _repositorioConfiguracaoBonus = repositorioConfiguracaoBonus;
            _repositorioPontoDoDia = repositorioPontoDoDia;
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
                var pontos = _repositorioPontoDoDia.ObtenhaLista(p => p.VinculoDeTrabalhoId == vinculoId && p.Data >= primeiroDia && p.Data <= ultimoDia);
                
                int diasTrabalhados = 0;
                int diasIntercalados = 0;
                var logs = new List<string>();

                foreach(var ponto in pontos)
                {
                    bool isFDSFeRiado = (ponto.DiaDaSemana == eDiaDaSemana.Sabado || ponto.DiaDaSemana == eDiaDaSemana.Domingo);
                    
                    if (isFDSFeRiado && !config.PagaEmFinaisDeSemanaEFeriados)
                    {
                        continue;
                    }

                    // Verifica se teve marcação de entrada/saída válida = Dia Efetivamente Trabalhado
                    if(ponto.Entrada1.HasValue || ponto.Saida1.HasValue) 
                    {
                        diasTrabalhados++;
                        logs.Add($"{ponto.Data:dd/MM}: Ponto presencial detectado.");

                        if(config.TurnoIntercaladoPagaDobrado)
                        {
                            // Regra de turno intercalado: intervalo entre Saida1 e Entrada2 superior a 2h
                            if (ponto.Saida1.HasValue && ponto.Entrada2.HasValue)
                            {
                                var diff = ponto.Entrada2.Value.Subtract(ponto.Saida1.Value);
                                if (diff.TotalHours >= 2)
                                {
                                    diasIntercalados++;
                                    logs.Add($"{ponto.Data:dd/MM}: Turno Intercalado (intervalo >= 2h). Bônus em dobro.");
                                }
                            }
                        }
                    }
                }

                var jaCalculado = _repositorio.Obtenha(b => b.ConfiguracaoBonusId == configuracaoBonusId && b.VinculoDeTrabalhoId == vinculoId && b.MesReferencia == mesReferencia);
                
                int fatorMultiplicacao = diasTrabalhados + diasIntercalados;
                decimal valorTotal = fatorMultiplicacao * config.ValorDiario;

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
    }
}
