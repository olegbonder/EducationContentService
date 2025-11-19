using EducationContentService.Core.Database;
using EducationContentService.Domain.Lesson;
using Microsoft.EntityFrameworkCore;

namespace EducationContentService.Infrastructure.Postgres
{
    public class EducationDbContext : DbContext, IEducationReadDbContext
    {
        public EducationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Lesson> Lessons => Set<Lesson>();

        public IQueryable<Lesson> LessonQuery => Lessons.AsNoTracking().AsQueryable();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EducationDbContext).Assembly);
        }
    }
}