using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ParadaRota : EntidadeOrganizacaoBase
    {
        public int RotaId { get; set; }
        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public int? UnidadeId { get; set; }
        [ForeignKey(nameof(UnidadeId))]
        public virtual UnidadeOrganizacional Unidade { get; set; }

        public string Endereco { get; set; }
        public int Ordem { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Link { get; set; }
        
    }
}
