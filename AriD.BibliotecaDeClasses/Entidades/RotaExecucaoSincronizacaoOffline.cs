using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    [Table("rotaexecucaosincronizacaooffline")]
    public class RotaExecucaoSincronizacaoOffline : EntidadeOrganizacaoBase
    {
        public int RotaExecucaoId { get; set; }
        [ForeignKey(nameof(RotaExecucaoId))]
        public virtual RotaExecucao RotaExecucao { get; set; }

        public string LocalExecucaoId { get; set; }
        public string? ClientEventId { get; set; }
        public string TipoRegistro { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public DateTime? DataHoraRegistroLocal { get; set; }
        public DateTime DataHoraSincronizacao { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
