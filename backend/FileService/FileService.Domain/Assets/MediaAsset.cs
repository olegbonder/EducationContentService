using CSharpFunctionalExtensions;

namespace FileService.Domain.Assets
{
    public abstract class MediaAsset
    {
        public Guid Id { get; protected set; }

        public MediaData MediaData { get; protected set; } = null!;

        public AssetType AssetType { get; protected set; }

        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

        public StorageKey Key { get; protected set; } = null!;
        public MediaStatus Status { get; protected set; }

        protected MediaAsset()
        {
        }
        
        protected MediaAsset(
            Guid id, 
            MediaData mediaData,
            MediaStatus status,
            AssetType assetType,
            MediaOwner owner,
            StorageKey key)
        {
            Id = id;
            MediaData = mediaData;
            Status = status;
            AssetType = assetType;
            Key = key;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Result<MediaAsset, Error> CreateForUpload(MediaData mediaData,  AssetType assetType, MediaOwner owner)
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
    }
}