using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AriD.GerenciamentoDePonto.Helpers
{
    public class RequestAuthenticationFilter : IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RequestAuthenticationFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// OnActionExecuting
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.ActionDescriptor.RouteValues["controller"]?.ToLower();
            var action = context.ActionDescriptor.RouteValues["action"]?.ToLower();

            if (controller == "politicadeprivacidade") return;

            context.HttpContext.Request.Headers.TryGetValue("X-Requested-With", out var headerValue);
            bool ajaxRequest = headerValue == "XMLHttpRequest";

            context.HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent);
            if (controller == "app")
                return;
            
            if (!string.IsNullOrEmpty(userAgent) && userAgent.Equals("AIFaceEVO.API-ARID.TECNOLOGIA"))
            {
                var actionsEquipamento = new List<string>
                {
                     "registro-equipamento",
                     "receberregistro",
                     "monitorarconectividade"
                };

                if (!actionsEquipamento.Contains(action))
                {
                    context.HttpContext.Response.StatusCode = 403;
                    context.Result = new JsonResult(new { message = "Sem permissão" });
                }
            }
            else
            {
                var autenticado = context.HttpContext.EstaAutenticado();
                if (autenticado && controller == "autenticacao" && action == "index")
                    context.Result = new RedirectToRouteResult
                        (
                            new RouteValueDictionary(new { action = "Index", controller = "Home" })
                        );
                else if (!autenticado && controller != "autenticacao")
                {
                    if (ajaxRequest)
                    {
                        context.HttpContext.Response.StatusCode = 401;
                        context.Result = new JsonResult(new { message = "Não autenticado." });
                    }
                    else
                    {
                        context.Result = new RedirectToRouteResult
                        (
                            new RouteValueDictionary(new { action = "Index", controller = "Autenticacao" })
                        );
                    }
                }
            }
        }

        /// <summary>
        /// OnActionExecuted
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}