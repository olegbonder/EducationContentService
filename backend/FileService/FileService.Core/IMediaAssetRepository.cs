using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IMediaAssetRepository
{
    Task<Result<Guid, Error>> Add(MediaAsset mediaAsset, CancellationToken cancellationToken);

    Task<Result<MediaAsset, Error>> GetBy(Expression<Func<MediaAsset, bool>> predicate, CancellationToken cancellationToken);

    Task<Result<MediaAsset, Error>> GetById(Guid mediaAssetId, CancellationToken cancellationToken);

    Task<int> SaveAsync(CancellationToken cancellationToken);
}