namespace FileService.Contracts
{
    public record CompleteMultiPartUploadRequest(Guid MediaAssetId, string UploadId, IReadOnlyList<PartEtagDto> PartETags);
}
