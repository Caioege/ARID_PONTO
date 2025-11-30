using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RegistroAplicativo : EntidadeOrganizacaoBase
    {
        [Required]
        public int VinculoDeTrabalhoId { get; set; }
        [ForeignKey(nameof(VinculoDeTrabalhoId))]
        public virtual VinculoDeTrabalho VinculoDeTrabalho { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? AnexoPonto { get; set; }

        [Required]
        public eSituacaoRegistroAplicativo Situacao { get; set; }

        [Required]
        public bool Manual { get; set; }
        public string? Observacao { get; set; }

        public int? JustificativaDeAusenciaId { get; set; }
        [ForeignKey(nameof(JustificativaDeAusenciaId))]
        public virtual JustificativaDeAusencia JustificativaDeAusencia { get; set; }
        public DateTime? DataInicialAtestado { get; set; }
        public DateTime? DataFinalAtestado { get; set; }

        public bool ForaDaCerca { get; set; }
    }
}