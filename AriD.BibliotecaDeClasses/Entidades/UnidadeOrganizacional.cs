using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class UnidadeOrganizacional : EntidadeOrganizacaoBase
    {
        public string Nome { get; set; }
        public string CNPJ { get; set; }

        public int EnderecoId { get; set; }

        [ForeignKey(nameof(EnderecoId))]
        public virtual Endereco Endereco { get; set; }

        public eTipoUnidadeOrganizacional? Tipo { get; set; }

        public bool Ativa { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public int? RaioDaCercaVirtualEmMetros { get; set; }
    }
}