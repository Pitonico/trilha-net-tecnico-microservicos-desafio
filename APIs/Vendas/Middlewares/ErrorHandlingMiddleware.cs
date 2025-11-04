using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace APIs.Vendas.Middlewares
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

            (statusCode, string message) = ex switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                InvalidOperationException invEx when invEx.InnerException is NpgsqlException
                    => (HttpStatusCode.ServiceUnavailable, "Falha na conexão com o banco de dados. Tente novamente mais tarde."),
                InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
                ArgumentOutOfRangeException => (HttpStatusCode.BadRequest, ex.Message),
                TaskCanceledException => (HttpStatusCode.RequestTimeout, "Tempo de resposta excedido."),
                HttpRequestException httpEx when httpEx.StatusCode.HasValue => (httpEx.StatusCode.Value, "Falha na comunicação com outro serviço."),
                HttpRequestException => (HttpStatusCode.BadGateway, "Falha na comunicação com outro serviço."),
                DbUpdateException dbEx when dbEx.InnerException is NpgsqlException
                    => (HttpStatusCode.ServiceUnavailable, "Falha na conexão com o banco de dados. Tente novamente mais tarde."),
                DbUpdateException => (HttpStatusCode.InternalServerError, "Erro ao atualizar dados no banco."),
                _ => (HttpStatusCode.InternalServerError, "Erro inesperado no servidor.")
            };

            response = new
            {
                statusCode = (int)statusCode,
                error = message,
                path = context.Request.Path.Value,
                timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}