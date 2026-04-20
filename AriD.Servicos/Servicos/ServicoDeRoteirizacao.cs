using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Servicos.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeRoteirizacao : IServicoDeRoteirizacao
    {
        private readonly HttpClient _httpClient;

        public ServicoDeRoteirizacao(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Rota> OtimizarRotaAsync(Rota rota, List<ParadaRota> paradas)
        {
            if (paradas == null || paradas.Count < 2)
            {
                // Not enough points to optimize, just return
                if (paradas != null && paradas.Count > 0)
                {
                    paradas[0].Ordem = 0;
                }
                return rota;
            }

            try
            {
                var listaCoordenadas = new List<string>();

                // 1. Origem Fixa (Se houver)
                if (rota.UnidadeOrigem != null && !string.IsNullOrEmpty(rota.UnidadeOrigem.Latitude))
                {
                    listaCoordenadas.Add($"{rota.UnidadeOrigem.Longitude.Replace(",", ".")},{rota.UnidadeOrigem.Latitude.Replace(",", ".")}");
                }

                // 2. Paradas intermédias
                listaCoordenadas.AddRange(paradas.Select(p => $"{p.Longitude?.Replace(",", ".")},{p.Latitude?.Replace(",", ".")}"));

                // 3. Destino Fixo (Se houver)
                if (rota.UnidadeDestino != null && !string.IsNullOrEmpty(rota.UnidadeDestino.Latitude))
                {
                    listaCoordenadas.Add($"{rota.UnidadeDestino.Longitude.Replace(",", ".")},{rota.UnidadeDestino.Latitude.Replace(",", ".")}");
                }

                var coords = string.Join(";", listaCoordenadas);

                var requestUrl = $"http://router.project-osrm.org/trip/v1/driving/{coords}?source=first&destination=last&roundtrip=false&geometries=polyline";

                // Add User-Agent otherwise OSRM might block it
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("User-Agent", "AriD.GerenciamentoDePonto.TSP");

                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(jsonStr);

                    var waypoints = json["waypoints"]?.ToArray();
                    if (waypoints != null)
                    {
                        for (int i = 0; i < waypoints.Length; i++)
                        {
                            var wp = waypoints[i];
                            // The response waypoints array matches the original request order
                            // wp["waypoint_index"] indicates its new position in the optimized route
                            int newOrder = wp["waypoint_index"]?.Value<int>() ?? i;
                            if (i < paradas.Count)
                            {
                                paradas[i].Ordem = newOrder;
                            }
                        }
                    }

                    var geometry = json["trips"]?[0]?["geometry"]?.Value<string>();
                    if (!string.IsNullOrEmpty(geometry))
                    {
                        rota.PolylineOficial = geometry;
                    }
                }
                else
                {
                    // Fallback normal sequence
                    for (int i = 0; i < paradas.Count; i++) paradas[i].Ordem = i;
                }
                
                return rota;
            }
            catch (Exception ex)
            {
                // Fallback gracefully
                for (int i = 0; i < paradas.Count; i++)
                {
                    paradas[i].Ordem = i;
                }
                return rota;
            }
        }
    }
}
