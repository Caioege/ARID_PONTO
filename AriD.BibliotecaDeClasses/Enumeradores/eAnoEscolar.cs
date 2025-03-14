using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eAnoEscolar
    {
        // Educação Infantil
        [Description("Berçário 1")] Berçario1,
        [Description("Berçário 2")] Berçario2,
        [Description("Maternal 1")] Maternal1,
        [Description("Maternal 2")] Maternal2,
        [Description("Pré I")] Pre1,
        [Description("Pré II")] Pre2,

        // Ensino Fundamental - Anos Iniciais (Fundamental I)
        [Description("1º Ano do Ensino Fundamental")] PrimeiroAno,
        [Description("2º Ano do Ensino Fundamental")] SegundoAno,
        [Description("3º Ano do Ensino Fundamental")] TerceiroAno,
        [Description("4º Ano do Ensino Fundamental")] QuartoAno,
        [Description("5º Ano do Ensino Fundamental")] QuintoAno,

        // Ensino Fundamental - Anos Finais (Fundamental II)
        [Description("6º Ano do Ensino Fundamental")] SextoAno,
        [Description("7º Ano do Ensino Fundamental")] SetimoAno,
        [Description("8º Ano do Ensino Fundamental")] OitavoAno,
        [Description("9º Ano do Ensino Fundamental")] NonoAno,

        // Ensino Médio
        [Description("1º Ano do Ensino Médio")] PrimeiroAnoEM,
        [Description("2º Ano do Ensino Médio")] SegundoAnoEM,
        [Description("3º Ano do Ensino Médio")] TerceiroAnoEM,

        // Educação de Jovens e Adultos (EJA) - Ensino Fundamental
        [Description("EJA - Fundamental Fase 1")] EJA_Fundamental_Fase1,
        [Description("EJA - Fundamental Fase 2")] EJA_Fundamental_Fase2,
        [Description("EJA - Fundamental Fase 3")] EJA_Fundamental_Fase3,
        [Description("EJA - Fundamental Fase 4")] EJA_Fundamental_Fase4,
        [Description("EJA - Fundamental Fase 5")] EJA_Fundamental_Fase5,
        [Description("EJA - Fundamental Fase 6")] EJA_Fundamental_Fase6,

        // Educação de Jovens e Adultos (EJA) - Ensino Médio
        [Description("EJA - Médio Fase 1")] EJA_Medio_Fase1,
        [Description("EJA - Médio Fase 2")] EJA_Medio_Fase2,
        [Description("EJA - Médio Fase 3")] EJA_Medio_Fase3
    }
}