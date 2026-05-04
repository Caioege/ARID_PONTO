using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.GerenciamentoDePonto.Models
{
    public class HorarioDeTrabalhoCadastroVM
    {
        public int Id { get; set; }
        public string Sigla { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }

        public int VigenciaId { get; set; }
        public DateTime VigenciaInicio { get; set; }

        public bool UtilizaCincoPeriodos { get; set; }
        public bool UtilizaBancoDeHoras { get; set; }
        public DateTime? InicioBancoDeHoras { get; set; }

        public eTipoCargaHoraria TipoCargaHoraria { get; set; }
        public int? CargaHorariaMensalFixa { get; set; }
        public eIntervaloAutomatico IntervaloAutomatico { get; set; }

        public int ToleranciaDiariaEmMinutos { get; set; }
        public eColunasDaFolha ColunasVisiveis { get; set; }

        public bool ConsiderarFacultativoComoFeriadoHoraExtra { get; set; }
        public int ToleranciaDsrEmMinutos { get; set; }

        public int ToleranciaAntesDaEntradaEmMinutos { get; set; }
        public int ToleranciaAposAEntradaEmMinutos { get; set; }
        public int ToleranciaAntesDaSaidaEmMinutos { get; set; }
        public int ToleranciaAposASaidaEmMinutos { get; set; }

        public bool BancoDeHorasSomenteHorasExtrasAprovadas { get; set; }
        public string BancoDeHorasPrioridadePercentuais { get; set; }

        public System.Collections.Generic.List<HorarioDeTrabalhoDia> Dias { get; set; } = new();
        public System.Collections.Generic.List<RegraHoraExtra> RegrasHoraExtra { get; set; } = new();

        public bool CargaHorariaFixa => TipoCargaHoraria != eTipoCargaHoraria.EntradaSaida;
    }
}