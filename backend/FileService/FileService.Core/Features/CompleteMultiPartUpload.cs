using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
using FileService.Domain.Assets;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features
{
    public class CompleteMultiPartUpload : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("/files/complete-upload", async Task<EndpointResult> (
                    [FromBody] CompleteMultiPartUploadRequest request,
                    [FromServices] CompleteMultiPartUploadHandler handler,
                    CancellationToken cancellationToken) =>
                await handler.Handle(request, cancellationToken));
        }
    }

    public sealed class CompleteMultiPartUploadHandler
    {
        private readonly IMediaAssetRepository _mediaAssetRepository;
        private readonly ILogger<StartMultiPartUploadHandler> _logger;
        private readonly IS3Provider _s3Provider;

        public CompleteMultiPartUploadHandler(
            IMediaAssetRepository mediaAssetRepository,
            ILogger<StartMultiPartUploadHandler> logger,
            IS3Provider s3Provider)
        {
            _mediaAssetRepository = mediaAssetRepository;
            _logger = logger;
            _s3Provider = s3Provider;
        }

        public async Task<UnitResult<Error>> Handle(CompleteMultiPartUploadRequest request, CancellationToken cancellationToken)
        {
            (_, bool isFailure, MediaAsset? mediaAsset, Error? error) = await _mediaAssetRepository.GetBy(m => m.Id == request.MediaAssetId, cancellationToken);
            if (isFailure)
                return error;


            if (mediaAsset.MediaData.ExpectedChunksCount != request.PartETags.Count)
                return GeneralErrors.Failure("Количество etags не соответствует количеству чанков");

            var completeResult = await _s3Provider.CompleteMultiPartUploadAsync(
                mediaAsset.Key,
                request.UploadId,
                request.PartETags,
                cancellationToken
            );
            if (completeResult.IsFailure)
                return completeResult.Error;

            mediaAsset.MarkUploaded();
            await _mediaAssetRepository.SaveAsync(cancellationToken);

            _logger.LogInformation("File uploading sucessfully. MediaAssetId: {MediaAssetId}", mediaAsset.Id);

            return Result.Success<Error>();
        }
    }
}