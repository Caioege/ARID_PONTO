using AriD.BibliotecaDeClasses.Atributos;
using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

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
        public int? RegistroDePontoEntrada1Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoEntrada1Id))]
        public virtual RegistroDePonto RegistroDePontoEntrada1 { get; set; }

        public TimeSpan? Saida1 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida1 { get; set; }
        public int? RegistroDePontoSaida1Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoSaida1Id))]
        public virtual RegistroDePonto RegistroDePontoSaida1 { get; set; }

        public TimeSpan? Entrada2 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada2 { get; set; }
        public int? RegistroDePontoEntrada2Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoEntrada2Id))]
        public virtual RegistroDePonto RegistroDePontoEntrada2 { get; set; }

        public TimeSpan? Saida2 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida2 { get; set; }
        public int? RegistroDePontoSaida2Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoSaida2Id))]
        public virtual RegistroDePonto RegistroDePontoSaida2 { get; set; }

        public TimeSpan? Entrada3 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada3 { get; set; }
        public int? RegistroDePontoEntrada3Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoEntrada3Id))]
        public virtual RegistroDePonto RegistroDePontoEntrada3 { get; set; }

        public TimeSpan? Saida3 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida3 { get; set; }
        public int? RegistroDePontoSaida3Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoSaida3Id))]
        public virtual RegistroDePonto RegistroDePontoSaida3 { get; set; }

        public TimeSpan? Entrada4 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada4 { get; set; }
        public int? RegistroDePontoEntrada4Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoEntrada4Id))]
        public virtual RegistroDePonto RegistroDePontoEntrada4 { get; set; }

        public TimeSpan? Saida4 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida4 { get; set; }
        public int? RegistroDePontoSaida4Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoSaida4Id))]
        public virtual RegistroDePonto RegistroDePontoSaida4 { get; set; }

        public TimeSpan? Entrada5 { get; set; }
        public eTipoDeRegistroDePeriodo TipoEntrada5 { get; set; }
        public int? RegistroDePontoEntrada5Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoEntrada5Id))]
        public virtual RegistroDePonto RegistroDePontoEntrada5 { get; set; }

        public TimeSpan? Saida5 { get; set; }
        public eTipoDeRegistroDePeriodo TipoSaida5 { get; set; }
        public int? RegistroDePontoSaida5Id { get; set; }
        [ForeignKey(nameof(RegistroDePontoSaida5Id))]
        public virtual RegistroDePonto RegistroDePontoSaida5 { get; set; }

        public TimeSpan? Abono { get; set; }

        public bool PontoFechado { get; set; }

        public TimeSpan? BancoDeHorasCredito { get; set; }
        public TimeSpan? BancoDeHorasDebito { get; set; }

        public TimeSpan? BancoDeHorasAjuste { get; set; }

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

        public virtual List<PontoDoDiaHoraExtra> ListaDeHoraExtra { get; set; } = new();

        public bool PossuiRegistroNaEntrada(int periodo)
        {
            switch (periodo)
            {
                case 1:
                    return Entrada1.HasValue;
                case 2:
                    return Entrada2.HasValue;
                case 3:
                    return Entrada3.HasValue;
                case 4:
                    return Entrada4.HasValue;
                case 5:
                    return Entrada5.HasValue;
                default:
                    return false;
            }
        }

        public bool PossuiRegistroNaSaida(int periodo)
        {
            switch (periodo)
            {
                case 1:
                    return Saida1.HasValue;
                case 2:
                    return Saida2.HasValue;
                case 3:
                    return Saida3.HasValue;
                case 4:
                    return Saida4.HasValue;
                case 5:
                    return Saida5.HasValue;
                default:
                    return false;
            }
        }

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
                        $"{Entrada1.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoEntrada1)}" :
                        JustificativaPeriodo1Id.HasValue ?
                            JustificativaPeriodo1.Sigla :
                    string.Empty;

                    //retorno += TipoEntrada1.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 2:
                    retorno = Entrada2.HasValue ?
                        $"{Entrada2.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoEntrada2)}" :
                        JustificativaPeriodo2Id.HasValue ?
                            JustificativaPeriodo2.Sigla :
                    string.Empty;

                    //retorno += TipoEntrada2.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 3:
                    retorno = Entrada3.HasValue ?
                        $"{Entrada3.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoEntrada3)}" :
                        JustificativaPeriodo3Id.HasValue ?
                            JustificativaPeriodo3.Sigla :
                            string.Empty;

                    //retorno += TipoEntrada3.DescricaoTipoDeRegistroDoEnumerador();
                    break;


                case 4:
                    retorno = Entrada4.HasValue ?
                        $"{Entrada4.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoEntrada4)}" :
                        JustificativaPeriodo4Id.HasValue ?
                            JustificativaPeriodo4.Sigla :
                            string.Empty;

                    //retorno += TipoEntrada4.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 5:
                    retorno = Entrada5.HasValue ?
                        $"{Entrada5.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoEntrada5)}" :
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
                        $"{Saida1.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoSaida1)}" :
                        JustificativaPeriodo1Id.HasValue ?
                            JustificativaPeriodo1.Sigla :
                            string.Empty;

                    //retorno += TipoSaida1.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 2:
                    retorno = Saida2.HasValue ?
                        $"{Saida2.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoSaida2)}" :
                        JustificativaPeriodo2Id.HasValue ?
                            JustificativaPeriodo2.Sigla :
                            string.Empty;

                    //retorno += TipoSaida2.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 3:
                    retorno = Saida3.HasValue ?
                        $"{Saida3.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoSaida3)}" :
                        JustificativaPeriodo3Id.HasValue ?
                            JustificativaPeriodo3.Sigla :
                            string.Empty;

                    //retorno += TipoSaida3.DescricaoTipoDeRegistroDoEnumerador();
                    break;
                case 4:
                    retorno = Saida4.HasValue ?
                        $"{Saida4.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoSaida4)}" :
                        JustificativaPeriodo4Id.HasValue ?
                            JustificativaPeriodo4.Sigla :
                            string.Empty;

                    //retorno += TipoSaida4.DescricaoTipoDeRegistroDoEnumerador();
                    break;

                case 5:
                    retorno = Saida5.HasValue ?
                        $"{Saida5.Value.ToString(@"hh\:mm")}{DescricaoTipoDeRegistroDoEnumerador(TipoSaida5)}" :
                        JustificativaPeriodo5Id.HasValue ?
                            JustificativaPeriodo5.Sigla :
                            string.Empty;

                    //retorno += TipoSaida5.DescricaoTipoDeRegistroDoEnumerador();

                    break;

            }

            return retorno;
        }

        public bool RegistroEntradaApp(int periodo)
        {
            switch (periodo)
            {
                case 1:
                    return RegistroDePontoEntrada1?.RegistroAplicativoId.HasValue ?? false;

                case 2:
                    return RegistroDePontoEntrada2?.RegistroAplicativoId.HasValue ?? false;

                case 3:
                    return RegistroDePontoEntrada3?.RegistroAplicativoId.HasValue ?? false;

                case 4:
                    return RegistroDePontoEntrada4?.RegistroAplicativoId.HasValue ?? false;

                case 5:
                    return RegistroDePontoEntrada5?.RegistroAplicativoId.HasValue ?? false;

                default:
                    return false;
            }
        }

        public bool RegistroSaidaApp(int periodo)
        {
            switch (periodo)
            {
                case 1:
                    return RegistroDePontoSaida1?.RegistroAplicativoId.HasValue ?? false;

                case 2:
                    return RegistroDePontoSaida2?.RegistroAplicativoId.HasValue ?? false;

                case 3:
                    return RegistroDePontoSaida3?.RegistroAplicativoId.HasValue ?? false;

                case 4:
                    return RegistroDePontoSaida4?.RegistroAplicativoId.HasValue ?? false;

                case 5:
                    return RegistroDePontoSaida5?.RegistroAplicativoId.HasValue ?? false;

                default:
                    return false;
            }
        }

        public static string DescricaoTipoDeRegistroDoEnumerador(Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type type = value.GetType();

            string name = Enum.GetName(type, value);

            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescricaoTipoRegistroDePontoAttribute attribute = field.GetCustomAttribute<DescricaoTipoRegistroDePontoAttribute>();
                    if (attribute != null)
                    {
                        return attribute.Descricao;
                    }
                }
            }

            return string.Empty;
        }
    }
}