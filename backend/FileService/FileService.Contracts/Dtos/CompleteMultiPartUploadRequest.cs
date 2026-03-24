namespace FileService.Contracts.Dtos
{
    public record CompleteMultiPartUploadRequest(Guid MediaAssetId, string UploadId, IReadOnlyList<PartEtagDto> PartETags);
}
