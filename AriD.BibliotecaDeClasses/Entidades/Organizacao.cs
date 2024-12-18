using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Organizacao : EntidadeBase
    {
        public string Nome { get; set; }

        public bool Ativa { get; set; }

        public int EnderecoId { get; set; }

        [ForeignKey(nameof(EnderecoId))]
        public virtual Endereco Endereco { get; set; }

        public int QuantidadeDeUnidadesAtivas { get; set; }
    }
}