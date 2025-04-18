using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Aluno : EntidadeRedeDeEnsinoBase
    {
        [Required]
        public int PessoaId { get; set; }

        [ForeignKey(nameof(PessoaId))]
        public virtual Pessoa Pessoa { get; set; }

        [MaxLength(12)]
        public string? IdEquipamento { get; set; }

        public int? EscolaId { get; set; }
        [ForeignKey(nameof(EscolaId))]
        public virtual Escola Escola { get; set; }

        [Required]
        public DateTime DataDeCadastro { get; set; }

        [Required]
        public bool ConcluiuOsEstudos { get; set; }

        [Required]
        public eAnoEscolar AnoEscolarAtual { get; set; }

        [MaxLength(150)]
        public string? NomeMae { get; set; }
        [MaxLength(15)]
        public string? TelefoneMae { get; set; }

        [MaxLength(150)]
        public string? NomePai { get; set; }
        [MaxLength(15)]
        public string? TelefonePai { get; set; }

        [MaxLength(150)]
        public string? NomeOutroResponsavel { get; set; }
        [MaxLength(15)]
        public string? TelefoneOutroResponsavel { get; set; }
        [MaxLength(50)]
        public string? ParentescoOutroResponsavel { get; set; }

        public string Nome => Pessoa?.Nome;

        public virtual List<AlunoTurma> ListaDeVinculosDeTurma { get; set; } = new();

        public AlunoTurma AlunoTurmaAtual => 
            ListaDeVinculosDeTurma
            .FirstOrDefault(c => c.Situacao == eSituacaoAlunoNaTurma.Cursando);
    }
}