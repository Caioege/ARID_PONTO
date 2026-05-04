using AriD.BibliotecaDeClasses.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeRoteirizacao
    {
        /// <summary>
        /// Ottimiza a ordem das paradas usando o algoritmo TSP do OSRM.
        /// Retorna a Rota atualizada com a nova ordem nas escalas de Ponto e a Polyline Oficial preenchida.
        /// </summary>
        Task<Rota> OtimizarRotaAsync(Rota rota, List<ParadaRota> paradasAtualizadas);
    }
}
