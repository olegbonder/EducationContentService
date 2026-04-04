namespace FileService.Contracts.Dtos;

public record StartMultiPartUploadRequest
(
    string FileName,
    string AssetType,
    string ContentType,
    long Size,
    string OwnerType,
    Guid OwnerId
);