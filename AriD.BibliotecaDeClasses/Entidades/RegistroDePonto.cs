using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class RegistroDePonto : EntidadeOrganizacaoBase
    {
        public int? EquipamentoDePontoId { get; set; }
        [ForeignKey(nameof(EquipamentoDePontoId))]
        public virtual EquipamentoDePonto EquipamentoDePonto { get; set; }

        [MaxLength(50)]
        public string? UsuarioEquipamentoId { get; set; }

        [Required]
        public DateTime DataHoraRegistro { get; set; }

        [Required]
        public DateTime DataHoraRecebimento { get; set; }

        [Required]
        public eTipoDeRegistroEquipamento TipoRegistro { get; set; }

        public int? UsuarioImportacaoId { get; set; }
        [ForeignKey(nameof(UsuarioImportacaoId))]
        public virtual Usuario UsuarioImportacao { get; set; }

        public DateTime? DataImportacao { get; set; }

        public int? RegistroAplicativoId { get; set; }
        [ForeignKey(nameof(RegistroAplicativoId))]
        public virtual RegistroAplicativo RegistroAplicativo { get; set; }

        public bool Desconsiderado { get; set; }
        [MaxLength(255)]
        public string? MotivoDesconsideracao { get; set; }
        public string? UsuarioDesconsideracaoNome { get; set; }
        public DateTime? DataDesconsideracao { get; set; }
    }
}