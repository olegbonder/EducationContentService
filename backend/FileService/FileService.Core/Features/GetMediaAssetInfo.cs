using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
using FileService.Domain;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace FileService.Core.Features
{
    public class GetMediaAssetInfo : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapGet("/files/{mediaAssetId:guid}", async Task<EndpointResult<GetMediaAssetDto?>> (
                    Guid mediaAssetId,
                    [FromServices] GetMediaAssetInfoHandler handler,
                    CancellationToken cancellationToken) =>
                await handler.Handle(mediaAssetId, cancellationToken));
        }        
    }

    public sealed class GetMediaAssetInfoHandler
    {        
        private readonly IReadDbContext _readDbContext;
        private readonly IS3Provider _s3Provider;

        public GetMediaAssetInfoHandler(
            IReadDbContext readDbContext,
            IS3Provider s3Provider)
        {
            _readDbContext = readDbContext;
            _s3Provider = s3Provider;
        }

        public async Task<Result<GetMediaAssetDto?, Error>> Handle(Guid mediaAssetId, CancellationToken cancellationToken)
        {
            var mediaAsset = await _readDbContext.MediaAssetsQuery
                .FirstOrDefaultAsync(m => m.Id == mediaAssetId, cancellationToken);
            
            if (mediaAsset == null)
                return Result.Success<GetMediaAssetDto?, Error>(null);

            string? url = null;

            if (mediaAsset.Status == MediaStatus.READY)
            {
                var urlsResult = await _s3Provider.GenerateDownloadUrlAsync(mediaAsset.Key);
                if (urlsResult.IsFailure)
                    return urlsResult.Error;

                url = urlsResult.Value;
            }
            

            var mediaAssetDto = new GetMediaAssetDto(
                    mediaAsset.Id,
                    mediaAsset.Status.ToString().ToLowerInvariant(),
                    mediaAsset.AssetType.ToString().ToLowerInvariant(),
                    url,
                    mediaAsset.MediaData.Size,
                    mediaAsset.MediaData.FileName.Value,
                    mediaAsset.MediaData.ContentType.Value
                );

            return mediaAssetDto;
        }
    }
}