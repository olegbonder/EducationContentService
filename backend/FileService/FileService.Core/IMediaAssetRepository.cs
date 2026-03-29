using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IMediaAssetRepository
{
    void Add(MediaAsset mediaAsset);

    Task<Result<MediaAsset, Error>> GetBy(Expression<Func<MediaAsset, bool>> predicate, CancellationToken cancellationToken);

    Task<Result<VideoAsset, Error>> GetVideoBy(Expression<Func<VideoAsset, bool>> predicate, CancellationToken cancellationToken);

    Task<Result<MediaAsset, Error>> GetById(Guid mediaAssetId, CancellationToken cancellationToken);
}