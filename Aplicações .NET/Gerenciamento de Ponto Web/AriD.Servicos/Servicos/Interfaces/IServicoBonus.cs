using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoBonus : IServico<BonusCalculado>
    {
        void GerarBonusDoMes(int organizacaoId, int configuracaoBonusId, string mesReferencia, List<int> vinculosIds, bool forcarRecalculo = false);
        List<BonusCalculado> ObterOuCalcularBonusFolha(int organizacaoId, int vinculoId, string mesReferencia, bool salvarNoBanco, bool forcarRecalculo = false);
    }
}
