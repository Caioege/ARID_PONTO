using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RegistroDePonto : EntidadeRedeDeEnsinoBase
    {
        [Required]
        public int EquipamentoDeFrequenciaId { get; set; }
        [ForeignKey(nameof(EquipamentoDeFrequenciaId))]
        public virtual EquipamentoDeFrequencia EquipamentoDeFrequencia { get; set; }

        [Required, MaxLength(50)]
        public string UsuarioEquipamentoId { get; set; }

        [Required]
        public DateTime DataHoraRegistro { get; set; }

        [Required]
        public DateTime DataHoraRecebimento { get; set; }

        [Required]
        public eTipoDeRegistroEquipamento TipoRegistro { get; set; }
    }
}
