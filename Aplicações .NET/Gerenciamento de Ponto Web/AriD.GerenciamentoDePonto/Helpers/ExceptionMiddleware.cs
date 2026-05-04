using AriD.GerenciamentoDePonto.Helpers;
using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var controllerName = context.GetRouteData()?.Values["controller"]?.ToString()?.ToLower();

        var isAppController = controllerName == "app" || controllerName == "rastreioapp";

        int statusCode;
        string mensagem;

        if (isAppController)
        {
            if (exception is ApplicationException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                mensagem = exception.Message;
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                mensagem = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
            }
        }
        else
        {
            statusCode = (int)HttpStatusCode.OK;
            mensagem = exception is ApplicationException
                ? exception.Message
                : exception.Message.ToLower().Contains("cannot delete or update a parent row") ||
                  (exception.InnerException != null && exception.InnerException.Message.ToLower().Contains("cannot delete or update a parent row"))
                    ? "Esse item não pode ser alterado/removido pois possui vínculo com outro item."
                    : "Ocorreu um erro inesperado. Tente novamente mais tarde.";
        }

        if (!(exception is ApplicationException))
        {
            try
            {
                Logger.Write(exception);
            }
            catch
            {
            }
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            sucesso = false,
            mensagem
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

}
