using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RotaProfissional : EntidadeBase
    {
        public int RotaId { get; set; }

        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public int ServidorId { get; set; }

        [ForeignKey(nameof(ServidorId))]
        public virtual Servidor Servidor { get; set; }

        public string? Observacao { get; set; }
    }
}
