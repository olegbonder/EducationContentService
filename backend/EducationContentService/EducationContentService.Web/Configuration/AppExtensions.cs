using EducationContentService.Web.EndPointSettings;
using Framework.Middlewares;
using Serilog;

namespace EducationContentService.Web.Configuration
{
    internal static class AppExtensions
    {
        public static IApplicationBuilder Configure(this WebApplication app)
        {
            app.UseCors(builder =>
            {
                builder
                .WithOrigins("http://localhost:3000")
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
            app.UseExceptionMiddleware();
            app.UseRequestCorrelationId();
            app.UseSerilogRequestLogging();

            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Education Content Service V1");
            });

            var apiGroup = app.MapGroup("/api").WithOpenApi();
            app.UseEndPoints(apiGroup);

            return app;
        }
    }
}
