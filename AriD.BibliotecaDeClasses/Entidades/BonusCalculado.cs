using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Comum;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class BonusCalculado : EntidadeOrganizacaoBase
    {
        public int VinculoDeTrabalhoId { get; set; }
        [ForeignKey(nameof(VinculoDeTrabalhoId))]
        public virtual VinculoDeTrabalho VinculoDeTrabalho { get; set; }

        public int ConfiguracaoBonusId { get; set; }
        [ForeignKey(nameof(ConfiguracaoBonusId))]
        public virtual ConfiguracaoBonus ConfiguracaoBonus { get; set; }

        public string MesReferencia { get; set; } // Formato MM/yyyy

        public int DiasEfetivosTrabalhados { get; set; }
        public int DiasTurnoIntercalado { get; set; } // Qtd de dias que teve o bônus dobrado

        public decimal ValorTotal { get; set; }
        
        public string DetalhesDoCalculoJson { get; set; } // Armazena info diária de porque foi gerado ou não o bônus em tal dia

        public DateTime DataCalculo { get; set; } = DateTime.Now;
    }
}
