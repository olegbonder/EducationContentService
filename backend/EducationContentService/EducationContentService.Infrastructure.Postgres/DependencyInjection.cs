using EducationContentService.Core.Database;
using EducationContentService.Core.Features.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Infrastructure.Postgres
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ILessonsRepository, LessonsRepository>();

            services.AddDbContextPool<EducationDbContext>((sp, options) =>
            {
                string? connectionString = configuration.GetConnectionString(Constants.DATABASE);
                var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                options.UseNpgsql(connectionString);

                if (hostEnvironment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }

                options.UseLoggerFactory(loggerFactory);
            });

            services.AddDbContextPool<IEducationReadDbContext, EducationDbContext>((sp, options) => 
            {
                string? connectionString = configuration.GetConnectionString(Constants.DATABASE);
                var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                options.UseNpgsql(connectionString);

                if (hostEnvironment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }

                options.UseLoggerFactory(loggerFactory);
            });

            services.AddScoped<ITransactionManager, TransactionManager>();

            return services;
        }
    }
}
