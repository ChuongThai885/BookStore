using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.API.Infrastructure
{
    public sealed class GlobalExceptionHandler: IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, exposeMessage) = exception switch
            {
                UnauthorizedAccessException or SecurityTokenException =>
                    (StatusCodes.Status401Unauthorized, "Unauthorized", true),

                ArgumentException =>
                    (StatusCodes.Status400BadRequest, "Bad Request", true),

                InvalidOperationException =>
                    (StatusCodes.Status400BadRequest, "Invalid Operation", true),

                KeyNotFoundException =>
                    (StatusCodes.Status404NotFound, "Not Found", true),

                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", false)
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
            }
            else
            {
                _logger.LogWarning("Handled domain exception ({StatusCode}): {Message}", statusCode, exception.Message);
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exposeMessage ? exception.Message : "An unexpected error occurred on the server.",
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
}
}
