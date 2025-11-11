using EducationContentService.Domain.Lesson;
using EducationContentService.Domain.ModuleItems;
using EducationContentService.Domain.Modules;
using Microsoft.EntityFrameworkCore;

namespace EducationContentService.Infrastructure.Postgres
{
    public class EducationDbContext : DbContext
    {
        public EducationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<ModuleItem> ModuleItems => Set<ModuleItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EducationDbContext).Assembly);
        }
    }
}