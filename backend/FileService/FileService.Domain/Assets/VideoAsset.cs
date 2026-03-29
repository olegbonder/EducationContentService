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
        StorageKey rawKey)
        : base(id, data, status, AssetType.VIDEO, rawKey)
    {
    }

    private const long MAX_SIZE = 5_368_709_120;
    public const string LOCATION = "videos";
    public const string RAW_PREFIX = "raw";
    private const string ALLOWED_CONTENT_TYPE = "video";

    private const string HLS_PREFIX = "hls";

    public const string MASTER_PLAYLIST_NAME = "master.m3u8";
    public const string STREAM_PLAYLIST_PATTERN = "%v_stream.m3u8";
    public const string SEGMENT_FILE_PATTERN = "%v_%06d.ts";


    private static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];

    public static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation("video.invalid.extension", $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
        }

        if (mediaData.ContentType.Category != MediaType.VIDEO)
        {
            return Error.Validation("video.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation("video.invalid.size", $"File size must be less than {MAX_SIZE}");
        }

        return UnitResult.Success<Error>();
    }

    public static Result<VideoAsset, Error> CreateForUpload(Guid id, MediaData mediaData)
    {
        var validationResult = Validate(mediaData);
        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        var keyResult = StorageKey.Create(LOCATION, RAW_PREFIX, id.ToString());
        if (keyResult.IsFailure)
        {
            return keyResult.Error;
        }
        return new VideoAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            keyResult.Value);
    }

    public Result<StorageKey, Error> GetHlsRootKey()
    {
        return StorageKey.Create(LOCATION, HLS_PREFIX, Id.ToString());
    }

    public Result<StorageKey, Error> GetHlsMasterPlaylistKey()    
    {
        var hlsRootKey = GetHlsRootKey();
        if (hlsRootKey.IsFailure)
            return hlsRootKey.Error;
            

        return hlsRootKey.Value.AppendKey(MASTER_PLAYLIST_NAME);
    }

    public override bool RequiredProcessing() => true;

    public UnitResult<Error> StartProcessing()
    {
        if (Status != MediaStatus.UPLOADED)
            return Error.Validation("asset.invalid.status.transition", "Can only start processing from UPLOADING status");

        if (!RequiredProcessing())
            return Error.Validation("asset.processing.not.required", "This asset does not require processing");

        Status = MediaStatus.PROCESSING;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SetHlsMasterPlaylistKey(StorageKey value)
    {
        if (Status != MediaStatus.PROCESSING)
            return Error.Validation("video.invalid.status", "Can only set hls master playlist key from PROCESSING status");

        if (Key is not null)
            return Error.Validation("video.hls.key.exists", "HLS master playlist key already exists");

        Key = value;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> CompleteProcessing()
    {
        if (Status != MediaStatus.PROCESSING)
            return Error.Validation("video.invalid.status", "Can only complete processing from PROCESSING status");

        Status = MediaStatus.READY;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
}