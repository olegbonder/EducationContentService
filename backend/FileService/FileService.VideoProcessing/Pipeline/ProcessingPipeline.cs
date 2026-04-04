using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Core.Database;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.VideoProcessing.Pipeline
{
    public class ProcessingPipeline : IProcessingPipeline
    {
        private readonly IEnumerable<IProcessingStepHandler> _stepHandlers;
        private readonly ILogger<ProcessingPipeline> _logger;
        private readonly IMediaAssetRepository _mediaAssetRepository;
        private readonly IVideoProcessingRepository _videoProcessingRepository;
        private readonly ITransactionManager _transactionManager;

        public ProcessingPipeline(
            IEnumerable<IProcessingStepHandler> stepHandlers,
            ILogger<ProcessingPipeline> logger,
            IMediaAssetRepository mediaAssetRepository,
            IVideoProcessingRepository videoProcessingRepository,
            ITransactionManager transactionManager)
        {
            _stepHandlers = stepHandlers;
            _logger = logger;
            _mediaAssetRepository = mediaAssetRepository;
            _videoProcessingRepository = videoProcessingRepository;
            _transactionManager = transactionManager;
        }

        public async Task<UnitResult<Error>> ProcessAllStepsAsync(
            Guid videoAssetId,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await LoadContextAsync(videoAssetId, cancellationToken);
            if (contextResult.IsFailure)
                return contextResult.Error;

            var context = contextResult.Value;
            
            var executionResult = await ExecuteAllStepsAsync(context, cancellationToken);

            if (executionResult.IsFailure)
            {
                return await FinalizeWithFailureAsync(context, executionResult.Error, cancellationToken);    
            }

            return await FinalizeAsync(context, cancellationToken);
        }

        private async Task<UnitResult<Error>> FinalizeAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            var videoAssetId = context.VideoProcess.VideoAssetId;

            context.VideoAsset.CompleteProcessing();
            context.VideoProcess.Complete();

            _logger.LogInformation(
                "Processing completed for video asset {VideoAssetId}.", 
                videoAssetId);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to save success state for video asset {VideoAssetId}", 
                    videoAssetId);

                return saveResult.Error;
            }

            return UnitResult.Success<Error>();
        }


        private async Task<UnitResult<Error>> FinalizeWithFailureAsync(
            ProcessingContext context,
            Error error,
            CancellationToken cancellationToken)
        {
            var videoAssetId = context.VideoProcess.VideoAssetId;

            context.VideoProcess.Fail(error.GetMessage());

            _logger.LogError(
                "Processing failed for video asset {VideoAssetId}, Error: {Error}", 
                videoAssetId, 
                error.GetMessage());

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to save failure state for video asset {VideoAssetId}", 
                    videoAssetId);

                return saveResult.Error;
            }
            
            return UnitResult.Failure(error);
        }

        private async Task<UnitResult<Error>> ExecuteAllStepsAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            var videoAssetId = context.VideoProcess.VideoAssetId;
            while (true)
            {
                var stepResult = context.VideoProcess.ProcessingNextStep();
                if (stepResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Failed to process next step for video asset {VideoAssetId}, Status: {Status}",
                        videoAssetId,
                        context.VideoProcess.Status);
                    return stepResult.Error;
                }

                if (stepResult.Value == null)
                {
                    _logger.LogInformation(
                        "All steps processed for video asset {VideoAssetId}, Status: {Status}",
                        videoAssetId,
                        context.VideoProcess.Status);
                    return UnitResult.Success<Error>();
                }

                var currentStep = stepResult.Value;

                _logger.LogInformation(
                    "Processing step {StepType} (Order: {StepOrder}) for video asset {VideoAssetId}",
                    currentStep.StepType,
                    currentStep.Order,
                    videoAssetId);

                var stepHandler = _stepHandlers
                    .FirstOrDefault(x => x.StepType == currentStep.StepType);

                if (stepHandler == null)
                {
                    string error = $"No handler found for step {currentStep.StepType}";
                    _logger.LogError(
                        "No handler found for step {StepType} for video asset {VideoAssetId}",
                        currentStep.StepType,
                        videoAssetId);

                    context.VideoProcess.FailCurrentStep(error);
                    context.VideoProcess.Fail(error, true);
                    var saveReult = await _transactionManager.SaveChangesAsync(cancellationToken);
                    if (saveReult.IsFailure)
                    {
                        _logger.LogError(
                            "Failed to save changes to database while failing step {StepType} for video asset {VideoAssetId}",
                            currentStep.StepType,
                            videoAssetId);
                    }
                    return Error.Failure("pipeline.step.handler.not.found", error);
                }

                var executionResult = await ExecuteStepSafetyAsync(
                    stepHandler,
                    context, 
                    cancellationToken);

                if (executionResult.IsFailure)
                {
                    _logger.LogError(
                        "Failed to execute step {StepType} for video asset {VideoAssetId}, Error: {ErrorMessage}",
                        currentStep.StepType,
                        videoAssetId,
                        executionResult.Error);

                    context.VideoProcess.FailCurrentStep(executionResult.Error.Messages[0].Message);
                    context.VideoProcess.Fail(
                        executionResult.Error.Messages[0].Message, 
                        true);

                    var saveReult = await _transactionManager.SaveChangesAsync(cancellationToken);
                    if (saveReult.IsFailure)
                    {
                        _logger.LogError(
                            "Failed to save changes to database while failing step {StepType} for video asset {VideoAssetId}",
                            currentStep.StepType,
                            videoAssetId);
                    }

                    return executionResult.Error;
                }

                context.VideoProcess.CompleteCurrentStep();

                _logger.LogInformation(
                    "Completed step {StepType} (Order: {StepOrder}) for video asset {VideoAssetId}, Progress: {Progress}",
                    currentStep.StepType,
                    currentStep.Order,
                    videoAssetId,
                    context.VideoProcess.ProgressPercentage
                );

                var completeSaveReult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (completeSaveReult.IsFailure)
                {
                    _logger.LogError(
                        "Failed to save changes to database after step {StepType} for video asset {VideoAssetId}",
                        currentStep.StepType,
                        videoAssetId);
                }
            }
        }


        private async Task<Result<ProcessingContext, Error>> ExecuteStepSafetyAsync(
            IProcessingStepHandler step,
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            try
            {
                return await step.ExecuteAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Unhandler exception in step {StepType} for video asset {VideoAssetId}", 
                    context.VideoProcess.CurrentStep.StepType, 
                    context.VideoProcess.VideoAssetId);

                return Error.Failure("pipeline.step.exception", $"Step execution failed: {ex.Message}");
            }
        }

        private async Task<Result<ProcessingContext, Error>> LoadContextAsync(
            Guid videoAssetId,
            CancellationToken cancellationToken)
        {
            var processResult = await _videoProcessingRepository
                .GetBy(v => v.Id == videoAssetId, cancellationToken);

            VideoProcess videoProcess;

            if (processResult.IsFailure)
            {
                var newProcess = new VideoProcess(videoAssetId);
                videoProcess = newProcess;

                _videoProcessingRepository.Add(newProcess);

                _logger.LogInformation("Created new process for video asset {VideoAssetId}.", videoAssetId);
            }
            else
            {
                videoProcess = processResult.Value;
                _logger.LogInformation("Found existing process for video asset {VideoAssetId}.", videoAssetId);
            }
            
            var assetResult = await _mediaAssetRepository
                .GetVideoBy(v => v.Id == videoAssetId, cancellationToken);
            if (assetResult.IsFailure)
                return assetResult.Error;

            var startResult = assetResult.Value.StartProcessing();
            if (startResult.IsFailure)
                return startResult.Error;

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
                return saveResult.Error;

            var processingContext = new ProcessingContext
            {
                VideoAsset = assetResult.Value,
                VideoProcess = videoProcess
            };

            return processingContext;
        }
    }
}