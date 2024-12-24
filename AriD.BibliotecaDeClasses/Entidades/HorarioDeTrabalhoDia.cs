using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class HorarioDeTrabalhoDia : EntidadeOrganizacaoBase
    {
        [Required]
        public int HorarioDeTrabalhoId { get; set; }

        [ForeignKey(nameof(HorarioDeTrabalhoId))]
        public virtual HorarioDeTrabalho HorarioDeTrabalho { get; set; }

        [Required]
        public eDiaDaSemana DiaDaSemana { get; set; }

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