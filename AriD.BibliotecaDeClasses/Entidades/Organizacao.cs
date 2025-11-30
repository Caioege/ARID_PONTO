using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
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

        [Required]
        public eNomenclaturaServidor NomenclaturaServidor { get; set; }

        [Required]
        public bool EnvioDeMensagemWhatsAppExperimental { get; set; }
    }
}