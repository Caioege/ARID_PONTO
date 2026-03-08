using System.ComponentModel;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    public enum eCampoExportacaoFolhaPagamento
    {
        [Description("Matrícula")]
        Matricula = 1,

        [Description("Nome do Servidor")]
        NomeServidor = 2,

        [Description("CPF")]
        CPF = 3,

        [Description("Competência (MM/yyyy)")]
        Competencia = 4,

        [Description("Código do Evento")]
        CodigoEvento = 5,

        [Description("Quantidade")]
        Quantidade = 6,

        [Description("Unidade")]
        Unidade = 7,

        [Description("Departamento")]
        Departamento = 8,

        [Description("Função")]
        Funcao = 9,

        [Description("Tipo do Evento")]
        TipoEvento = 10,

        [Description("Percentual")]
        Percentual = 11,

        [Description("Data do Evento (dd/MM/yyyy)")]
        DataEvento = 12
    }
}