using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    [Table("rotaexecucaolocalizacao")]
    public class LocalizacaoRota : EntidadeOrganizacaoBase
    {
        public int RotaExecucaoId { get; set; }
        [ForeignKey(nameof(RotaExecucaoId))]
        public virtual RotaExecucao RotaExecucao { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal? PrecisaoEmMetros { get; set; }
        public decimal? VelocidadeMetrosPorSegundo { get; set; }
        public decimal? DirecaoGraus { get; set; }
        public decimal? AltitudeMetros { get; set; }
        public bool GpsSimulado { get; set; }
        public int FonteCaptura { get; set; }
        public DateTime DataHoraCaptura { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
