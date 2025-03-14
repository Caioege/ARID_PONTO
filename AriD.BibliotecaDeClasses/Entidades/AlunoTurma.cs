using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class AlunoTurma : EntidadeRedeDeEnsinoBase
    {
        [Required]
        public int TurmaId { get; set; }
        [ForeignKey(nameof(TurmaId))]
        public virtual Turma Turma { get; set; }

        [Required]
        public int AlunoId { get; set; }
        [ForeignKey(nameof(AlunoId))]
        public virtual Aluno Aluno { get; set; }

        [Required]
        public DateTime EntradaNaTurma { get; set; }
        [Required]
        public DateTime SaidaNaTurma { get; set; }
    }
}