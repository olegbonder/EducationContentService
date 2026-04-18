using FileService.Core.Features;
using FileService.Core.Messaging;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FileService.Core
{
    public static class DependencyInjectionCoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjectionCoreExtensions).Assembly;
            services.AddValidatorsFromAssembly(assembly);

            services.AddScoped<IAssetCreatedEventPublisher, AssetCreatedEventPublisher>();
            services.AddScoped<StartMultiPartUploadHandler>();
            services.AddScoped<CompleteMultiPartUploadHandler>();

            return services;
        }

        public static IServiceCollection AddQuartzServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddQuartz(options => 
            {
               options.UsePersistentStore(persistenceOptions =>
               {
                   persistenceOptions.UsePostgres(cfg =>
                   {
                       cfg.ConnectionString = configuration.GetConnectionString("FileServiceDb");
                   });
                   
                   persistenceOptions.UseNewtonsoftJsonSerializer();
                   persistenceOptions.UseProperties = true;
               });
            });

            return services;
        }
    }
}
