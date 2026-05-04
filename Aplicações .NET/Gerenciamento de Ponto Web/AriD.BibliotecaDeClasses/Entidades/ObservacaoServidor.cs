using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ObservacaoServidor : EntidadeOrganizacaoBase
    {
        [Required]
        public int ServidorId { get; set; }
        [ForeignKey(nameof(ServidorId))]
        public virtual Servidor Servidor { get; set; }

        [Required, MaxLength(3000)]
        public string Texto { get; set; }

        [Required]
        public bool Ativa { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; }

        [Required]
        public DateTime CadastradaEm { get; set; }
    }
}