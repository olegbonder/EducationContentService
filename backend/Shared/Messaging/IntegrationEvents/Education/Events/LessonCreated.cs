namespace IntegrationEvents.Education.Events
{
    public record LessonCreated(Guid LessonId, Guid CourseId, string Name);
}