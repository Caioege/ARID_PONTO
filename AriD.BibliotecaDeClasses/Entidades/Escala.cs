using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Escala : EntidadeOrganizacaoBase
    {
        [Required]
        public eTipoDeEscala Tipo { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        public virtual List<CicloDaEscala> Ciclos { get; set; } 
            = new List<CicloDaEscala>();

        public virtual List<EscalaDoServidor> EscalaDoServidor { get; set; } 
            = new List<EscalaDoServidor>();
    }
}