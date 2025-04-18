using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Usuario : EntidadeBase
    {
        public int? RedeDeEnsinoId { get; set; }
        [ForeignKey(nameof(RedeDeEnsinoId))]
        public virtual RedeDeEnsino RedeDeEnsino { get; set; }

        [Required, MaxLength(150)]
        public string NomeDaPessoa { get; set; }

        [Required, MaxLength(100)]
        public string UsuarioDeAcesso { get; set; }

        [Required, MaxLength(1000)]
        public string Senha { get; set; }

        [Required]
        public ePerfilDeAcesso PerfilDeAcesso { get; set; }

        [Required]
        public bool Ativo { get; set; }

        public int? GrupoDePermissaoId { get; set; }
        [ForeignKey(nameof(GrupoDePermissaoId))]
        public virtual GrupoDePermissao GrupoDePermissao { get; set; }

        public int? EscolaId { get; set; }
        [ForeignKey(nameof(EscolaId))]
        public virtual Escola Escola { get; set; }
    }
}
