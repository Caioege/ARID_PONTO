using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Entidades.Base;
using System.Linq.Expressions;

namespace AriD.BibliotecaDeClasses.Comum
{
    public static class ConcatenadorDeExpressao
    {
        public static Expression<Func<T, bool>> Concatenar<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
            where T : EntidadeBase
        {
            // Obtem os parâmetros da primeira expressão
            var parametro = expr1.Parameters[0];

            // Substitui o parâmetro da segunda expressão pelo da primeira
            var corpoSubstituido = new ParameterReplacer(parametro).Visit(expr2.Body);

            // Combina as expressões usando AndAlso
            var corpoCombinado = Expression.AndAlso(expr1.Body, corpoSubstituido);

            // Retorna a nova expressão
            return Expression.Lambda<Func<T, bool>>(corpoCombinado, parametro);
        }
    }
}
