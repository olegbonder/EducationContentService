using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;
using Shared.SharedKernel.Exceptions;
using System.Security.Authentication;

namespace Framework.Middlewares
{
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Exception was thrown in education service");

            (int code, Error error) = exception switch
            {
                NotFoundException ex => (StatusCodes.Status404NotFound, ex.Error),
                VaildationException ex => (StatusCodes.Status400BadRequest, ex.Error),
                ConflictException ex => (StatusCodes.Status409Conflict, ex.Error),
                FailureException ex => (StatusCodes.Status500InternalServerError, ex.Error),
                AuthenticationException ex => (StatusCodes.Status401Unauthorized, Error.Authentification("authentification.failed", exception.Message)),               
                _ => (StatusCodes.Status500InternalServerError, Error.Failure("server.internal", exception.Message))
            };

            context.Response.StatusCode = code;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(error);
        }
    }
}
