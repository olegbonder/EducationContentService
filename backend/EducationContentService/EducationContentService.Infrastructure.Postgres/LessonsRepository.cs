using CSharpFunctionalExtensions;
using EducationContentService.Core.Features.Lessons;
using EducationContentService.Domain.Lesson;
using EducationContentService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.SharedKernel;
using System.Linq.Expressions;
using Index = EducationContentService.Infrastructure.Postgres.Configurations.Index;

namespace EducationContentService.Infrastructure.Postgres
{
    public class LessonsRepository : ILessonsRepository
    {
        private readonly EducationDbContext _dbContext;
        private readonly ILogger<LessonsRepository> _logger;

        public LessonsRepository(EducationDbContext dbContext, ILogger<LessonsRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<Guid, Error>> AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
        {
            _dbContext.Add(lesson);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                return lesson.Id;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null }
                    && pgEx.ConstraintName.Contains(Index.TITLE, StringComparison.InvariantCultureIgnoreCase))
                {
                    return EducationErrors.TitleConflict(lesson.Title.Value);
                }

                _logger.LogError(ex, "Database update error while creating lesson with title {title}", lesson.Title.Value);

                return EducationErrors.DataBaseError();
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogError(ex, "Operation was cancelled while creating lesson with title {Title}", lesson.Title.Value);

                return EducationErrors.OperationCancelled();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating lesson with title {Title}", lesson.Title.Value);

                return EducationErrors.DataBaseError();
            }
        }

        public async Task<Result<Lesson, Error>> GetBy(Expression<Func<Lesson, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(predicate, cancellationToken);
            if (lesson is null)
            {
                return GeneralErrors.NotFound();
            }

            return lesson;
        }
    }
}