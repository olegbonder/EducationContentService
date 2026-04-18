using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Core.Database;
using FileService.Core.FilesStorage;
using FileService.Core.Processing;
using FileService.Domain.Assets;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Quartz;
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
        private readonly ITransactionManager _transactionManager;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IEnumerable<IProcessingJobFactory> _processingJobFactories;

        public CompleteMultiPartUploadHandler(
            IMediaAssetRepository mediaAssetRepository,
            ILogger<StartMultiPartUploadHandler> logger,
            IS3Provider s3Provider,
            ITransactionManager transactionManager,
            ISchedulerFactory  schedulerFactory,
            IEnumerable<IProcessingJobFactory> processingJobFactories)
        {
            _mediaAssetRepository = mediaAssetRepository;
            _logger = logger;
            _s3Provider = s3Provider;
            _transactionManager = transactionManager;
            _schedulerFactory = schedulerFactory;
            _processingJobFactories = processingJobFactories;
        }

        public async Task<UnitResult<Error>> Handle(CompleteMultiPartUploadRequest request, CancellationToken cancellationToken)
        {
            (_, bool isFailure, MediaAsset? mediaAsset, Error? error) = await _mediaAssetRepository
                .GetBy(m => m.Id == request.MediaAssetId, cancellationToken);
            if (isFailure)
                return error;


            if (mediaAsset.MediaData.ExpectedChunksCount != request.PartETags.Count)
                return GeneralErrors.Failure("Количество etags не соответствует количеству чанков");

            var completeResult = await _s3Provider.CompleteMultiPartUploadAsync(
                mediaAsset.UploadKey,
                request.UploadId,
                request.PartETags,
                cancellationToken
            );
            

            try
            {
                var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);

                if (completeResult.IsFailure)
                {
                    mediaAsset.MarkFailed();
                    await _transactionManager.SaveChangesAsync(cancellationToken);
                    return completeResult.Error;
                }
                    
                mediaAsset.MarkUploaded();
                await _transactionManager.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("File uploading successfully. MediaAssetId: {MediaAssetId}", mediaAsset.Id);

                if (mediaAsset.RequiredProcessing())
                {
                    var factory = _processingJobFactories.FirstOrDefault(f => f.CanProcess(mediaAsset));
                    if (factory == null)
                    {
                        _logger.LogError("No  processing job available for mediaAssetId: {MediaAssetId}", mediaAsset.Id);
                        return GeneralErrors.Failure("No processing job factory found");
                    }
                    var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

                    var job = factory.CreateJob(mediaAsset);
                    var trigger = factory.CreateTrigger(mediaAsset);
                    
                    await scheduler.ScheduleJob(job, trigger, cancellationToken);
                    
                    _logger.LogInformation("Scheduled processing job completed. MediaAssetId: {MediaAssetId}", mediaAsset.Id);
                }
                else
                {
                    var markReadyResult = mediaAsset.MarkReady();
                    if (markReadyResult.IsFailure)
                        return markReadyResult.Error;
                    
                    _logger.LogInformation("MediaAssetId: {MediaAssetId} does not require processing", mediaAsset.Id);
                }

                var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                    return saveResult.Error;

                await _transactionManager.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error completing multipart upload for MediaAssetId: {MediaAssetId}", mediaAsset.Id);
                return GeneralErrors.Failure("Error completing multipart upload");
            }
            return Result.Success<Error>();
        }
    }
}