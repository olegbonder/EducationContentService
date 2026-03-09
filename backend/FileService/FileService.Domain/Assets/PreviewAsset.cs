using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.Assets;

public class PreviewAsset: MediaAsset
{
    private PreviewAsset()
    {
    }
    
    private PreviewAsset(
        Guid id,
        MediaData data,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key)
        : base(id, data, status, AssetType.PREVIEW, owner, key)
    {
    }

    private const long MAX_SIZE = 5_368_709;
    public const string LOCATION = "preview";
    public const string RAW_PREFIX = "raw";
    private const string ALLOWED_CONTENT_TYPE = "image";

    private static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];

    public static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation("video.invalid.extension", $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
        }

        if (mediaData.ContentType.Category == MediaType.VIDEO)
        {
            return Error.Validation("video.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation("video.invalid.size", $"File size must be less than {MAX_SIZE}");
        }

        return UnitResult.Success<Error>();
    }

    public static Result<PreviewAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        var validationResult = Validate(mediaData);
        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }
        var keyResult = StorageKey.Create(LOCATION, null, id.ToString());
        if (keyResult.IsFailure)
        {
            return keyResult.Error;
        }
        return new PreviewAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            owner,
            keyResult.Value);
    }
}