using Microsoft.AspNetCore.Html;

namespace AriD.GerenciamentoDePonto.WebGrid
{
    public class WebGrid<T>
    {
        public ListaPaginada<T> Model { get; set; }
        public string AjaxUpdateContainerId { get; set; }
        public string AjaxUpdateCallback { get; set; }

        public WebGrid(
            ListaPaginada<T> pagedList,
            string ajaxUpdateContainerId,
            string ajaxUpdateCallback)
        {
            Model = pagedList;
            AjaxUpdateContainerId = ajaxUpdateContainerId;
            AjaxUpdateCallback = ajaxUpdateCallback;
        }

        public WebGridColumn Column(
            string columnName = null,
            string header = null,
            Func<dynamic, object> format = null,
            string style = null,
            bool canSort = false,
            float? width = null)
                => new WebGridColumn(columnName, header, format, style, canSort, width);

        public IEnumerable<WebGridColumn> Columns(params WebGridColumn[] columns) => new List<WebGridColumn>(columns);

        public async Task<IHtmlContent> Table(IEnumerable<WebGridColumn> columns, string tableStyle, Task<IHtmlContent> footer)
        {
            var builder = new HtmlContentBuilder();

            builder.AppendHtmlLine($"<table class=\"table table-bordered webgrid-table {tableStyle}\">");
            builder.AppendHtmlLine("<thead style=\"background-color: whitesmoke;\">");
            builder.AppendHtmlLine("<tr>");
            foreach (var column in columns)
            {
                var classCanSort = column.CanSort ? "columnSort" : string.Empty;
                builder.AppendHtmlLine($"<th {(column.Width.HasValue ? $"width=\"{column.Width}%\"" : string.Empty)} scope=\"col\" class=\"column-header-webgrid {column.Style} {classCanSort}\">");
                if (column.CanSort)
                {
                    builder.AppendHtmlLine($"<a href=\"javascript:\" onclick=$('#{Model.GridId}').load('{GetURLSort(column.ColumnName)}',function(){{ {Model.ExecutarScripts} }})>");
                    builder.AppendHtmlLine(column.Header ?? column.ColumnName);
                    builder.AppendHtmlLine("</a>");
                }
                else
                {
                    builder.AppendHtmlLine(column.Header ?? column.ColumnName);
                }

                builder.AppendHtmlLine("</th>");
            }
            builder.AppendHtmlLine("</tr>");
            builder.AppendHtmlLine("</thead>");

            builder.AppendHtmlLine("<tbody style=\"background-color: white;\">");
            if (Model.Itens != null && Model.Itens.Any())
            {
                foreach (var item in Model.Itens)
                {
                    builder.AppendHtmlLine("<tr>");
                    foreach (var column in columns)
                    {
                        var label = (column.Header ?? column.ColumnName ?? string.Empty).Replace("\"", "&quot;");
                        builder.AppendHtmlLine($"<td data-label=\"{label}\">");
                        if (column.Format != null)
                            builder.AppendLine(Format(column.Format, item));
                        else
                            builder.AppendHtmlLine(
                                GetPropValueWithSubClass(item, column.ColumnName)?.ToString());

                        builder.AppendHtmlLine("</td>");
                    }
                    builder.AppendHtmlLine("</tr>");
                }
            }
            else
            {
                builder.AppendHtmlLine("<tr>");
                builder.AppendHtmlLine($"<td colspan=\"{columns.Count()}\">< Nenhum registro encontrado. ></td>");
                builder.AppendHtmlLine("</tr>");
            }
            builder.AppendHtmlLine("</tbody>");
            builder.AppendHtmlLine("</table>");

            if (footer != null)
                builder.AppendLine(await footer);

            return builder;
        }

        private IHtmlContent Format(Func<dynamic, object> format, dynamic arg)
        {
            dynamic result = ((Func<object, object>)format)(arg);
            if (result is IHtmlContent)
                return result;
            else
            {
                return new HtmlContentBuilder().Append(result.ToString());
            }
        }

        private string GetURLSort(string columnName)
        {
            string url = $"/{Model.Controller}/{(Model.Action ?? "IndexGrid")}?page=1&sort={columnName}";

            if (string.IsNullOrEmpty(Model.DirecaoDaOrdenacao) || Model.DirecaoDaOrdenacao.ToUpper() == "DESC")
                url += $"&direcaoDaOrdenacao=ASC";
            else
                url += $"&direcaoDaOrdenacao=DESC";

            if (Model.TermoDeBusca != null)
                url += $"&termoDeBusca={Model.TermoDeBusca}";

            if (Model.QuantidadeDeItensPorPagina != null)
                url += $"&quantidadeDeItensPorPagina={Model.QuantidadeDeItensPorPagina}";

            if (Model.Adicional != null)
                url += $"&adicional={Model.Adicional}";

            return Uri.EscapeDataString(url);
        }

        public static object GetPropValueWithSubClass(object src, string propNameWithClassPath)
        {
            if (string.IsNullOrEmpty(propNameWithClassPath))
                return null;

            var paths = propNameWithClassPath.Split('.');
            object currentObject = null;

            foreach (var propPath in paths)
                currentObject = paths.First() == propPath ?
                        src?.GetType().GetProperty(propPath)?.GetValue(src, null) :
                        currentObject?.GetType().GetProperty(propPath)?.GetValue(currentObject, null);

            return currentObject;
        }
    }

    public class WebGridColumn
    {
        public string ColumnName { get; set; }
        public string Header { get; set; }
        public Func<dynamic, object> Format { get; set; }
        public string Style { get; set; }
        public bool CanSort { get; set; }
        public float? Width { get; set; }

        public WebGridColumn(
            string columnName = null,
            string header = null,
            Func<dynamic, object> format = null,
            string style = null,
            bool canSort = true,
            float? width = null)
        {
            ColumnName = columnName;
            Header = header;
            Format = format;
            Style = style;
            CanSort = canSort;
            Width = width;
        }
    }
}