using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class VinculoDeTrabalho : EntidadeOrganizacaoBase
    {
        public int TipoDoVinculoDeTrabalhoId { get; set; }

        [ForeignKey(nameof(TipoDoVinculoDeTrabalhoId))]
        public virtual TipoDoVinculoDeTrabalho TipoDoVinculoDeTrabalho { get; set; }

        [Required, MaxLength(10)]
        public string Matricula { get; set; }

        [Required]
        public DateTime Inicio { get; set; }
        public DateTime? Fim { get; set; }

        public eSituacaoVinculoDeTrabalho Situacao { get; set; }

        public virtual List<LotacaoUnidadeOrganizacional> Lotacoes { get; set; } = new List<LotacaoUnidadeOrganizacional>();
    }
}