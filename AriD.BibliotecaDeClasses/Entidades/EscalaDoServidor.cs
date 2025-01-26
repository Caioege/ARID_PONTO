using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class EscalaDoServidor : EntidadeOrganizacaoBase
    {
        [Required]
        public int EscalaId { get; set; }
        [ForeignKey(nameof(EscalaId))]
        public virtual Escala Escala { get; set; }

        [Required]
        public int CicloDaEscalaId { get; set; }
        [ForeignKey(nameof(CicloDaEscalaId))]
        public virtual CicloDaEscala CicloDaEscala { get; set; }

        [Required]
        public int VinculoDeTrabalhoId { get; set; }
        [ForeignKey(nameof(VinculoDeTrabalhoId))]
        public virtual VinculoDeTrabalho VinculoDeTrabalho { get; set; }

        [Required]
        public DateTime Data { get; set; }
    }
}