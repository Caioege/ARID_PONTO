using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class MotoristaHistoricoSituacao : EntidadeOrganizacaoBase
    {
        public int MotoristaId { get; set; }
        [ForeignKey(nameof(MotoristaId))]
        public virtual Motorista Motorista { get; set; }

        public int UsuarioId { get; set; }
        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; }

        public eStatusMotorista SituacaoAnterior { get; set; }
        public eStatusMotorista SituacaoNova { get; set; }
        public DateTime DataAlteracao { get; set; }
    }
}
