using System.Net;
using System.Text.Json;

namespace NuvPizza.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ocorrido na requisição {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Em produção, nunca vazar stack traces ou detalhes internos
        var mensagem = _env.IsDevelopment()
            ? $"Erro interno: {exception}"
            : "Ocorreu um erro interno no servidor. Tente novamente mais tarde.";

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = mensagem,
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}