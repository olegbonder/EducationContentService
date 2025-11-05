using EducationContentService.Core;
using EducationContentService.Core.Features.Lessons;
using EducationContentService.Web.EndPointSettings;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

namespace EducationContentService.Web.Configuration
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<CreateHanlder>();
            services
                .AddSerilogLogging(configuration)
                .AddOpenApiSpec()                
                .AddEndpoints(typeof(IEndpoint).Assembly);



            return services;
        }

        private static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
        {
            services.AddOpenApi();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Eduacation Content Service",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "Oleg",
                        Email = "olegbonder@gmail.com",
                    },
                });
            });

            return services;
        }

        private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSerilog((serviceProvider, lo) => lo
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(serviceProvider)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("ServiceName", "LessonService"));
            return services;
        }
    }
}
