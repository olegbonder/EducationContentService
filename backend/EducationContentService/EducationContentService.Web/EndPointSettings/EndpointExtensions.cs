using System.Reflection;
using EducationContentService.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EducationContentService.Web.EndPointSettings
{
    internal static class EndpointExtensions
    {
        public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
        {
            var serviceDescriptors = assembly
                .DefinedTypes
                .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)))
                .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type));
            services.TryAddEnumerable(serviceDescriptors);

            return services;
        }

        public static IApplicationBuilder UseEndPoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
        {
            var enpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

            IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

            foreach (IEndpoint enpoint in enpoints)
            {
                enpoint.MapEndPoint(builder);
            }

            return app;
        }
    }
}
