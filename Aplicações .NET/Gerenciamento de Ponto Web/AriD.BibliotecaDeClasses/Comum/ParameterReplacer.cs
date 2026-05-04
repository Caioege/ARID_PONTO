using System.Linq.Expressions;

namespace AriD.BibliotecaDeClasses.Comum
{
    public class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parametro;

        public ParameterReplacer(ParameterExpression parametro)
        {
            _parametro = parametro;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(_parametro);
        }
    }
}