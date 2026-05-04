using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    [Table("rotaexecucaoevento")]
    public class RotaExecucaoEvento : EntidadeOrganizacaoBase
    {
        public int RotaExecucaoId { get; set; }
        [ForeignKey(nameof(RotaExecucaoId))]
        public virtual RotaExecucao RotaExecucao { get; set; }

        public int? ParadaRotaId { get; set; }
        [ForeignKey(nameof(ParadaRotaId))]
        public virtual ParadaRota? ParadaRota { get; set; }

        public int? UnidadeId { get; set; }
        [ForeignKey(nameof(UnidadeId))]
        public virtual UnidadeOrganizacional? Unidade { get; set; }

        public int Sequencia { get; set; }
        public int TipoEvento { get; set; }
        public int? StatusEvento { get; set; }
        public bool? Entregue { get; set; }
        public string? Observacao { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool GpsSimulado { get; set; }
        public DateTime DataHoraEvento { get; set; }
        public bool RegistradoOffline { get; set; }
        public DateTime? DataHoraRegistroLocal { get; set; }
        public DateTime? DataHoraSincronizacao { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public string? LocalExecucaoId { get; set; }
        public string? ClientEventId { get; set; }
        public int? UsuarioIdRegistro { get; set; }
        [ForeignKey(nameof(UsuarioIdRegistro))]
        public virtual Usuario? UsuarioRegistro { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
