using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class EventoAnual : EntidadeOrganizacaoBase
    {
        [Required, MaxLength(150)]
        public string Descricao { get; set; }
        [Required]
        public eTipoDeEvento Tipo { get; set; }
        [Required]
        public DateTime Data { get; set; }
    }
}