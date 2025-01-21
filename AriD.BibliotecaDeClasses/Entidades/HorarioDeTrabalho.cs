using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string SiglaComDescricao => $"[{Sigla}] {Descricao}";

        public virtual List<HorarioDeTrabalhoDia> Dias { get; set; }

        public TimeSpan? ObtenhaCargaHorariaDoDia(eDiaDaSemana dia)
            => Dias.FirstOrDefault(c => c.DiaDaSemana == dia)?.CalculeCargaHorariaTotal();
    }
}