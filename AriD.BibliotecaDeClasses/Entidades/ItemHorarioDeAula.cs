using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ItemHorarioDeAula : EntidadeRedeDeEnsinoBase
    {
        [Required]
        public int TurmaId { get; set; }
        [ForeignKey(nameof(TurmaId))]
        public virtual Turma Turma { get; set; }

        [Required]
        public eDiaDaSemana DiaDaSemana { get; set; }
        [MaxLength(100)]
        public string? Disciplina { get; set; }
        [Required]
        public TimeSpan InicioAula { get; set; }
        [Required]
        public TimeSpan FimAula { get; set; }
        [Required]
        public bool Intervalo { get; set; }
    }
}