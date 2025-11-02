using EducationContentService.Web.EndPointSettings;
using Serilog;

namespace EducationContentService.Web.Configuration
{
    public static class AppExtensions
    {
        public static IApplicationBuilder Configure(this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            app.UseSwagger();
            app.UseSwaggerUI();

            var apiGroup = app.MapGroup("/api/lessons").WithOpenApi();
            app.UseEndPoints(apiGroup);

            return app;
        }
    }
}
