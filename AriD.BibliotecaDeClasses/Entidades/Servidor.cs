using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Servidor : EntidadeOrganizacaoBase
    {
        [Required]
        public int PessoaId { get; set; }

        [ForeignKey(nameof(PessoaId))]
        public virtual Pessoa Pessoa { get; set; }

        [Required]
        public DateTime DataDeCadastro { get; set; }

        public virtual List<VinculoDeTrabalho> VinculosDeTrabalho { get; set; }

        public string Nome => Pessoa?.Nome;
    }
}