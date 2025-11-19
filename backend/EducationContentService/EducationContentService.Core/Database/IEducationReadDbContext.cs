using EducationContentService.Domain.Lesson;

namespace EducationContentService.Core.Database
{
    public interface IEducationReadDbContext
    {
        IQueryable<Lesson> LessonQuery { get; }
    }
}
