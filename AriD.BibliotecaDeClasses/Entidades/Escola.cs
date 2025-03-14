using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Escola : EntidadeRedeDeEnsinoBase
    {
        [Required]
        public string Nome { get; set; }
        public string? CNPJ { get; set; }

        [Required]
        public string CodigoInep { get; set; }

        public int EnderecoId { get; set; }

        [ForeignKey(nameof(EnderecoId))]
        public virtual Endereco Endereco { get; set; }

        public eTipoEscola? Tipo { get; set; }

        public bool Ativa { get; set; }
    }
}
