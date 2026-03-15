using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FileService.Domain.Assets;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace FileService.Core.Features
{
    public class GetMediaAssets : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("/files/batch", async Task<EndpointResult<GetMediaAssetsResponse>> (
                    [FromBody] GetMediaAssetsRequest request,
                    [FromServices] GetMediaAssetsHandler handler,
                    CancellationToken cancellationToken) =>
                await handler.Handle(request, cancellationToken));
        }        
    }

    public sealed class GetMediaAssetsHandler
    {        
        private readonly IReadDbContext _readDbContext;
        private readonly IS3Provider _s3Provider;

        public GetMediaAssetsHandler(
            IReadDbContext readDbContext,
            IS3Provider s3Provider)
        {
            _readDbContext = readDbContext;
            _s3Provider = s3Provider;
        }

        public async Task<Result<GetMediaAssetsResponse, Error>> Handle(GetMediaAssetsRequest request, CancellationToken cancellationToken)
        {
            if (!request.MediaAssetIds.Any())
                return new GetMediaAssetsResponse([]);

            var mediaAssets = await _readDbContext.MediaAssetsQuery
                .Where(m => request.MediaAssetIds.Contains(m.Id) && m.Status != MediaStatus.DELETED).ToListAsync(cancellationToken);
            
            var readyMediaAssets = mediaAssets.Where(m => m.Status == MediaStatus.READY).ToList();
            var keys = readyMediaAssets.Select(m => m.Key).ToList();
            var urlsResult = await _s3Provider.GenerateDownloadUrlsAsync(keys);
            if (urlsResult.IsFailure)
                return urlsResult.Error;

            var urls = urlsResult.Value;
            var urlsDict = urls.ToDictionary(u => u.StorageKey, u => u.PresignedUrl);

            var results = new List<GetMediaAssetsDto>();
            foreach (MediaAsset mediaAsset in mediaAssets)
            {
                string? downloadUrl = null;
                if (urlsDict.TryGetValue(mediaAsset.Key, out string? url))
                {
                    downloadUrl = url;                   
                };

                var mediaAssetDto = new GetMediaAssetsDto(
                    mediaAsset.Id,
                    mediaAsset.Status.ToString().ToLowerInvariant(),
                    mediaAsset.AssetType.ToString().ToLowerInvariant(),
                    downloadUrl);
                results.Add(mediaAssetDto);
            }

            return new GetMediaAssetsResponse(results);
        }    
    }
}