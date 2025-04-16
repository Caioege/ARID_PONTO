using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class FrequenciaAlunoTurma : EntidadeRedeDeEnsinoBase
    {
        [Required]
        public int AlunoTurmaId { get; set; }
        [ForeignKey(nameof(AlunoTurmaId))]
        public virtual AlunoTurma AlunoTurma { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        [Required]
        public bool EstavaPresente { get; set; }
    }
}