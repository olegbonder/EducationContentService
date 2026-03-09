using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Infrastructure.Postgres;

public class MediaAssetRepository : IMediaAssetRepository
{
    private readonly FileServiceDbContext _context;
    private readonly ILogger<MediaAssetRepository> _logger;

    public MediaAssetRepository(FileServiceDbContext context, ILogger<MediaAssetRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        var fileInfo = $"Файл: {mediaAsset.MediaData.FileName} Тип: {mediaAsset.AssetType} Путь к S3:{mediaAsset.Key.FullPath}";
        try
        {
            await _context.MediaAssets.AddAsync(mediaAsset, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return mediaAsset.Id;
        }
        catch(OperationCanceledException ex)
        {
            _logger.LogError(ex, "Отмена операции добавления медиа-файла {fileInfo}", fileInfo);
            return GeneralErrors.Failure("create.media_asset");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка добавления медиа-файла {fileInfo}", fileInfo);
            return GeneralErrors.Failure("create.media_asset");
        }
    }

    public async Task<MediaAsset?> GetBy(
        Expression<Func<MediaAsset, bool>> predicate, CancellationToken cancellationToken) =>
        await _context.MediaAssets.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<Result<MediaAsset, Error>> GetById(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        var mediaAsset = await GetBy(m => m.Id == mediaAssetId, cancellationToken);
        if (mediaAsset == null)
            return GeneralErrors.NotFound(mediaAssetId, "media_asset");

        return mediaAsset;
    }

    public async Task<UnitResult<Error>> Delete(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        string fileInfo = $"Id: {mediaAssetId}";
        try
        {
            int deletedCount = await _context.MediaAssets
                .Where(m => m.Id == mediaAssetId)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount == 0)
                return GeneralErrors.NotFound(mediaAssetId, "media_asset");

            _logger.LogInformation("Медиа-файл с {fileInfo} успешно удален", fileInfo);
            return UnitResult.Success<Error>();
        }
        catch(OperationCanceledException ex)
        {
            _logger.LogError(ex, "Отмена операции удаления медиа-файла {fileInfo}", fileInfo);
            return UnitResult.Failure<Error>(Error.Failure(new ErrorMessage("delete.media_asset", "error", null)));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ошибка удаления медиа-файла {fileInfo}", fileInfo);
            return UnitResult.Failure<Error>(Error.Failure(new ErrorMessage("delete.media_asset", "error", null)));
        }
    }
}