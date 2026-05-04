using System.Collections.Generic;

namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class EncerrarRotaAppDTO
    {
        public int RotaExecucaoId { get; set; }
        public List<EncerrarParadaDTO> Paradas { get; set; } = new List<EncerrarParadaDTO>();
        public string? Observacao { get; set; }
    }
}
