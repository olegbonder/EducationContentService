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
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(DependencyInjectionCoreExtensions).Assembly;
            services.AddValidatorsFromAssembly(assembly);

            services.AddScoped<IAssetCreatedEventPublisher, AssetCreatedEventPublisher>();
            services.AddScoped<StartMultiPartUploadHandler>();
            services.AddScoped<CompleteMultiPartUploadHandler>();
            services.AddScoped<CheckMediaAssetExistsHandler>();
            services.AddScoped<GetMediaAssetInfoHandler>();
            services.AddScoped<GetMediaAssetsHandler>();

            services.AddQuartzServices(configuration);
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
               
               /*var testJobKey = new JobKey("testJob");
               options.AddJob<TestJob> (opts => opts.WithIdentity(testJobKey));
               options.AddTrigger(opts => opts
                   .ForJob(testJobKey)
                   .WithIdentity("TestJob-trigger")
                    .StartNow()
                   .WithSimpleSchedule(x => x
                       .WithIntervalInSeconds(1)
                       .RepeatForever()));*/
            });
            
            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            return services;
        }
    }
}
