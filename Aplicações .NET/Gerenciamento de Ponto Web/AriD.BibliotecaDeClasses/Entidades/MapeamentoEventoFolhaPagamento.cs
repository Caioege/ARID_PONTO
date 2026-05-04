using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class MapeamentoEventoFolhaPagamento : EntidadeOrganizacaoBase
    {
        [Required]
        public eTipoEventoFolhaPagamento TipoEvento { get; set; }

        // Para evoluir depois com HE 50/70/100.
        public decimal? Percentual { get; set; }

        [Required, MaxLength(30)]
        public string Codigo { get; set; } = "";

        [MaxLength(120)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;
    }
}