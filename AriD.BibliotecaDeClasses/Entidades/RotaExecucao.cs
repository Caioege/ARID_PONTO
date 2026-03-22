using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RotaExecucao : EntidadeOrganizacaoBase
    {
        public int RotaId { get; set; }
        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public DateTime DataHoraInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }

        public int UsuarioIdInicio { get; set; }
        [ForeignKey(nameof(UsuarioIdInicio))]
        public virtual Usuario UsuarioInicio { get; set; }

        public int? UsuarioIdFim { get; set; }
        [ForeignKey(nameof(UsuarioIdFim))]
        public virtual Usuario UsuarioFim { get; set; }
    }
}
