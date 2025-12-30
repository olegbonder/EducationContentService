namespace EducationContentService.Contracts
{
    public record PaginationLessonResponse(
        IReadOnlyList<LessonDto> Items, 
        int TotalCount, 
        int Page, 
        int PageSize, 
        int TotalPages);
}
