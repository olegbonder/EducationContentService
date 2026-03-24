namespace EducationContentService.Contracts
{
    public record GetLessonsRequest(string? Search, bool? IsDeleted, int Page, int PageSize);
}
