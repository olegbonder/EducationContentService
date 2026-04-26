namespace EducationContentService.Contracts
{
    public record LessonDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        
        public MediaDto? Video { get; set; } = null!;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public bool IsDeleted { get; init; }
    }

    public record MediaDto
    {
        public Guid Id { get; init; }
        public string Url { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
    }
}
