using AriD.BibliotecaDeClasses.Entidades.Base;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class ConfiguracaoBonusFuncao : EntidadeIdentityBase
    {
        public int ConfiguracaoBonusId { get; set; }
        public virtual ConfiguracaoBonus ConfiguracaoBonus { get; set; }

        public int FuncaoId { get; set; }
        public virtual Funcao Funcao { get; set; }
    }
}
