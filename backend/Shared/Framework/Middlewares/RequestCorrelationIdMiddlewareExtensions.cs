using Microsoft.AspNetCore.Builder;

namespace Framework.Middlewares
{
    public static class RequestCorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestCorrelationIdMiddleware>();
        }
    }
}
