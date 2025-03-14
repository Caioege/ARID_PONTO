using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class GrupoDePermissao : EntidadeRedeDeEnsinoBase
    {
        [Required, MaxLength(5)]
        public string Sigla { get; set; }
        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        [Required]
        public ePerfilDeAcesso PerfilDeAcesso { get; set; }

        [Required]
        public bool Ativo { get; set; }

        public string SiglaComDescricao => $"[{Sigla}] {Descricao}";

        public virtual List<ItemDoGrupoDePermissao> ListaDePermissao { get; set; } 
            = new();
    }
}
