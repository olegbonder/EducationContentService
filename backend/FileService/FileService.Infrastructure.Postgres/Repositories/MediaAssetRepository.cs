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

    public void Add(MediaAsset mediaAsset)
    {
        _context.MediaAssets.Add(mediaAsset);
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
        
    public async Task<Result<VideoAsset, Error>> GetVideoBy(Expression<Func<VideoAsset, bool>> predicate, CancellationToken cancellationToken)
    {
        var videoAsset = await _context.MediaAssets
            .OfType<VideoAsset>().FirstOrDefaultAsync(predicate, cancellationToken);
        if (videoAsset == null)
            return GeneralErrors.NotFound(null, "video_asset");

        return videoAsset;
    }
}