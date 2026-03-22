using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Rota : EntidadeOrganizacaoBase
    {
        public int MotoristaId { get; set; }
        [ForeignKey(nameof(MotoristaId))]
        public virtual Motorista Motorista { get; set; }

        public int? VeiculoId { get; set; }
        [ForeignKey(nameof(VeiculoId))]
        public virtual Veiculo Veiculo { get; set; }

        public string Descricao { get; set; }
        
        public eStatusRota Situacao { get; set; }
        public bool Recorrente { get; set; }

        public virtual ICollection<ParadaRota> Paradas { get; set; } = new List<ParadaRota>();
        public virtual ICollection<RotaExecucao> Execucoes { get; set; } = new List<RotaExecucao>();
    }
}
