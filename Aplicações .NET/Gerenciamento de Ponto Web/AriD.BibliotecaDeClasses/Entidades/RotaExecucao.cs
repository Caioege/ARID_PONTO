using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    [Table("rotaexecucao")]
    public class RotaExecucao : EntidadeOrganizacaoBase
    {
        public int RotaId { get; set; }
        [ForeignKey(nameof(RotaId))]
        public virtual Rota Rota { get; set; }

        public int MotoristaId { get; set; }
        [ForeignKey(nameof(MotoristaId))]
        public virtual Motorista Motorista { get; set; }

        public int VeiculoId { get; set; }
        [ForeignKey(nameof(VeiculoId))]
        public virtual Veiculo Veiculo { get; set; }

        public int? ChecklistExecucaoId { get; set; }
        [ForeignKey(nameof(ChecklistExecucaoId))]
        public virtual ChecklistExecucao? ChecklistExecucao { get; set; }

        public int Status { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime? DataHoraFim { get; set; }
        public int? UsuarioIdInicio { get; set; }
        [ForeignKey(nameof(UsuarioIdInicio))]
        public virtual Usuario? UsuarioInicio { get; set; }
        public int? UsuarioIdFim { get; set; }
        [ForeignKey(nameof(UsuarioIdFim))]
        public virtual Usuario? UsuarioFim { get; set; }

        public string? ObservacaoInicio { get; set; }
        public string? ObservacaoFim { get; set; }
        public string? UltimaLatitude { get; set; }
        public string? UltimaLongitude { get; set; }
        public DateTime? UltimaAtualizacaoEm { get; set; }
        public bool GpsSimuladoUltimaLeitura { get; set; }
        public bool PossuiRegistroOffline { get; set; }
        public bool ExecucaoOfflineCompleta { get; set; }
        public DateTime? DataHoraPrimeiroRegistroOffline { get; set; }
        public DateTime? DataHoraUltimoRegistroOffline { get; set; }
        public DateTime? DataHoraUltimaComunicacaoApp { get; set; }
        public string? LocalExecucaoId { get; set; }
        public string? IdentificadorDispositivo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAlteracao { get; set; }
    }
}
