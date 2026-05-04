using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RotaOcorrenciaDesvio : EntidadeOrganizacaoBase
    {
        public int RotaId { get; set; }
        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        
        public double DistanciaEmMetros { get; set; }
        public DateTime DataHora { get; set; }
        
        // Se a gente precisar sinalizar se o motorista deu uma justificativa, salvamos aqui
        public string? Motivo { get; set; } 
    }
}
