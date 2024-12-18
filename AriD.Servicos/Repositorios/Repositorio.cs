using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.Servicos.DBContext;
using AriD.Servicos.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AriD.Servicos.Repositorios
{
    public class Repositorio<T> : IRepositorio<T> where T : EntidadeBase
    {
        private readonly MySQLDBContext _contexto;
        private readonly DbSet<T> _dbSet;

        public Repositorio(MySQLDBContext contexto)
        {
            _contexto = contexto;
            _dbSet = contexto.Set<T>();
        }

        public void Add(T entidade)
        {
            if (entidade != null)
                _dbSet.Add(entidade);
        }

        public void Atualizar(T entidade)
        {
            if (entidade != null)
                _dbSet.Update(entidade);
        }
        public void Remover(T entidade)
        {
            if (entidade != null)
                _dbSet.Remove(entidade);
        }

        public T Obtenha(int id)
        {
            return id == 0 ? default : _dbSet.Find(id);
        }

        public T Obtenha(Expression<Func<T, bool>> predicate)
        {
            return _dbSet
                .Where(predicate)
                .FirstOrDefault();
        }

        public List<T> ObtenhaLista()
        {
            return _dbSet.ToList();
        }

        public List<T> ObtenhaLista(Expression<Func<T, bool>> predicate)
        {
            return _dbSet
                .Where(predicate)
                .ToList();
        }

        public List<T> ObtenhaListaPaginada(Expression<Func<T, bool>> predicate, int pagina, int limite)
        {
            return _dbSet
                .Where(predicate)
                .Skip((pagina - 1) * limite)
                .Take(limite)
                .ToList();
        }

        public int TotalDeItens(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Count(predicate);
        }

        public void Commit()
        {
            _contexto.SaveChanges();
        }
    }
}