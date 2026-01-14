namespace EducationContentService.Contracts
{
    public record GetLessonRequest(string? Search, bool? IsDeleted, int Page, int PageSize);
}
