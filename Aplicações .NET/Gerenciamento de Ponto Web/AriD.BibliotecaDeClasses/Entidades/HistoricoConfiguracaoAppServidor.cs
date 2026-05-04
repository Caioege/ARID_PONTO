using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class HistoricoConfiguracaoAppServidor : EntidadeOrganizacaoBase
    {
        [Required]
        public int ServidorId { get; set; }
        [ForeignKey(nameof(ServidorId))]
        public virtual Servidor Servidor { get; set; }

        public eTipoComprovacaoPontoApp TipoComprovacaoAnterior { get; set; }
        public eTipoComprovacaoPontoApp TipoComprovacaoNova { get; set; }

        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; }

        public DateTime DataAlteracao { get; set; }

        [Required]
        public int UsuarioAlteracaoId { get; set; }
        [ForeignKey(nameof(UsuarioAlteracaoId))]
        public virtual Usuario UsuarioAlteracao { get; set; }
    }
}
