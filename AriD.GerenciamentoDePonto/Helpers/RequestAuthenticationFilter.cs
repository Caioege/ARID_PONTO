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

            var autenticado = context.HttpContext.EstaAutenticado();
            if (autenticado && controller == "autenticacao" && action == "index")
                context.Result = new RedirectToRouteResult
                    (
                        new RouteValueDictionary(new { action = "Index", controller = "Home" })
                    );
            else if (!autenticado && controller != "autenticacao")
                context.Result = new RedirectToRouteResult
                    (
                        new RouteValueDictionary(new { action = "Index", controller = "Autenticacao" })
                    );
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