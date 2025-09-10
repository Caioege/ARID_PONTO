using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class HorarioDeTrabalhoDia : EntidadeOrganizacaoBase
    {
        [Required]
        public int HorarioDeTrabalhoId { get; set; }

        [ForeignKey(nameof(HorarioDeTrabalhoId))]
        public virtual HorarioDeTrabalho HorarioDeTrabalho { get; set; }

        [Required]
        public eDiaDaSemana DiaDaSemana { get; set; }

        public TimeSpan? Entrada1 { get; set; }
        public TimeSpan? Saida1 { get; set; }

        public TimeSpan? Entrada2 { get; set; }
        public TimeSpan? Saida2 { get; set; }

        public TimeSpan? Entrada3 { get; set; }
        public TimeSpan? Saida3 { get; set; }

        public TimeSpan? Entrada4 { get; set; }
        public TimeSpan? Saida4 { get; set; }

        public TimeSpan? Entrada5 { get; set; }
        public TimeSpan? Saida5 { get; set; }

        public TimeSpan? CargaHorariaFixa { get; set; }

        public TimeSpan? CargaHorariaPeriodo(int periodo)
        {
            switch (periodo)
            {
                case 1:
                    if (CargaHorariaFixa.HasValue)
                        return CargaHorariaFixa;

                    return Entrada1.HasValue && Saida1.HasValue ?
                        Saida1.Value.Subtract(Entrada1.Value) :
                        null;

                case 2:
                    if (CargaHorariaFixa.HasValue)
                        return null;

                    return Entrada2.HasValue && Saida2.HasValue ?
                        Saida2.Value.Subtract(Entrada2.Value) :
                        null;

                case 3:
                    if (CargaHorariaFixa.HasValue)
                        return null;

                    return Entrada3.HasValue && Saida3.HasValue ?
                        Saida3.Value.Subtract(Entrada3.Value) :
                        null;

                case 4:
                    if (CargaHorariaFixa.HasValue)
                        return null;

                    return Entrada4.HasValue && Saida4.HasValue ?
                        Saida4.Value.Subtract(Entrada4.Value) :
                        null;

                case 5:
                    if (CargaHorariaFixa.HasValue)
                        return null;

                    return Entrada5.HasValue && Saida5.HasValue ?
                        Saida5.Value.Subtract(Entrada5.Value) :
                        null;

                default:
                    return null;
            }
        }

        public TimeSpan? CalculeCargaHorariaTotal(bool diaFeriadoOuFacultativo)
        {
            if (diaFeriadoOuFacultativo)
                return null;

            if (CargaHorariaFixa.HasValue)
                return CargaHorariaFixa;

            TimeSpan? chPeriodo_1 = Entrada1.HasValue && Saida1.HasValue ?
                Saida1.Value.Subtract(Entrada1.Value) :
                null;

            TimeSpan? chPeriodo_2 = Entrada2.HasValue && Saida2.HasValue ?
                Saida2.Value.Subtract(Entrada2.Value) :
                null;

            TimeSpan? chPeriodo_3 = Entrada3.HasValue && Saida3.HasValue ?
                Saida3.Value.Subtract(Entrada3.Value) :
                null;

            TimeSpan? chPeriodo_4 = Entrada4.HasValue && Saida4.HasValue ?
                Saida4.Value.Subtract(Entrada4.Value) :
                null;

            TimeSpan? chPeriodo_5 = Entrada5.HasValue && Saida5.HasValue ?
                Saida5.Value.Subtract(Entrada5.Value) :
                null;

            TimeSpan? chTotal = null;

            if (chPeriodo_1.HasValue || chPeriodo_2.HasValue || chPeriodo_3.HasValue || chPeriodo_4.HasValue || chPeriodo_5.HasValue)
            {
                chTotal = TimeSpan.Zero;

                if (chPeriodo_1.HasValue)
                    chTotal = chTotal.Value.Add(chPeriodo_1.Value);

                if (chPeriodo_2.HasValue)
                    chTotal = chTotal.Value.Add(chPeriodo_2.Value);

                if (chPeriodo_3.HasValue)
                    chTotal = chTotal.Value.Add(chPeriodo_3.Value);

                if (chPeriodo_4.HasValue)
                    chTotal = chTotal.Value.Add(chPeriodo_4.Value);

                if (chPeriodo_5.HasValue)
                    chTotal = chTotal.Value.Add(chPeriodo_5.Value);
            }

            return chTotal;
        }
    }
}