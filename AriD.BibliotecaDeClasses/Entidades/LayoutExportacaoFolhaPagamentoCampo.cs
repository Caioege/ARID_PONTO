using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class LayoutExportacaoFolhaPagamentoCampo : EntidadeOrganizacaoBase
    {
        [Required]
        public int LayoutId { get; set; }

        [ForeignKey(nameof(LayoutId))]
        public virtual LayoutExportacaoFolhaPagamento Layout { get; set; }

        [Required]
        public int Ordem { get; set; }

        [Required]
        public eCampoExportacaoFolhaPagamento Campo { get; set; }

        [MaxLength(60)]
        public string? NomeColuna { get; set; }

        [MaxLength(200)]
        public string? ValorFixo { get; set; }

        public bool Ativo { get; set; } = true;
    }
}