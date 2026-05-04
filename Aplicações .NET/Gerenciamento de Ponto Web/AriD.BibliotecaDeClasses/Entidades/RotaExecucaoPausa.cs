using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    [Table("rotaexecucaopausa")]
    public class RotaExecucaoPausa : EntidadeOrganizacaoBase
    {
        public int RotaExecucaoId { get; set; }
        [ForeignKey(nameof(RotaExecucaoId))]
        public virtual RotaExecucao RotaExecucao { get; set; }

        public string Motivo { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public string? LatitudeInicio { get; set; }
        public string? LongitudeInicio { get; set; }
        public bool GpsSimuladoInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public string? LatitudeFim { get; set; }
        public string? LongitudeFim { get; set; }
        public bool GpsSimuladoFim { get; set; }
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
