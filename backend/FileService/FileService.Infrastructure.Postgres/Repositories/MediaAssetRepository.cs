using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Infrastructure.Postgres.Repositories;

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

    public async Task<Result<MediaAsset, Error>> GetBy(
        Expression<Func<MediaAsset, bool>> predicate, 
        CancellationToken cancellationToken)
    {
        var mediaAsset = await _context.MediaAssets.FirstOrDefaultAsync(predicate, cancellationToken);
        if (mediaAsset == null)
            return GeneralErrors.NotFound(null, "media_asset");

        return mediaAsset;
    }
        

    public async Task<Result<MediaAsset, Error>> GetById(Guid mediaAssetId, CancellationToken cancellationToken) =>
        await GetBy(m => m.Id == mediaAssetId, cancellationToken);
    public async Task<int> SaveAsync(CancellationToken cancellationToken) => 
        await _context.SaveChangesAsync(cancellationToken);
}