using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ConfiguracaoBonus : EntidadeOrganizacaoBase
    {
        [Required]
        [MaxLength(100)]
        public string Descricao { get; set; } // Ex: Vale Alimentação R$ 30, Vale Transporte R$ 10

        [Required]
        public decimal ValorDiario { get; set; }

        public bool PagaEmFinaisDeSemanaEFeriados { get; set; }
        
        public bool TurnoIntercaladoPagaDobrado { get; set; } // Regra customizada do turno intercalado

        public bool Ativo { get; set; } = true;
    }
}
