using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RotaVeiculo : EntidadeOrganizacaoBase
    {
        public int RotaId { get; set; }
        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public int VeiculoId { get; set; }
        [ForeignKey(nameof(VeiculoId))]
        public virtual Veiculo Veiculo { get; set; }
    }
}
