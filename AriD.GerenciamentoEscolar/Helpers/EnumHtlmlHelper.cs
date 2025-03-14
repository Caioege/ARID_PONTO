using AriD.BibliotecaDeClasses.DTO;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AriD.GerenciamentoEscolar.Helpers
{
    public static class EnumHtlmlHelper
    {
        public static SelectList GetEnumSelectList<TEnum>(TEnum? selectedValue = null) where TEnum : struct, Enum
        {
            return new SelectList(
                    Enum.GetValues(typeof(TEnum))
                    .Cast<TEnum>()
                    .Select(e => new CodigoDescricaoDTO(Convert.ToInt32(e), GetEnumDescription(e))),
                "Codigo",
                "Descricao",
                selectedValue.HasValue ? Convert.ToInt32(selectedValue) : default(int?));
        }

        private static string GetEnumDescription<TEnum>(TEnum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                 .FirstOrDefault() as DescriptionAttribute;

            return attribute?.Description ?? enumValue.ToString();
        }

        public static IHtmlContent EnumDropDownList<TEnum>(
            this IHtmlHelper htmlHelper,
            string name,
            TEnum? selectedValue = null,
            object htmlAttributes = null) where TEnum : struct, Enum
        {
            return htmlHelper.EnumDropDownList(name, selectedValue, null, htmlAttributes);
        }

        public static IHtmlContent EnumDropDownList<TEnum>(
            this IHtmlHelper htmlHelper,
            string name,
            TEnum? selectedValue,
            string label,
            object htmlAttributes = null) where TEnum : struct, Enum
        {
            var selectList = GetEnumSelectList<TEnum>(selectedValue);
            return htmlHelper.DropDownList(name, selectList, htmlAttributes);
        }

        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression,
            object htmlAttributes = null) where TEnum : struct, Enum
        {
            return htmlHelper.EnumDropDownListFor(expression, null, htmlAttributes);
        }

        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum?>> expression,
            object htmlAttributes = null) where TEnum : struct, Enum
        {
            return htmlHelper.EnumDropDownListFor(expression, null, htmlAttributes);
        }

        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum?>> expression,
            string label,
            object htmlAttributes = null) where TEnum : struct, Enum
        {
            var expressionProvider = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;

            var modelExpression = expressionProvider.CreateModelExpression(htmlHelper.ViewData, expression);
            var metadata = modelExpression.Metadata;
            var name = modelExpression.Name;

            var value = (TEnum?)modelExpression.Model;

            var selectList = GetEnumSelectList<TEnum>(value);

            return htmlHelper.DropDownList(name, selectList, label, htmlAttributes);
        }

        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression,
            string label,
            object htmlAttributes = null) where TEnum : struct, Enum
        {
            var expressionProvider = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;

            var modelExpression = expressionProvider.CreateModelExpression(htmlHelper.ViewData, expression);
            var metadata = modelExpression.Metadata;
            var name = modelExpression.Name;

            var value = (TEnum)modelExpression.Model;

            var selectList = GetEnumSelectList<TEnum>(value);

            return htmlHelper.DropDownList(name, selectList, label, htmlAttributes);
        }
    }
}