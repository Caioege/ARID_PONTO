using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class HorarioDeTrabalho : EntidadeOrganizacaoBase
    {
        [Required, MaxLength(5)]
        public string Sigla { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        public bool Ativo { get; set; }

        public bool UtilizaCincoPeriodos { get; set; }

        public bool UtilizaBancoDeHoras { get; set; }
        public DateTime? InicioBancoDeHoras { get; set; }

        public eTipoCargaHoraria TipoCargaHoraria { get; set; }

        public bool CargaHorariaFixa => TipoCargaHoraria != eTipoCargaHoraria.EntradaSaida;

        public int? CargaHorariaMensalFixa { get; set; }

        public eIntervaloAutomatico IntervaloAutomatico { get; set; }

        public int ToleranciaDiariaEmMinutos { get; set; } = 0;

        public eColunasDaFolha ColunasVisiveis { get; set; } = eColunasDaFolha.Todas;

        public string SiglaComDescricao => $"[{Sigla}] {Descricao}";

        public virtual List<HorarioDeTrabalhoDia> Dias { get; set; }

        public TimeSpan? ObtenhaCargaHorariaDoDia(eDiaDaSemana dia, bool diaFeriadoOuFacultativo)
            => diaFeriadoOuFacultativo || TipoCargaHoraria == eTipoCargaHoraria.MensalFixa ? null : Dias.FirstOrDefault(c => c.DiaDaSemana == dia)?.CalculeCargaHorariaTotal(diaFeriadoOuFacultativo);
    }
}