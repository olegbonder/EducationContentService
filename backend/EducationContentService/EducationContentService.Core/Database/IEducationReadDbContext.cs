using EducationContentService.Domain.Lessons;

namespace EducationContentService.Core.Database
{
    public interface IEducationReadDbContext
    {
        IQueryable<Lesson> LessonQuery { get; }
    }
}
