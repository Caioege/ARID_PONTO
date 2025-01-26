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
    }
}