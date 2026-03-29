using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FileService.Domain.Assets;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public sealed class StartMultiPartUpload : IEndpoint
{
    public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/files/multipart-upload", async Task<EndpointResult<StartMultiPartUploadResponse>> (
                [FromBody] StartMultiPartUploadRequest request,
                [FromServices] StartMultiPartUploadHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(request, cancellationToken));
    }
}

public sealed class StartMultiPartUploadHandler
{
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly ILogger<StartMultiPartUploadHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly ITransactionManager _transactionManager;

    public StartMultiPartUploadHandler(
        IMediaAssetRepository mediaAssetRepository,
        ILogger<StartMultiPartUploadHandler> logger,
        IS3Provider s3Provider,
        IChunkSizeCalculator chunkSizeCalculator,
        ITransactionManager transactionManager)
    {
        _mediaAssetRepository = mediaAssetRepository;
        _logger = logger;
        _s3Provider = s3Provider;
        _chunkSizeCalculator = chunkSizeCalculator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<StartMultiPartUploadResponse, Error>> Handle(StartMultiPartUploadRequest request, CancellationToken cancellationToken)
    {
        var fileNameResult = FileName.Create(request.FileName);
        if (fileNameResult.IsFailure)
            return fileNameResult.Error;

        var contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error;

        var chunkCalculationResult = _chunkSizeCalculator.CalculateChunkSize(request.Size);
        if (chunkCalculationResult.IsFailure)
            return chunkCalculationResult.Error;

        var mediaDataResult = MediaData.Create(
            fileNameResult.Value, 
            contentTypeResult.Value, 
            request.Size, 
            chunkCalculationResult.Value.TotalChunks);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error;

        var mediaAssetResult = MediaAsset.CreateForUpload(mediaDataResult.Value, request.AssetType.ToAssetType());
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        _mediaAssetRepository.Add(mediaAssetResult.Value);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;        

        var mediaAsset = mediaAssetResult.Value;
        var startUploadResult = await _s3Provider.StartMultiPartUploadAsync(mediaAsset.UploadKey, mediaAsset.MediaData, cancellationToken);
        if (startUploadResult.IsFailure)
            return startUploadResult.Error;

        var chunksUploadUrlsResult = await _s3Provider.GenerateAllChunksUploadUrlsAsync(
            mediaAsset.UploadKey, 
            startUploadResult.Value, 
            chunkCalculationResult.Value.TotalChunks, 
            cancellationToken);
        if (chunksUploadUrlsResult.IsFailure)
            return chunksUploadUrlsResult.Error;

        _logger.LogInformation("Media Asset started uploading: {MediaAssetId} with key: {StorageKey}", mediaAsset.Id, mediaAsset.Key);

        return new StartMultiPartUploadResponse(
            mediaAsset.Id,
            startUploadResult.Value,
            chunksUploadUrlsResult.Value,
            chunkCalculationResult.Value.ChunkSize
        );
    }
}
