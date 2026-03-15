namespace FileService.Contracts
{
    public record GetMediaAssetsDto(
        Guid id, 
        string Status, 
        string AssetType,
        string? Url);

    public record GetMediaAssetsResponse(IEnumerable<GetMediaAssetsDto> Items); 
}