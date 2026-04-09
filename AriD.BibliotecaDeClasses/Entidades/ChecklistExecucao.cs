using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ChecklistExecucao : EntidadeOrganizacaoBase
    {
        public int VeiculoId { get; set; }
        
        [ForeignKey(nameof(VeiculoId))]
        public virtual Veiculo Veiculo { get; set; }

        public int MotoristaId { get; set; }
        
        [ForeignKey(nameof(MotoristaId))]
        public virtual Motorista Motorista { get; set; }

        public int? RotaId { get; set; }
        
        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public DateTime DataHora { get; set; }

        public virtual ICollection<ChecklistExecucaoItem> Itens { get; set; }
    }
}
