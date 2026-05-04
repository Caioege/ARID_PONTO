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

        public string? Categoria { get; set; }

        public int KmNaManutencao { get; set; }

        public int? KmProximaManutencao { get; set; }

        public DateTime? DataVencimentoManutencao { get; set; }

        public DateTime? DataAgendamento { get; set; }

        public DateTime? DataConclusao { get; set; }

        public DateTime? GarantiaAte { get; set; }

        public string? LocalExecucaoServico { get; set; }

        public string? Fornecedor { get; set; }

        public string? ResponsavelServico { get; set; }

        public string? ContatoFornecedor { get; set; }

        public string? NumeroDocumento { get; set; }

        public decimal? CustoPrevisto { get; set; }

        public decimal? ValorMaximoAutorizado { get; set; }

        public decimal? ValorTotalGasto { get; set; }

        public string? Observacao { get; set; }

        public eSituacaoManutencao Situacao { get; set; }
    }
}
