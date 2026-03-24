using FileService.Contracts;
using FileService.Contracts.Dtos;

namespace FileService.Core.Features;

public record StartMultiPartUploadResponse(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls,
    int ChunkSize);
