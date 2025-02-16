using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class EquipamentoDePonto : EntidadeOrganizacaoBase
    {
        public int UnidadeOrganizacionalId { get; set; }
        [ForeignKey(nameof(UnidadeOrganizacionalId))]
        public virtual UnidadeOrganizacional UnidadeOrganizacional { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        [Required, MaxLength(100)]
        public string NumeroDeSerie { get; set; }

        [Required]
        public bool Ativo { get; set; }
    }
}