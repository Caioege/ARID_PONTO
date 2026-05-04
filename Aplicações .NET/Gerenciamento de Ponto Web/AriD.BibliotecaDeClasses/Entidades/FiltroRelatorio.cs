using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    [Table("FiltroRelatorio")]
    public class FiltroRelatorio : EntidadeOrganizacaoBase
    {
        [Required]
        public int UsuarioCriadorId { get; set; }

        [Required, MaxLength(100)]
        public string Nome { get; set; }

        [Required, MaxLength(255)]
        public string UrlRelatorio { get; set; }

        [Required]
        public string JsonParametros { get; set; }

        public bool Compartilhado { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [ForeignKey(nameof(UsuarioCriadorId))]
        public virtual Usuario UsuarioCriador { get; set; }
    }
}
