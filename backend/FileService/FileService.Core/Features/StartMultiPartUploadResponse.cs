using FileService.Contracts;

namespace FileService.Core.Features;

public record StartMultiPartUploadResponse(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls,
    long ChunkSize);
