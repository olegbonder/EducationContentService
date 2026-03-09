using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IMediaAssetRepository
{
    Task<Result<Guid, Error>> Add(MediaAsset mediaAsset, CancellationToken cancellationToken);

    Task<MediaAsset?> GetBy(Expression<Func<MediaAsset, bool>> predicate, CancellationToken cancellationToken);

    Task<Result<MediaAsset, Error>> GetById(Guid mediaAssetId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> Delete(Guid mediaAssetId, CancellationToken cancellationToken);
}