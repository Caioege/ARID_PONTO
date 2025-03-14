using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Escala : EntidadeOrganizacaoBase
    {
        [Required]
        public eTipoDeEscala Tipo { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        [Required]
        public int UnidadeOrganizacionalId { get; set; }
        [ForeignKey(nameof(UnidadeOrganizacionalId))]
        public virtual UnidadeOrganizacional UnidadeOrganizacional { get; set; }

        public virtual List<CicloDaEscala> Ciclos { get; set; } 
            = new List<CicloDaEscala>();

        public virtual List<EscalaDoServidor> EscalaDoServidor { get; set; } 
            = new List<EscalaDoServidor>();
    }
}