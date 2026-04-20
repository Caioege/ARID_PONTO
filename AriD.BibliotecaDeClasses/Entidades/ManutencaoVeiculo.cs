using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ManutencaoVeiculo : EntidadeOrganizacaoBase
    {
        [Required]
        public int VeiculoId { get; set; }

        [ForeignKey(nameof(VeiculoId))]
        public virtual Veiculo Veiculo { get; set; }

        [Required]
        public DateTime DataManutencao { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        public int KmNaManutencao { get; set; }

        public int? KmProximaManutencao { get; set; }

        public DateTime? DataVencimentoManutencao { get; set; }

        public string? Observacao { get; set; }

        public eSituacaoManutencao Situacao { get; set; }
    }
}
