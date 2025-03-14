using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class EquipamentoDeFrequencia : EntidadeRedeDeEnsinoBase
    {
        public int EscolaId { get; set; }
        [ForeignKey(nameof(EscolaId))]
        public virtual Escola Escola { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        [Required, MaxLength(100)]
        public string NumeroDeSerie { get; set; }

        [Required]
        public bool Ativo { get; set; }
    }
}
