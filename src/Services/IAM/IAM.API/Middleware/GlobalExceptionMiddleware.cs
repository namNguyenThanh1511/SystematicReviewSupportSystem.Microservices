
using IAM.Services.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Shared.Builder;
using System.Net;
using System.Text.Json;

namespace IAM.API.Middleware
{
    public class GlobalExceptionMiddleware : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            var (statusCode, response) = exception switch
            {
                BaseDomainException domainEx => (
                    (int)domainEx.StatusCode,
                    domainEx.StatusCode switch
                    {
                        HttpStatusCode.BadRequest => ResponseBuilder.BadRequest(domainEx.Message),
                        HttpStatusCode.Unauthorized => ResponseBuilder.Unauthorized(domainEx.Message),
                        HttpStatusCode.Forbidden => ResponseBuilder.Forbidden(domainEx.Message),
                        HttpStatusCode.NotFound => ResponseBuilder.NotFound(domainEx.Message),
                        _ => ResponseBuilder.Error(domainEx.Message, domainEx.StatusCode)
                    }
                ),
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    ResponseBuilder.InternalServerError("Đã xảy ra lỗi không mong đợi.")
                )
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await httpContext.Response.WriteAsync(json, cancellationToken);
            return true;
        }
    }
}