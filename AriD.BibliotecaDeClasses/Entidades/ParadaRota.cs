using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ParadaRota : EntidadeOrganizacaoBase
    {
        public int RotaId { get; set; }
        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public string Endereco { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Link { get; set; }
        
        public bool Entregue { get; set; }
        public string? Observacao { get; set; }

        public DateTime? ConcluidoEm { get; set; }
    }
}
