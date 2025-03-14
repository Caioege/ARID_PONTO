using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades.Base
{
    public class EntidadeRedeDeEnsinoBase : EntidadeBase
    {
        public int RedeDeEnsinoId { get; set; }

        [ForeignKey(nameof(RedeDeEnsinoId))]
        public virtual RedeDeEnsino RedeDeEnsino { get; set; }
    }
}
