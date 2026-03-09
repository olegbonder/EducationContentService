using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.Assets;

public class VideoAsset: MediaAsset
{
    private VideoAsset()
    {
    }
    
    private VideoAsset(
        Guid id,
        MediaData data,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key)
        : base(id, data, status, AssetType.VIDEO, owner, key)
    {
    }

    private const long MAX_SIZE = 5_368_709_120;
    public const string LOCATION = "videos";
    public const string RAW_PREFIX = "raw";
    private const string ALLOWED_CONTENT_TYPE = "video";

    private static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];

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

    public static Result<VideoAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
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
        return new VideoAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            owner,
            keyResult.Value);
    }
}