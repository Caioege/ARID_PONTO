using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Motorista : EntidadeOrganizacaoBase
    {
        public int ServidorId { get; set; }
        [ForeignKey(nameof(ServidorId))]
        public virtual Servidor Servidor { get; set; }

        public string NumeroCNH { get; set; }
        public eCategoriaCNH CategoriaCNH { get; set; }
        public DateTime EmissaoCNH { get; set; }
        public DateTime VencimentoCNH { get; set; }
        public eStatusMotorista Status { get; set; }
        public string Observacoes { get; set; }
    }
}