using AriD.BibliotecaDeClasses.Entidades.Base;
using System.Linq.Expressions;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServico<T> where T : EntidadeBase
    {
        T Obtenha(int id);
        T Obtenha(Expression<Func<T, bool>> predicate);

        List<T> ObtenhaLista();
        List<T> ObtenhaLista(Expression<Func<T, bool>> predicate);

        (int Total, List<T> Itens) ObtenhaListaPaginada(Expression<Func<T, bool>> predicate, int pagina, int limite);
        (int Total, List<T> Itens) ObtenhaListaPaginada<TKey>(Expression<Func<T, bool>> predicate, int pagina, int limite, Func<T, TKey> orderSeletor, bool asc);

        int Adicionar(T entidade);
        void Atualizar(T entidade, bool commit = true);
        void Remover(T entidade, bool commit = true);

        void Commit();
    }
}