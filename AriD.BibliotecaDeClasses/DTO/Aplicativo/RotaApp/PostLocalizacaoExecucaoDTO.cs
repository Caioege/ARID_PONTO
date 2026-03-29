using System;

namespace AriD.BibliotecaDeClasses.DTO.Aplicativo.RotaApp
{
    public class PostLocalizacaoExecucaoDTO
    {
        public int RotaExecucaoId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime DataHora { get; set; }
    }
}
