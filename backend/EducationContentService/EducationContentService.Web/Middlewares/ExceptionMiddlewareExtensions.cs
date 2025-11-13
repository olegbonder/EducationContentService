namespace EducationContentService.Web.Middlewares
{
    internal static class ExceptionMiddlewareExtensions
    {
        public static void UseExceptionMiddleware(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
