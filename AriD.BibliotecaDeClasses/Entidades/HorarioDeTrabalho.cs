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
        public HorarioDeTrabalho()
        {
            Dias =
            [
                new() { DiaDaSemana = eDiaDaSemana.Segunda },
                new() { DiaDaSemana = eDiaDaSemana.Terca },
                new() { DiaDaSemana = eDiaDaSemana.Quarta },
                new() { DiaDaSemana = eDiaDaSemana.Quinta },
                new() { DiaDaSemana = eDiaDaSemana.Sexta },
                new() { DiaDaSemana = eDiaDaSemana.Sabado },
                new() { DiaDaSemana = eDiaDaSemana.Domingo }
            ];
        }

        [Required, MaxLength(5)]
        public string Sigla { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        public virtual List<HorarioDeTrabalhoDia> Dias { get; set; }
    }
}