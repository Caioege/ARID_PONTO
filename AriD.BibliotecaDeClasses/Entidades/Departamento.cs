using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Departamento : EntidadeOrganizacaoBase
    {
        [Required, MaxLength(5)]
        public string Sigla { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public string SiglaComDescricao => $"[{Sigla}] {Descricao}";

    }
}