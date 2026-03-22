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
        public string Descricao { get; set; } // Ex: Vale Alimentação R$ 30, Vale Transporte R$ 10

        [Required(ErrorMessage = "O valor diário/total é obrigatório.")]
        [Range(0.01, 99999.99, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal ValorDiario { get; set; } // Representa Valor Diário ou Valor Mensal (Total) dependendo do TipoBonus

        [Required]
        public eTipoBonus TipoBonus { get; set; } = eTipoBonus.Diario;

        public bool PerdeIntegralmenteComFalta { get; set; } = true;

        public bool PagaEmFinaisDeSemanaEFeriados { get; set; }
        
        public bool TurnoIntercaladoPagaDobrado { get; set; } // Regra customizada do turno intercalado
        
        public int MinutosIntervaloTurnoIntercalado { get; set; } = 120; // Padrão 120 (2h)

        public bool Ativo { get; set; } = true;

        public virtual ICollection<ConfiguracaoBonusFuncao> Funcoes { get; set; } = new List<ConfiguracaoBonusFuncao>();
    }
}
