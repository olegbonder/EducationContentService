using FileService.Core;
using FileService.Core.Database;
using FileService.Infrastructure.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
            services.AddScoped<IVideoProcessingRepository, VideoProcessingRepository>();
            services.AddScoped<ITransactionManager, TransactionManager>();
            services.AddScoped<IOutboxService, OutboxService>();
            services.AddScoped<IDbContextOutbox<FileServiceDbContext>, DbContextOutbox<FileServiceDbContext>>();
            
            services.AddDbContextPool<FileServiceDbContext>((sp, options) =>
            {
                string? connectionString = configuration.GetConnectionString(ConnectionStringNames.DATABASE);
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

            services.AddDbContextPool<IReadDbContext, FileServiceDbContext>((sp, options) => 
            {
                string? connectionString = configuration.GetConnectionString(ConnectionStringNames.DATABASE);
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

            return services;
        }
    }
}
