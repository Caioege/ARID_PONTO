using AriD.BibliotecaDeClasses.Entidades.Base;
using MySqlConnector;
using System.Linq.Expressions;

namespace AriD.Servicos.Repositorios.Interfaces
{
    public interface IRepositorio<T> where T : EntidadeBase
    {
        T Obtenha(int id);
        T Obtenha(Expression<Func<T, bool>> predicate);

        List<T> ObtenhaLista();
        List<T> ObtenhaLista(Expression<Func<T, bool>> predicate);
        List<T> ObtenhaListaPaginada(Expression<Func<T, bool>> predicate, int pagina, int limite);
        int TotalDeItens(Expression<Func<T, bool>> predicate);

        void Add(T entidade);
        void Atualizar(T entidade);
        void Remover(T entidade);

        void Commit();

        MySqlConnection MySQLConn();
        List<T> ConsultaDapper<T>(string query, object parametros);
    }
}
