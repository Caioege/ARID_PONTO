using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class LogAuditoriaPonto : EntidadeOrganizacaoBase
    {
        [Required]
        public int VinculoDeTrabalhoId { get; set; }

        [Required]
        public string MesAno { get; set; }

        public int? PontoDoDiaId { get; set; }
        [ForeignKey(nameof(PontoDoDiaId))]
        public virtual PontoDoDia PontoDoDia { get; set; }

        [Required, MaxLength(120)]
        public string UsuarioNome { get; set; }

        [Required]
        public DateTime DataHora { get; set; } = DateTime.Now;

        [Required, MaxLength(60)]
        public string Acao { get; set; }

        [Required]
        public string Descricao { get; set; }
    }
}