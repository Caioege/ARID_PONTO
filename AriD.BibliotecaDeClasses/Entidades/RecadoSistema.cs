using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RecadoSistema : EntidadeOrganizacaoBase
    {
        public RecadoSistema()
        {
            ListaDeUsuariosQueLeram = new List<Usuario>();
        }

        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; }

        [Required, MaxLength(5000)]
        public string Mensagem { get; set; }

        [Required]
        public DateTime DataHoraCadastro { get; set; }

        [Required]
        public bool Ativo { get; set; }

        public virtual List<Usuario> ListaDeUsuariosQueLeram { get; set; }
    }
}