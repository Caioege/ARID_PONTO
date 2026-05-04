using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class PontoDoDiaHoraExtra : EntidadeOrganizacaoBase
    {
        [Required]
        public int PontoDoDiaId { get; set; }

        [ForeignKey(nameof(PontoDoDiaId))]
        public virtual PontoDoDia PontoDoDia { get; set; }

        [Required]
        public eTipoDiaHoraExtra TipoDia { get; set; }

        [Required]
        public eDiaDaSemana DiaDaSemana { get; set; }

        public bool EhFeriado { get; set; }
        public bool EhFacultativo { get; set; }

        [Required]
        public eOrigemHoraExtra Origem { get; set; }

        [Required]
        public decimal Percentual { get; set; }

        [Required]
        public int Minutos { get; set; }

        [Required]
        public eStatusAprovacaoHoraExtra Status { get; set; } = eStatusAprovacaoHoraExtra.Pendente;

        public int MinutosAprovados { get; set; } = 0;
    }
}
