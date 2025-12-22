using CSharpFunctionalExtensions;
using EducationContentService.Domain.Lesson;
using Shared.SharedKernel;
using System.Linq.Expressions;

namespace EducationContentService.Core.Features.Lessons
{
    public interface ILessonsRepository
    {
        Task<Result<Guid, Error>> AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
        Task<Result<Lesson, Error>> GetBy(Expression<Func<Lesson, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
