using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class HorarioDeTrabalhoVigencia : EntidadeOrganizacaoBase
    {
        [Required]
        public int HorarioDeTrabalhoId { get; set; }

        [ForeignKey(nameof(HorarioDeTrabalhoId))]
        public virtual HorarioDeTrabalho HorarioDeTrabalho { get; set; }

        [Required]
        public DateTime VigenciaInicio { get; set; }

        public bool UtilizaCincoPeriodos { get; set; }

        public bool UtilizaBancoDeHoras { get; set; }
        public DateTime? InicioBancoDeHoras { get; set; }

        public eTipoCargaHoraria TipoCargaHoraria { get; set; }
        public int? CargaHorariaMensalFixa { get; set; }

        public eIntervaloAutomatico IntervaloAutomatico { get; set; }

        public int ToleranciaDiariaEmMinutos { get; set; } = 0;

        public eColunasDaFolha ColunasVisiveis { get; set; } = eColunasDaFolha.Todas;

        public bool ConsiderarFacultativoComoFeriadoHoraExtra { get; set; } = false;

        public int ToleranciaDsrEmMinutos { get; set; } = 0;

        public bool BancoDeHorasSomenteHorasExtrasAprovadas { get; set; } = true;

        [MaxLength(120)]
        public string BancoDeHorasPrioridadePercentuais { get; set; }

        public virtual List<HorarioDeTrabalhoDia> Dias { get; set; } = new();

        public virtual List<RegraHoraExtra> RegrasHoraExtra { get; set; } = new();

        public bool CargaHorariaFixa => TipoCargaHoraria != eTipoCargaHoraria.EntradaSaida;
    }
}