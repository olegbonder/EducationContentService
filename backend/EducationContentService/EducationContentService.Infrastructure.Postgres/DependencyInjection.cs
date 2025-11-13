using EducationContentService.Core.Features.Lessons;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EducationContentService.Infrastructure.Postgres
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ILessonsRepository, LessonsRepository>();

            services.AddDbContextPool<EducationDbContext>((sp, options) => 
            { 
            });

            return services;
        }
    }
}
