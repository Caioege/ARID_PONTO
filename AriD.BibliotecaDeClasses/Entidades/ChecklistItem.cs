using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ChecklistItem : EntidadeOrganizacaoBase
    {
        public int VeiculoId { get; set; }
        
        [ForeignKey(nameof(VeiculoId))]
        public virtual Veiculo Veiculo { get; set; }

        public string Descricao { get; set; }
        
        public bool Ativo { get; set; }
    }
}
