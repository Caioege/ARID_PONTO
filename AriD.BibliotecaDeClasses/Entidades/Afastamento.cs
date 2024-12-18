using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Afastamento : EntidadeOrganizacaoBase
    {
        public int VinculoDeTrabalhoId { get; set; }

        [ForeignKey(nameof(VinculoDeTrabalhoId))]
        public virtual VinculoDeTrabalho VinculoDeTrabalho { get; set; }

        public DateTime Inicio { get; set; }
        public DateTime? Fim { get; set; }
        public DateTime? Retorno { get; set; }

        public bool PorTempoInderteminado => !Fim.HasValue || !Retorno.HasValue;
    }
}