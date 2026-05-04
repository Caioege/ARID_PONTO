using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class CicloDaEscala : EntidadeOrganizacaoBase
    {
        [Required]
        public int EscalaId { get; set; }
        [ForeignKey(nameof(EscalaId))]
        public virtual Escala Escala { get; set; }

        [Required]
        public int Ciclo { get; set; }

        public TimeSpan? Entrada1 { get; set; }
        public TimeSpan? Saida1 { get; set; }

        public TimeSpan? Entrada2 { get; set; }
        public TimeSpan? Saida2 { get; set; }

        public TimeSpan? Entrada3 { get; set; }
        public TimeSpan? Saida3 { get; set; }

        public TimeSpan? Entrada4 { get; set; }
        public TimeSpan? Saida4 { get; set; }

        public TimeSpan? Entrada5 { get; set; }
        public TimeSpan? Saida5 { get; set; }

        public HorarioDeTrabalhoDia ObtenhaHorarioDia()
            => new()
            {
                Entrada1 = Entrada1,
                Entrada2 = Entrada2,
                Entrada3 = Entrada3,
                Entrada4 = Entrada4,
                Entrada5 = Entrada5,
                Saida1 = Saida1,
                Saida2 = Saida2,
                Saida3 = Saida3,
                Saida4 = Saida4,
                Saida5 = Saida5,
            };
    }
}