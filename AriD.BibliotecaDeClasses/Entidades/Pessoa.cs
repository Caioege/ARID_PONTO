using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Pessoa : EntidadeRedeDeEnsinoBase
    {
        [Required, MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(100)]
        public string? NomeSocial { get; set; }

        [Required, MaxLength(14)]
        public string Cpf { get; set; }

        [MaxLength(10)]
        public string? Rg { get; set; }

        public DateTime? DataDeNascimento { get; set; }

        [Required]
        public int EnderecoId { get; set; }
        [ForeignKey(nameof(EnderecoId))]
        public virtual Endereco Endereco { get; set; }
    }
}
