using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class FaixaHoraExtra : EntidadeOrganizacaoBase
    {
        [Required]
        public int RegraHoraExtraId { get; set; }

        [ForeignKey(nameof(RegraHoraExtraId))]
        public virtual RegraHoraExtra RegraHoraExtra { get; set; }

        [Required]
        public int Ordem { get; set; }

        public int? MinutosAte { get; set; }

        [Required]
        public int Percentual { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
