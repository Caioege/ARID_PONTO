using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class AnexoServidor : EntidadeOrganizacaoBase
    {
        public int ServidorId { get; set; }
        [ForeignKey(nameof(ServidorId))]
        public virtual Servidor Servidor { get; set; }

        [Required, MaxLength(120)]
        public string Descricao { get; set; }

        [Required, MaxLength(200)]
        public string CaminhoArquivo { get; set; }

        public string ExtensaoArquivo 
            => string.IsNullOrEmpty(CaminhoArquivo) ? string.Empty : Path.GetExtension(CaminhoArquivo).Trim();
    }
}