using System;
using System.Collections.Generic;

using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoMonitoramentoRotas
    {
        MonitoramentoRotasResultadoDTO ObtenhaMonitoramento(int organizacaoId, DateTime dataBase, bool exibirFinalizadas);
        ChecklistExecucaoRotaDTO? ObtenhaChecklistExecucao(int organizacaoId, int execucaoId);
    }
}
