namespace IntegrationEvents.Education.Events
{
    public record IssueCreated(Guid IssueId, Guid LessonId, string Title, string Description);
}