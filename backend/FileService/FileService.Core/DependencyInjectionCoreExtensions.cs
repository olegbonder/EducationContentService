using FileService.Core.Features;
using FileService.Core.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
