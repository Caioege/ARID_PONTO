using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.DTO.Aplicativo
{
    public class PostRegistroDePontoDTO
    {
        [Required]
        public int VinculoDeTrabalhoId { get; set; }
        public IFormFile? Imagem { get; set; }

        public string? Latitude { get;set; }
        public string? Longitude { get;set; }

        public bool Manual { get; set; }
        public DateTime? DataHora { get; set; }
        public string? Observacao { get; set; }

        public int? JustificativaDeAusenciaId { get; set; }
        public DateTime? DataInicialAtestado { get; set; }
        public DateTime? DataFinalAtestado { get; set; }
    }
}