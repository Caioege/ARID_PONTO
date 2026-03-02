using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RegraHoraExtra : EntidadeOrganizacaoBase
    {
        [Required]
        public int HorarioDeTrabalhoId { get; set; }

        [ForeignKey(nameof(HorarioDeTrabalhoId))]
        public virtual HorarioDeTrabalho HorarioDeTrabalho { get; set; }

        [Required]
        public eTipoDiaHoraExtra TipoDia { get; set; }

        // Se true, quando for "DiaFolga" ou "Feriado" (ou até DiaTrabalho se você quiser),
        // a BASE (min(trabalhado, carga)) também vira HE (ex: 100%).
        public bool GerarHoraExtraSobreBaseDaJornada { get; set; }

        // Percentual para a base (ex: 100). Só usado se GerarHoraExtraSobreBaseDaJornada = true.
        public decimal PercentualBase { get; set; } = 0;

        public bool Ativo { get; set; } = true;

        public bool AprovarAutomaticamente { get; set; } = false;

        public virtual List<FaixaHoraExtra> Faixas { get; set; } = new();
    }
}