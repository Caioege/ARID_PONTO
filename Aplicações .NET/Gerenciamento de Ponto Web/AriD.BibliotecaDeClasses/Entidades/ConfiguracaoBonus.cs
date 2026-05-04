using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ConfiguracaoBonus : EntidadeOrganizacaoBase
    {
        [Required]
        [MaxLength(100)]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O valor diário/total é obrigatório.")]
        [Range(0.01, 99999.99, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal ValorDiario { get; set; } 

        [Required]
        public eTipoBonus TipoBonus { get; set; } = eTipoBonus.Diario;

        public bool PerdeIntegralmenteComFalta { get; set; }
        
        public bool ApenasDiasComCargaHoraria { get; set; }

        public int? MinutosFaltaDesconto { get; set; }

        public int? MinutosFaltaDescontoMensal { get; set; }
        
        public bool TurnoIntercaladoPagaDobrado { get; set; }
        
        public int MinutosIntervaloTurnoIntercalado { get; set; } = 120;

        public bool Ativo { get; set; } = true;

        public virtual ICollection<ConfiguracaoBonusFuncao> Funcoes { get; set; } = new List<ConfiguracaoBonusFuncao>();
    }
}
