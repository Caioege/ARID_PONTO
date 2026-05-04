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

        public int? CicloDaEscalaId { get; set; }
        [ForeignKey(nameof(CicloDaEscalaId))]
        public virtual CicloDaEscala CicloDaEscala { get; set; }

        [Required]
        public int VinculoDeTrabalhoId { get; set; }
        [ForeignKey(nameof(VinculoDeTrabalhoId))]
        public virtual VinculoDeTrabalho VinculoDeTrabalho { get; set; }

        [Required]
        public DateTime Data { get; set; }

        public DateTime? DataFim { get; set; }

        public CicloDaEscala ObterCicloAtual(DateTime dataConsulta)
        {
            int diasPassados = (dataConsulta - Data).Days;
            var cicloAtual = (diasPassados % Escala.Ciclos.Count()) + 1;
            return Escala.Ciclos.FirstOrDefault(c => c.Ciclo == cicloAtual);
        }
    }
}