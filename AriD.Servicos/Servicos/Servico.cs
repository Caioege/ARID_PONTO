using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;
using System.Linq.Expressions;

namespace AriD.Servicos.Servicos
{
    public class Servico<T> : IServico<T> where T : EntidadeBase
    {
        private readonly IRepositorio<T> _repositorio;

        public Servico(IRepositorio<T> repositorio)
        {
            _repositorio = repositorio;
        }

        public int Adicionar(T entidade)
        {
            _repositorio.Add(entidade);
            _repositorio.Commit();

            return entidade.Id;
        }

        public void Atualizar(T entidade)
        {
            _repositorio.Atualizar(entidade);
            _repositorio.Commit();
        }

        public T Obtenha(int id)
        {
            return _repositorio.Obtenha(id);
        }

        public T Obtenha(Expression<Func<T, bool>> predicate)
        {
            return _repositorio.Obtenha(predicate);
        }

        public List<T> ObtenhaLista()
        {
            return _repositorio.ObtenhaLista();
        }
           
        public List<T> ObtenhaLista(Expression<Func<T, bool>> predicate)
        {
            return _repositorio.ObtenhaLista(predicate);
        }

        public (int Total, List<T> Itens) ObtenhaListaPaginada(Expression<Func<T, bool>> predicate, int pagina, int limite)
        {
            return
            (
                _repositorio.TotalDeItens(predicate),
                _repositorio.ObtenhaListaPaginada(predicate, pagina, limite)
            );
        }

        public void Remover(T entidade)
        {
            _repositorio.Remover(entidade);
            _repositorio.Commit();
        }
    }
}
