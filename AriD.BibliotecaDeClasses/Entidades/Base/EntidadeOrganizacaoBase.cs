using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades.Base
{
    public class EntidadeOrganizacaoBase : EntidadeBase
    {
        public int OrganizacaoId { get; set; }

        [ForeignKey(nameof(OrganizacaoId))]
        public virtual Organizacao Organizacao { get; set; }
    }
}