using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Turma : EntidadeRedeDeEnsinoBase
    {
        [Required]
        public int EscolaId { get; set; }
        [ForeignKey(nameof(EscolaId))]
        public virtual Escola Escola { get; set; }

        [Required]
        public int AnoLetivo { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }
        [Required]
        public eTurno Turno { get; set; }
        [Required]
        public eSituacaoTurma Situacao { get; set; }
        
        [Required]
        public eAnoEscolar AnoEscolar { get; set; }

        [Required]
        public DateTime InicioDasAulas { get; set; }
        [Required]
        public DateTime FimDasAulas { get; set; }

        [Required]
        public int DiasLetivos { get; set; }

        public virtual List<AlunoTurma> ListaDeAlunos { get; set; } = new();

        public virtual List<ItemHorarioDeAula> ListaDeHorarioDeAula { get; set; } = new();

        public string DescricaoComTurnoAnoLetivo => $"{Descricao} - {Turno.ToString()} - {AnoLetivo}";
        public string DescricaoComTurno => $"{Descricao} - {Turno.ToString()}";
    }
}