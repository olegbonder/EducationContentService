using EducationContentService.Core;
using EducationContentService.Infrastructure.Postgres;
using EducationContentService.Web.EndPointSettings;
using Framework;
using Framework.Logging;
using Framework.Swagger;

namespace EducationContentService.Web.Configuration
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddCore(configuration)
                .AddInfrastructurePostgres(configuration)
                .AddSerilogLogging(configuration, "LessonService")
                .AddOpenApiSpec("Eduacation Content Service", "v1")                
                .AddEndpoints(typeof(IEndpoint).Assembly);
        }
    }
}
