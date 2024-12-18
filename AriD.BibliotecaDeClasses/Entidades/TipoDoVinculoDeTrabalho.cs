using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class TipoDoVinculoDeTrabalho : EntidadeOrganizacaoBase
    {
        public string Sigla { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        public bool Ativo { get; set; }
    }
}