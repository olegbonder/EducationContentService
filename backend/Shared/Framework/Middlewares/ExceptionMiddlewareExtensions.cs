using Microsoft.AspNetCore.Builder;

namespace Framework.Middlewares
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void UseExceptionMiddleware(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
