using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eCampoExportacao
    {
        [Description("Nome do Servidor")]
        NomeServidor = 1,
        [Description("CPF")]
        CPF = 2,
        [Description("PIS")]
        PIS = 3,
        [Description("Matrícula do Vínculo de Trabalho")]
        Matricula = 4,
        [Description("Horas Trabalhadas")]
        HorasTrabalhadas = 5,
        [Description("Carga Horária Mensal")]
        CargaHorariaMensal = 6,
        [Description("Carga Horária Semanal")]
        CargaHorariaSemanal = 7,
        [Description("Período (Mês/Ano)")]
        Periodo = 8,
        [Description("Horas Positivas (Extras)")]
        HorasPositivas = 9,
        [Description("Horas Negativas (Faltas)")]
        HorasNegativas = 10,
        [Description("Saldo Banco de Horas")]
        SaldoBancoHoras = 11
    }
}