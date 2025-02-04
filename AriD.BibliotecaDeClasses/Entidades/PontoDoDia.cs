using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class PontoDoDia : EntidadeOrganizacaoBase
    {
        public bool DataFutura => Data.Date > DateTime.Today;

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

        public bool PontoFechado { get; set; }

        public TimeSpan? BancoDeHorasCredito { get; set; }
        public TimeSpan? BancoDeHorasDebito { get; set; }

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

        public TimeSpan? CargaHoraria { get; set; }

        public TimeSpan? HorasTrabalhadas { get; set; }
        public TimeSpan? HorasTrabalhadasConsiderandoAbono { get; set; }
        public TimeSpan? HorasPositivas { get; set; }

        public TimeSpan? HorasNegativas { get; set; }

        public int? AfastamentoId { get; set; }
        [ForeignKey(nameof(AfastamentoId))]
        public virtual Afastamento Afastamento { get; set; }

        public string DescricaoEntrada(int periodo)
        {
            if (!string.IsNullOrEmpty(Afastamento?.JustificativaDeAusencia?.Sigla))
            {
                return Afastamento?.JustificativaDeAusencia?.Sigla;
            }

            string retorno = string.Empty;
            switch (periodo)
            {
                case 1:
                    retorno = Entrada1.HasValue ?
                        Entrada1.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo1Id.HasValue ?
                            JustificativaPeriodo1.Sigla :
                    string.Empty;

                    //retorno += TipoEntrada1.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 2:
                    retorno = Entrada2.HasValue ?
                        Entrada2.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo2Id.HasValue ?
                            JustificativaPeriodo2.Sigla :
                    string.Empty;

                    //retorno += TipoEntrada2.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 3:
                    retorno = Entrada3.HasValue ?
                        Entrada3.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo3Id.HasValue ?
                            JustificativaPeriodo3.Sigla :
                            string.Empty;

                    //retorno += TipoEntrada3.DescricaoTipoDeRegistroDoEnumerador();
                    break;


                case 4:
                    retorno = Entrada4.HasValue ?
                        Entrada4.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo4Id.HasValue ?
                            JustificativaPeriodo4.Sigla :
                            string.Empty;

                    //retorno += TipoEntrada4.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 5:
                    retorno = Entrada5.HasValue ?
                        Entrada5.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo5Id.HasValue ?
                            JustificativaPeriodo5.Sigla :
                            string.Empty;

                    //retorno += TipoEntrada5.DescricaoTipoDeRegistroDoEnumerador();
                    break;
            }

            return retorno;
        }

        public string DescricaoSaida(int periodo)
        {
            if (!string.IsNullOrEmpty(Afastamento?.JustificativaDeAusencia?.Sigla))
            {
                return Afastamento?.JustificativaDeAusencia?.Sigla;
            }

            string retorno = string.Empty;
            switch (periodo)
            {
                case 1:
                    retorno = Saida1.HasValue ?
                        Saida1.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo1Id.HasValue ?
                            JustificativaPeriodo1.Sigla :
                            string.Empty;

                    //retorno += TipoSaida1.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 2:
                    retorno = Saida2.HasValue ?
                        Saida2.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo2Id.HasValue ?
                            JustificativaPeriodo2.Sigla :
                            string.Empty;

                    //retorno += TipoSaida2.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 3:
                    retorno = Saida3.HasValue ?
                        Saida3.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo3Id.HasValue ?
                            JustificativaPeriodo3.Sigla :
                            string.Empty;

                    //retorno += TipoSaida3.DescricaoTipoDeRegistroDoEnumerador();
                    break;
                case 4:
                    retorno = Saida4.HasValue ?
                        Saida4.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo4Id.HasValue ?
                            JustificativaPeriodo4.Sigla :
                            string.Empty;

                    //retorno += TipoSaida4.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 5:
                    retorno = Saida5.HasValue ?
                        Saida5.Value.ToString(@"hh\:mm") :
                        JustificativaPeriodo5Id.HasValue ?
                            JustificativaPeriodo5.Sigla :
                            string.Empty;

                    //retorno += TipoSaida5.DescricaoTipoDeRegistroDoEnumerador();

                    break;

            }

            return retorno;
        }
    }
}