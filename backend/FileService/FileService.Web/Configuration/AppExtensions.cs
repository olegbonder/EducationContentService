using CrystalQuartz.AspNetCore;
using Framework.EndPointSettings;
using Framework.Middlewares;
using Quartz;
using Serilog;

namespace FileService.Web.Configuration
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
                options.SwaggerEndpoint("/openapi/v1.json", "FileService V1");
            });
            
            app.UseRouting();
            app.UseCrystalQuartz(() => 
            {
                var factory = app.Services.GetRequiredService<ISchedulerFactory>();
                return factory.GetScheduler().GetAwaiter().GetResult();
            });

            var apiGroup = app.MapGroup("/api").WithOpenApi();
            app.UseEndPoints(apiGroup);

            return app;
        }
    }
}
