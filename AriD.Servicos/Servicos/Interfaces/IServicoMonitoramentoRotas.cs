using System;
using System.Collections.Generic;

using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoMonitoramentoRotas
    {
        IEnumerable<MonitoramentoRotaDTO> ObtenhaMonitoramento(int organizacaoId, DateTime dataBase, bool exibirFinalizadas);
    }
}
