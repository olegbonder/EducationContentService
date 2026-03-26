using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.Assets
{
    public abstract class MediaAsset
    {
        public Guid Id { get; protected set; }

        public MediaData MediaData { get; protected set; } = null!;

        public AssetType AssetType { get; protected set; }
        //public MediaOwner Owner { get; protected set; }

        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

        public StorageKey? Key { get; protected set; }
        public StorageKey? RawKey { get; protected set; }
        public MediaStatus Status { get; protected set; }

        public StorageKey? UploadKey => RequiredProcessing() ? RawKey : Key;

        protected MediaAsset()
        {
        }
        
        protected MediaAsset(
            Guid id, 
            MediaData mediaData,
            MediaStatus status,
            AssetType assetType,
            StorageKey key,
            bool isDirectUpload = false)
        {
            Id = id;
            MediaData = mediaData;
            Status = status;
            AssetType = assetType;            
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
            if (isDirectUpload)
            {
                Key = key;
            }
            else
            {
                RawKey = key;
            }

        }

        public static Result<MediaAsset, Error> CreateForUpload(MediaData mediaData,  AssetType assetType)
        {
            var assetId = Guid.NewGuid();

            switch (assetType)
            {
                case AssetType.VIDEO:
                    var videoResult = VideoAsset.CreateForUpload(assetId, mediaData);
                    return videoResult.IsFailure ? videoResult.Error : videoResult.Value;
                case AssetType.PREVIEW:
                    var previewResult = PreviewAsset.CreateForUpload(assetId, mediaData);
                    return previewResult.IsFailure ? previewResult.Error : previewResult.Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
            }
        }

        public virtual bool RequiredProcessing() => false;

        public Result MarkUploaded()
        {
            if (Status == MediaStatus.UPLOADED)
                return Result.Success();

            Status = MediaStatus.UPLOADED;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}