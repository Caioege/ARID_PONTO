using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace AriD.GerenciamentoDePonto.Helpers
{
    public static class ChecBoxHtmlHelper
    {
        private static IEnumerable<SelectListItem> Items(bool value) =>
        [
            new SelectListItem("Sim", "true", value),
            new SelectListItem("Não", "false", !value)
        ];

        public static IHtmlContent CheckboxDropDownList<TEnum>(
            this IHtmlHelper htmlHelper,
            string name,
            bool selectedValue = false,
            object htmlAttributes = null)
        {
            var selectList = Items(selectedValue);
            return htmlHelper.DropDownList(name, new SelectList(selectList, "Value", "Text"), htmlAttributes);
        }

        public static IHtmlContent CheckboxDropDownListFor<TModel, Boolean>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, Boolean>> expression,
            object htmlAttributes = null)
        {
            var expressionProvider = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;

            var modelExpression = expressionProvider.CreateModelExpression(htmlHelper.ViewData, expression);

            var metadata = modelExpression.Metadata;
            var name = modelExpression.Name;

            var value = (bool)modelExpression.Model;
            var selectList = Items(value);

            return htmlHelper.DropDownList(name, new SelectList(selectList, "Value", "Text"), htmlAttributes);
        }
    }
}