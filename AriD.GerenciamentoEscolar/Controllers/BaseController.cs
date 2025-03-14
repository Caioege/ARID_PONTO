using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AriD.GerenciamentoEscolar.Controllers
{
    public class BaseController : Controller
    {
        public async Task<string> RenderizarComoString(string viewName, object model)
        {
            // Usa o ControllerContext e outros serviços do controller
            var viewEngine = HttpContext.RequestServices.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = HttpContext.RequestServices.GetRequiredService<ITempDataProvider>();

            using var stringWriter = new StringWriter();

            // Localiza a view
            var viewResult = viewEngine.FindView(ControllerContext, viewName, isMainPage: false);
            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"A view '{viewName}' não foi encontrada.");
            }

            // Configura os dados da view
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), ModelState)
            {
                Model = model
            };

            // Adicione os valores da ViewBag ao ViewData
            foreach (var key in ViewData.Keys)
            {
                viewData[key] = ViewData[key];
            }

            var tempData = new TempDataDictionary(HttpContext, tempDataProvider);

            // Cria o contexto da view
            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                viewData,
                tempData,
                stringWriter,
                new HtmlHelperOptions()
            );

            // Renderiza a view para a string
            await viewResult.View.RenderAsync(viewContext);

            return stringWriter.ToString();

        }
    }
}
