using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RegistroDePonto : EntidadeOrganizacaoBase
    {
        [Required]
        public int EquipamentoDePontoId { get; set; }
        [ForeignKey(nameof(EquipamentoDePontoId))]
        public virtual EquipamentoDePonto EquipamentoDePonto { get; set; }

        [Required, MaxLength(50)]
        public string UsuarioEquipamentoId { get; set; }

        [Required]
        public DateTime DataHoraRegistro { get; set; }

        [Required]
        public DateTime DataHoraRecebimento { get; set; }
    }
}