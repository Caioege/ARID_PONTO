using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class LotacaoUnidadeOrganizacional : EntidadeOrganizacaoBase
    {
        public int UnidadeOrganizacionalId { get; set; }

        [ForeignKey(nameof(UnidadeOrganizacionalId))]
        public virtual UnidadeOrganizacional UnidadeOrganizacional { get; set; }

        public int VinculoDeTrabalhoId { get; set; }

        [ForeignKey(nameof(VinculoDeTrabalhoId))]
        public virtual VinculoDeTrabalho VinculoDeTrabalho { get; set; }

        public DateTime Entrada { get; set; }
        public DateTime? Saida { get; set; }

        [Required, MaxLength(12)]
        public string MatriculaEquipamento { get; set; }
    }
}