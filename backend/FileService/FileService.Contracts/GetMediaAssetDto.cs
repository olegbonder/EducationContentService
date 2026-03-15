namespace FileService.Contracts
{
    public record GetMediaAssetDto(
        Guid id, 
        string Status, 
        string AssetType,
        string? Url,
        long? Size,
        string? FileName,
        string? ContentType);    
}