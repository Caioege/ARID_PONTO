using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class LogAuditoriaEscala : EntidadeOrganizacaoBase
    {
        [Required]
        public DateTime DataHora { get; set; }

        [Required]
        [StringLength(100)]
        public string UsuarioNome { get; set; }

        public int? UsuarioId { get; set; }

        [Required, MaxLength(60)]
        public string Acao { get; set; }

        [Required]
        [Column(TypeName = "TEXT")]
        public string Descricao { get; set; }

        public int? EscalaId { get; set; }
        [ForeignKey(nameof(EscalaId))]
        public virtual Escala Escala { get; set; }

        public int? UnidadeOrganizacionalId { get; set; }
        [ForeignKey(nameof(UnidadeOrganizacionalId))]
        public virtual UnidadeOrganizacional UnidadeOrganizacional { get; set; }
    }
}
