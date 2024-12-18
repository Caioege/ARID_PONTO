using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Servidor : EntidadeOrganizacaoBase
    {
        public int PessoaId { get; set; }

        [ForeignKey(nameof(PessoaId))]
        public virtual Pessoa Pessoa { get; set; }

        public DateTime DataDeCadastro { get; set; }
    }
}