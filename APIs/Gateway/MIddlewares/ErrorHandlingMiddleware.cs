using System.Net;
using System.Text.Json;
using FluentValidation;

namespace APIs.Gateway.MIddlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode;
            object response;

            if (ex is ValidationException validationEx)
            {
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    statusCode = (int)statusCode,
                    errors = validationEx.Errors.Select(e => e.ErrorMessage).ToArray(),
                    path = context.Request.Path.Value,
                    timestamp = DateTime.UtcNow
                };
            }
            else
            {
                (statusCode, string message) = ex switch
                {
                    HttpRequestException httpEx when httpEx.StatusCode.HasValue => (httpEx.StatusCode.Value, "Falha na comunicação com outro serviço."),
                    HttpRequestException _ => (HttpStatusCode.BadGateway, "Falha na comunicação com outro serviço."),
                    TaskCanceledException _ => (HttpStatusCode.RequestTimeout, "Tempo de resposta excedido."),
                    _ => (HttpStatusCode.InternalServerError, "Erro inesperado no servidor.")
                };

                response = new
                {
                    statusCode = (int)statusCode,
                    error = message,
                    path = context.Request.Path.Value,
                    timestamp = DateTime.UtcNow
                };
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}