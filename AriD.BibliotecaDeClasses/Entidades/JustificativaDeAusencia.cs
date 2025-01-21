using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class JustificativaDeAusencia : EntidadeOrganizacaoBase
    {
        [Required, MaxLength(5)]
        public string Sigla { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        public bool Abono { get; set; }

        public bool Ativa { get; set; }

        public eLocalDeUsoDeJustificativaDeAusencia LocalDeUso { get; set; }

        public string SiglaComDescricao => $"[{Sigla}] {Descricao}";
    }
}