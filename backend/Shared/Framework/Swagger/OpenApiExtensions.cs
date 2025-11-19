using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Swagger
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddOpenApiSpec(this IServiceCollection services, string title, string version)
        {
            services.AddOpenApi();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = title,
                    Version = version,
                    Contact = new OpenApiContact
                    {
                        Name = "Oleg",
                        Email = "olegbonder@gmail.com",
                    },
                });
            });

            return services;
        }
    }
}
