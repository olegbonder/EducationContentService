using EducationContentService.Web.EndPointSettings;
using EducationContentService.Web.Middlewares;
using Serilog;

namespace EducationContentService.Web.Configuration
{
    public static class AppExtensions
    {
        public static IApplicationBuilder Configure(this WebApplication app)
        {
            app.UseRequestCorrelationId();
            app.UseSerilogRequestLogging();

            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Education Content Service V1");
            });

            var apiGroup = app.MapGroup("/api/lessons").WithOpenApi();
            app.UseEndPoints(apiGroup);

            return app;
        }
    }
}
