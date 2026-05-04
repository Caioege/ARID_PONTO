using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ItemDoGrupoDePermissao : EntidadeOrganizacaoBase
    {
        [Required]
        public int GrupoDePermissaoId { get; set; }
        [ForeignKey(nameof(GrupoDePermissaoId))]
        public virtual GrupoDePermissao GrupoDePermissao { get; set; }

        [Required, MaxLength(1000)]
        public string EnumeradorNome { get; set; }
        [Required]
        public int ValorDoEnumerador { get; set; }
        [Required]
        public bool PermissaoAtiva { get; set; }
    }
}