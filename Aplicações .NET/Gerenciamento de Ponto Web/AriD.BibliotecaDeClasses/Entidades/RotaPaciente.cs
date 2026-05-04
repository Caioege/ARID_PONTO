using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RotaPaciente : EntidadeBase
    {
        public int RotaId { get; set; }

        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public int PacienteId { get; set; }

        [ForeignKey(nameof(PacienteId))]
        public virtual Paciente Paciente { get; set; }

        public bool PossuiAcompanhante { get; set; }
    }
}
