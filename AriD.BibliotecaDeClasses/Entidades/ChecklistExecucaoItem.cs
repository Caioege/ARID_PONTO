using AriD.BibliotecaDeClasses.Entidades.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ChecklistExecucaoItem : EntidadeBase
    {
        public int ChecklistExecucaoId { get; set; }
        
        [ForeignKey(nameof(ChecklistExecucaoId))]
        public virtual ChecklistExecucao ChecklistExecucao { get; set; }

        public int ChecklistItemId { get; set; }
        
        [ForeignKey(nameof(ChecklistItemId))]
        public virtual ChecklistItem ChecklistItem { get; set; }

        public bool Marcado { get; set; }
    }
}
