namespace FileService.Contracts.Dtos.Videos
{
    public sealed record InitiateVideoUploadRequest(
        string FileName,
        string ContentType,
        long Size,
        Guid EntityId,
        string EntityType);
}