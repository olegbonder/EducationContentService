using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.VideoProcessing.Pipeline.Steps
{
    public sealed class InitializeStepHandler : IProcessingStepHandler
    {
        private readonly ILogger<InitializeStepHandler> _logger;

        public StepType StepType => StepType.INITIALIZATE;

        public InitializeStepHandler(ILogger<InitializeStepHandler> logger)
        {
            _logger = logger;
        }

        public Task<Result<ProcessingContext, Error>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Initializing video processing for video asset {VideoAssetId}.",
                 context.VideoProcess.VideoAssetId);

            var createResult = context.CreateWorkingDirectory();
            if (createResult.IsFailure)
                return Task.FromResult(Result.Failure<ProcessingContext, Error>(createResult.Error));
            
            return Task.FromResult(Result.Success<ProcessingContext, Error>(context)) ;
        }
    }
}