using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    [Table("rotaexecucaodesvio")]
    public class RotaExecucaoDesvio : EntidadeOrganizacaoBase
    {
        public int RotaExecucaoId { get; set; }
        [ForeignKey(nameof(RotaExecucaoId))]
        public virtual RotaExecucao RotaExecucao { get; set; }

        public int? RotaExecucaoLocalizacaoId { get; set; }
        [ForeignKey(nameof(RotaExecucaoLocalizacaoId))]
        public virtual LocalizacaoRota? Localizacao { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal DistanciaEmMetros { get; set; }
        public DateTime DataHoraDeteccao { get; set; }
        public string? Observacao { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
