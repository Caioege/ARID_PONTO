using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.Servicos.DBContext;
using AriD.Servicos.Repositorios.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Linq.Expressions;

namespace AriD.Servicos.Repositorios
{
    public class Repositorio<T> : IRepositorio<T> where T : EntidadeBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MySQLDBContext _contexto;
        private readonly DbSet<T> _dbSet;

        // TODO: Adicionar filtro de organizacao
        private readonly int? OrganizacaoId;

        public Repositorio(MySQLDBContext contexto, 
            IHttpContextAccessor httpContextAccessor)
        {
            _contexto = contexto;
            _dbSet = contexto.Set<T>();
            _httpContextAccessor = httpContextAccessor;
        }

        public void Add(T entidade)
        {
            if (OrganizacaoId.HasValue && typeof(T).IsAssignableFrom(typeof(EntidadeOrganizacaoBase)))
                (entidade as EntidadeOrganizacaoBase).OrganizacaoId = OrganizacaoId.Value;

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

        public List<T> ObtenhaListaPaginada<TKey>(Expression<Func<T, bool>> predicate, int pagina, int limite, Func<T, TKey> orderSeletor = null, bool asc = false)
        {
            if (orderSeletor != null)
            {
                if (asc)
                {
                    return _dbSet
                        .Where(predicate)
                        .OrderBy(orderSeletor)
                        .Skip((pagina - 1) * limite)
                        .Take(limite)
                        .ToList();
                }
                else
                {
                    return _dbSet
                        .Where(predicate)
                        .OrderByDescending(orderSeletor)
                        .Skip((pagina - 1) * limite)
                        .Take(limite)
                        .ToList();
                }
            }

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

        public MySqlConnection MySQLConn()
            => new MySqlConnection(_contexto.Database.GetConnectionString());

        public List<T> ConsultaDapper<T>(string query, object parametros)
        {
            using (var conn = MySQLConn())
            {
                return conn.Query<T>(query, parametros).ToList();
            }
        }

        public void ExecutarComando(string query, object parametros)
        {
            using (var conn = MySQLConn())
            {
                conn.Execute(query, parametros);
            }
        }
    }
}