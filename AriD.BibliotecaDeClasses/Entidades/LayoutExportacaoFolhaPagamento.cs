using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class LayoutExportacaoFolhaPagamento : EntidadeOrganizacaoBase
    {
        [Required, MaxLength(80)]
        public string Nome { get; set; } = "";

        [Required, MaxLength(5)]
        public string Delimitador { get; set; } = ";";

        public bool UsarCabecalho { get; set; } = true;

        public eFormatoQuantidadeExportacao FormatoQuantidade { get; set; } = eFormatoQuantidadeExportacao.HHMM;

        public int CasasDecimais { get; set; } = 2;

        public bool UsarBOM { get; set; } = true;

        public bool Ativo { get; set; } = true;

        public virtual List<LayoutExportacaoFolhaPagamentoCampo> Campos { get; set; } = new();
    }
}