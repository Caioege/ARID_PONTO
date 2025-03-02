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

        var response = new
        {
            sucesso = false,
            mensagem = exception is ApplicationException
                ? exception.Message
                : exception.Message.ToLower().Contains("cannot delete or update a parent row") || (exception.InnerException != null && exception.InnerException.Message.ToLower().Contains("cannot delete or update a parent row")) ? "Esse item não pode ser alterado/removido pois possui vínculo com outro item." : "Ocorreu um erro inesperado. Tente novamente mais tarde."
        };

        context.Response.StatusCode = (int)HttpStatusCode.OK;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
