using CSharpFunctionalExtensions;
using EducationContentService.Domain.Lesson;
using EducationContentService.Domain.Shared;

namespace EducationContentService.Core.Features.Lessons
{
    public interface ILessonsRepository
    {
        public Task<Result<Guid, Error>> AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
    }
}
