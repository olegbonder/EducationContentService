using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Core.Database;
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

namespace FileService.Core.Features
{
    public class AbortMultiPartUpload : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("/files/abort-upload", async Task<EndpointResult> (
                    [FromBody] AbortMultiPartUploadRequest request,
                    [FromServices] AbortMultiPartUploadHandler handler,
                    CancellationToken cancellationToken) =>
                await handler.Handle(request, cancellationToken));
        }
    }

    public sealed class AbortMultiPartUploadHandler
    {
        private readonly IMediaAssetRepository _mediaAssetRepository;
        private readonly ILogger<AbortMultiPartUploadHandler> _logger;
        private readonly IS3Provider _s3Provider;
        private readonly ITransactionManager _transactionManager;

        public AbortMultiPartUploadHandler(
            IMediaAssetRepository mediaAssetRepository,
            ILogger<AbortMultiPartUploadHandler> logger,
            IS3Provider s3Provider,
            ITransactionManager transactionManager)
        {
            _mediaAssetRepository = mediaAssetRepository;
            _logger = logger;
            _s3Provider = s3Provider;
            _transactionManager = transactionManager;
        }

        public async Task<UnitResult<Error>> Handle(AbortMultiPartUploadRequest request, CancellationToken cancellationToken)
        {
            (_, bool isFailure, MediaAsset? mediaAsset, Error? error) = await _mediaAssetRepository
                .GetBy(m => m.Id == request.MediaAssetId, cancellationToken);
            if (isFailure)
                return error;

            if (mediaAsset.Status != MediaStatus.UPLOADING)
            {
                return GeneralErrors.Failure("Only files in UPLOADING status can be aborted");
            }

            if (!string.IsNullOrEmpty(mediaAsset.UploadId))
            {
                var completeResult = await _s3Provider.AbortMultiPartUploadAsync(
                    mediaAsset.UploadKey,
                    mediaAsset.UploadId,
                    cancellationToken
                );

                if (completeResult.IsFailure)
                {
                    mediaAsset.MarkFailed();
                    await _transactionManager.SaveChangesAsync(cancellationToken);
                    return completeResult.Error;
                }
            }

            _mediaAssetRepository.Delete(mediaAsset);
            await _transactionManager.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Aborted multipart upload for MediaAssetId: {MediaAssetId}", mediaAsset.Id);        

            return Result.Success<Error>();
        }
    }
}