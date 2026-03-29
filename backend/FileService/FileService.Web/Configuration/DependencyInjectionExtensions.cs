using FileService.Core;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using FileService.VideoProcessing;
using Framework.EndPointSettings;
using Framework.Logging;
using Framework.Swagger;

namespace FileService.Web.Configuration
{
    internal static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSerilogLogging(configuration, "FileService")
                .AddOpenApiSpec("FileService", "v1")                
                .AddEndpoints(typeof(DependencyInjectionCoreExtensions).Assembly)
                .AddS3(configuration)
                .AddVideoProcessing(configuration)
                .AddCors();

            services
                .AddCore()
                .AddInfrastructurePostgres(configuration);
            
            return services;
        }
    }
}
