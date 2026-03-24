using CSharpFunctionalExtensions;
using Shared.SharedKernel;
using System.Linq.Expressions;
using EducationContentService.Domain.Lessons;

namespace EducationContentService.Core.Features.Lessons
{
    public interface ILessonsRepository
    {
        Task<Result<Guid, Error>> AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
        Task<Result<Lesson, Error>> GetBy(Expression<Func<Lesson, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
