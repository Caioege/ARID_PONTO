using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class PontoDoDia : EntidadeOrganizacaoBase
    {
        [Required]
        public int VinculoDeTrabalhoId { get; set; }
        [ForeignKey(nameof(VinculoDeTrabalhoId))]
        public virtual VinculoDeTrabalho VinculoDeTrabalho { get; set; }

        [Required]
        public DateTime Data { get; set; }

        public eDiaDaSemana DiaDaSemana => (eDiaDaSemana)Data.DayOfWeek;

        public TimeSpan? Entrada1 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada1 { get; set; }
        public TimeSpan? Saida1 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida1 { get; set; }

        public TimeSpan? Entrada2 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada2 { get; set; }
        public TimeSpan? Saida2 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida2 { get; set; }

        public TimeSpan? Entrada3 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada3 { get; set; }
        public TimeSpan? Saida3 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida3 { get; set; }

        public TimeSpan? Entrada4 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada4 { get; set; }
        public TimeSpan? Saida4 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida4 { get; set; }

        public TimeSpan? Entrada5 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada5 { get; set; }
        public TimeSpan? Saida5 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida5 { get; set; }

        public TimeSpan? Abono { get; set; }

        public int? JustificativaPeriodo1Id { get; set; }
        [ForeignKey(nameof(JustificativaPeriodo1Id))]
        public virtual JustificativaDeAusencia JustificativaPeriodo1 { get; set; }

        public int? JustificativaPeriodo2Id { get; set; }
        [ForeignKey(nameof(JustificativaPeriodo2Id))]
        public virtual JustificativaDeAusencia JustificativaPeriodo2 { get; set; }

        public int? JustificativaPeriodo3Id { get; set; }
        [ForeignKey(nameof(JustificativaPeriodo3Id))]
        public virtual JustificativaDeAusencia JustificativaPeriodo3 { get; set; }

        public int? JustificativaPeriodo4Id { get; set; }
        [ForeignKey(nameof(JustificativaPeriodo4Id))]
        public virtual JustificativaDeAusencia JustificativaPeriodo4 { get; set; }

        public int? JustificativaPeriodo5Id { get; set; }
        [ForeignKey(nameof(JustificativaPeriodo5Id))]
        public virtual JustificativaDeAusencia JustificativaPeriodo5 { get; set; }

        public TimeSpan? HorasTrabalhadas
        {
            get
            {
                TimeSpan? htPeriodo_1 = Entrada1.HasValue && Saida1.HasValue ?
                Saida1.Value.Subtract(Entrada1.Value) :
                null;

                TimeSpan? htPeriodo_2 = Entrada2.HasValue && Saida2.HasValue ?
                    Saida2.Value.Subtract(Entrada2.Value) :
                    null;

                TimeSpan? htPeriodo_3 = Entrada3.HasValue && Saida3.HasValue ?
                    Saida3.Value.Subtract(Entrada3.Value) :
                    null;

                TimeSpan? htPeriodo_4 = Entrada4.HasValue && Saida4.HasValue ?
                    Saida4.Value.Subtract(Entrada4.Value) :
                    null;

                TimeSpan? htPeriodo_5 = Entrada5.HasValue && Saida5.HasValue ?
                    Saida5.Value.Subtract(Entrada5.Value) :
                    null;

                TimeSpan? horasTrabalhas = null;

                if (htPeriodo_1.HasValue || htPeriodo_2.HasValue || htPeriodo_3.HasValue || htPeriodo_4.HasValue || htPeriodo_5.HasValue)
                {
                    horasTrabalhas = TimeSpan.Zero;

                    if (htPeriodo_1.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_1.Value);

                    if (htPeriodo_2.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_2.Value);

                    if (htPeriodo_3.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_3.Value);

                    if (htPeriodo_4.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_4.Value);

                    if (htPeriodo_5.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_5.Value);
                }

                return horasTrabalhas;
            }
        }

        public TimeSpan? HorasTrabalhadasConsiderandoAbono
        {
            get
            {
                if (VinculoDeTrabalho == null || VinculoDeTrabalho.HorarioDeTrabalho == null)
                    return HorasTrabalhadas;

                var horarioDia = VinculoDeTrabalho.HorarioDeTrabalho.Dias.FirstOrDefault(c => c.DiaDaSemana == DiaDaSemana);

                if (horarioDia == null)
                    return HorasTrabalhadas;

                TimeSpan? htPeriodo_1 = null;
                TimeSpan? htPeriodo_2 = null;
                TimeSpan? htPeriodo_3 = null;
                TimeSpan? htPeriodo_4 = null;
                TimeSpan? htPeriodo_5 = null;

                var afastamento = ObtenhaAfastamentoNoPeriodo();
                if (afastamento != null)
                {
                    if (afastamento.JustificativaDeAusencia.Abono)
                    {
                        htPeriodo_1 = horarioDia.CargaHorariaPeriodo(1);
                        htPeriodo_2 = horarioDia.CargaHorariaPeriodo(2);
                        htPeriodo_3 = horarioDia.CargaHorariaPeriodo(3);
                        htPeriodo_4 = horarioDia.CargaHorariaPeriodo(4);
                        htPeriodo_5 = horarioDia.CargaHorariaPeriodo(5);
                    }
                }
                else
                {
                    htPeriodo_1 = Entrada1.HasValue && Saida1.HasValue ?
                    Saida1.Value.Subtract(Entrada1.Value) :
                    JustificativaPeriodo1Id.HasValue && JustificativaPeriodo1.Abono ?
                        horarioDia.CargaHorariaPeriodo(1) :
                        null;

                    htPeriodo_2 = Entrada2.HasValue && Saida2.HasValue ?
                        Saida2.Value.Subtract(Entrada2.Value) :
                        JustificativaPeriodo2Id.HasValue && JustificativaPeriodo2.Abono ?
                            horarioDia.CargaHorariaPeriodo(2) :
                            null;

                    htPeriodo_3 = Entrada3.HasValue && Saida3.HasValue ?
                        Saida3.Value.Subtract(Entrada3.Value) :
                        JustificativaPeriodo3Id.HasValue && JustificativaPeriodo3.Abono ?
                            horarioDia.CargaHorariaPeriodo(3) :
                            null;

                    htPeriodo_4 = Entrada4.HasValue && Saida4.HasValue ?
                        Saida4.Value.Subtract(Entrada4.Value) :
                        JustificativaPeriodo4Id.HasValue && JustificativaPeriodo4.Abono ?
                            horarioDia.CargaHorariaPeriodo(4) :
                            null;

                    htPeriodo_5 = Entrada5.HasValue && Saida5.HasValue ?
                        Saida5.Value.Subtract(Entrada5.Value) :
                        JustificativaPeriodo5Id.HasValue && JustificativaPeriodo5.Abono ?
                            horarioDia.CargaHorariaPeriodo(5) :
                            null;
                }

                TimeSpan? horasTrabalhas = null;

                if (htPeriodo_1.HasValue || htPeriodo_2.HasValue || htPeriodo_3.HasValue || htPeriodo_4.HasValue || htPeriodo_5.HasValue)
                {
                    horasTrabalhas = TimeSpan.Zero;

                    if (htPeriodo_1.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_1.Value);

                    if (htPeriodo_2.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_2.Value);

                    if (htPeriodo_3.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_3.Value);

                    if (htPeriodo_4.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_4.Value);

                    if (htPeriodo_5.HasValue)
                        horasTrabalhas = horasTrabalhas.Value.Add(htPeriodo_5.Value);
                }

                return horasTrabalhas;
            }
        }

        public TimeSpan? HorasPositivas
        {
            get
            {
                if (VinculoDeTrabalho == null || VinculoDeTrabalho.HorarioDeTrabalho == null)
                    return null;

                var horarioDia = VinculoDeTrabalho.HorarioDeTrabalho.Dias.FirstOrDefault(c => c.DiaDaSemana == DiaDaSemana);

                if (horarioDia == null)
                    return null;

                var cargaHorariaDoDia = horarioDia.CalculeCargaHorariaTotal();

                if (!cargaHorariaDoDia.HasValue && !HorasTrabalhadasConsiderandoAbono.HasValue)
                    return null;

                if (!cargaHorariaDoDia.HasValue && HorasTrabalhadasConsiderandoAbono.HasValue)
                    return HorasTrabalhadasConsiderandoAbono;

                if (HorasTrabalhadasConsiderandoAbono > cargaHorariaDoDia)
                    return HorasTrabalhadasConsiderandoAbono.Value.Subtract(cargaHorariaDoDia.Value);

                return null;
            }
        }

        public TimeSpan? HorasNegativas
        {
            get
            {
                if (VinculoDeTrabalho == null || VinculoDeTrabalho.HorarioDeTrabalho == null)
                    return null;

                var horarioDia = VinculoDeTrabalho.HorarioDeTrabalho.Dias.FirstOrDefault(c => c.DiaDaSemana == DiaDaSemana);

                if (horarioDia == null)
                    return null;

                var cargaHorariaDoDia = horarioDia.CalculeCargaHorariaTotal();

                if (!cargaHorariaDoDia.HasValue && !HorasTrabalhadasConsiderandoAbono.HasValue)
                    return null;

                if (cargaHorariaDoDia > ((HorasTrabalhadasConsiderandoAbono ?? TimeSpan.Zero) + (Abono ?? TimeSpan.Zero)))
                    return cargaHorariaDoDia.Value.Subtract((HorasTrabalhadasConsiderandoAbono ?? TimeSpan.Zero) + (Abono ?? TimeSpan.Zero));

                return null;
            }
        }

        public Afastamento ObtenhaAfastamentoNoPeriodo() => VinculoDeTrabalho?.Afastamentos?.FirstOrDefault(d => d.Inicio.Date <= Data.Date && (!d.Fim.HasValue || d.Fim.Value.Date >= Data.Date));
        public string SiglaAfastamentoNoDia()=> ObtenhaAfastamentoNoPeriodo()?.JustificativaDeAusencia?.Sigla;
    }
}