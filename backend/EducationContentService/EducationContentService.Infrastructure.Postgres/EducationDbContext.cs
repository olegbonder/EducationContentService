using Microsoft.EntityFrameworkCore;

namespace EducationContentService.Infrastructure.Postgres
{
    public class EducationDbContext : DbContext
    {
        public EducationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected EducationDbContext()
        {
        }
    }
}