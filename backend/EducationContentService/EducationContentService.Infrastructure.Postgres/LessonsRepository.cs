using CSharpFunctionalExtensions;
using EducationContentService.Core.Features.Lessons;
using EducationContentService.Domain.Lesson;
using EducationContentService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EducationContentService.Infrastructure.Postgres
{
    public class LessonsRepository : ILessonsRepository
    {
        private readonly EducationDbContext _dbContext;

        public LessonsRepository(EducationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid, Error>> AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
        {
            await _dbContext.AddAsync(lesson, cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                return lesson.Id;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException)
            {

                throw;
            }
        }
    }
}
