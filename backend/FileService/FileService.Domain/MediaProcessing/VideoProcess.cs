using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.MediaProcessing
{
    public sealed class VideoProcess
    {
        private static readonly Dictionary<StepType, int> _stepWeights = new()
        {
            { StepType.INITIALIZATE, 0 },
            { StepType.EXTRACT_METADATA, 10 },
            { StepType.GENERATE_HLS, 60 },
            { StepType.UPLOAD_HLS, 15 },
            { StepType.GENERATE_PREVIEW, 10 },
            { StepType.CLEANUP, 5 }
        };
        
        private readonly List<ProcessingStep> _steps = [];
        
        public Guid Id { get; private set; }

        public Guid VideoAssetId { get; private set; }

        public ProcessingStatus Status { get; private set; }

        public int ProgressPercentage { get; private set; }

        public string? ErrorMessage { get; private set; }

        public bool IsCriticalError { get; private set; }

        public int RetryCount { get; private set; }

        public int MaxRetries { get; private set; } = 3;

        public DateTime? NextRetryAt { get; private set; }

        public DateTime StartedAt { get; private set; }

        public DateTime? CompletedAt { get; private set; }

        public IReadOnlyList<ProcessingStep> Steps => _steps.AsReadOnly();

        public ProcessingStep? CurrentStep => _steps.FirstOrDefault(x => x.Status == StepStatus.IN_PROGRESS);   

        public VideoProcess(Guid videoAssetId)
        {
            Id = Guid.NewGuid();
            VideoAssetId = videoAssetId;
            Status = ProcessingStatus.IN_PROGRESS;
            ProgressPercentage = 0;
            StartedAt = DateTime.UtcNow;

            InitializeSteps();
        }

        // EF Core
        public VideoProcess()
        {
            
        }

        private void InitializeSteps()
        {
            int order = 1;
            foreach ((StepType stepType, int weight) in _stepWeights)
            {
                _steps.Add(new ProcessingStep(stepType, order++, weight));
            }
        }

        public Result<ProcessingStep?, Error> ProcessingNextStep()
        {
            if (Status != ProcessingStatus.IN_PROGRESS)
                return Error.Failure("processing.invalid.status", $"Cannot process step with status: {Status}");

            ProcessingStep? currentStep = CurrentStep;
            if (currentStep is not null)
                return currentStep;

            ProcessingStep? nextStep = _steps
                .OrderBy(x => x.Order)
                .FirstOrDefault(x => x.Status == StepStatus.PENDING);

            if (nextStep is null)
            {
                Complete();
                return Result.Success<ProcessingStep?, Error>(null);
            }

            var startResult = nextStep.Start();
            if (startResult.IsFailure)
                return startResult.Error;

            return nextStep;
        }

        public UnitResult<Error> CompleteCurrentStep(string? resultData = null)
        {
            if (Status != ProcessingStatus.IN_PROGRESS)
                return Error.Failure("processing.invalid.status", $"Cannot complete step when status is {Status}");

            ProcessingStep? currentStep = CurrentStep;
            if (currentStep is null)
                return Error.Failure("processing.no.active.step", "No active step to complete");

            var completeResult = currentStep.Complete(resultData);
            if (completeResult.IsFailure)
                return completeResult.Error;

            RecalculateProgress();

            return UnitResult.Success<Error>();
        }

        public UnitResult<Error> FailCurrentStep(string errorMessage)
        {
            if (Status != ProcessingStatus.IN_PROGRESS)
                return Error.Failure("processing.invalid.status", $"Cannot fail step when status is {Status}");

            ProcessingStep? currentStep = CurrentStep;
            if (currentStep is null)
                return Error.Failure("processing.no.active.step", "No active step to fail");

            var failResult = currentStep.Fail(errorMessage);
            if (failResult.IsFailure)
                return failResult.Error;

            return UnitResult.Success<Error>();
        }

        public UnitResult<Error> Fail(string errorMessage, bool isCritical= false)
        {
            if (Status != ProcessingStatus.IN_PROGRESS)
                return Error.Failure("processing.invalid.status", $"Can only fail from IN_PROGRESS status, current status: {Status}");

            if (string.IsNullOrWhiteSpace(errorMessage))
                return Error.Failure("processing.error.required", "Error message is required");

            Status = ProcessingStatus.FAILED;
            ErrorMessage = errorMessage;
            CompletedAt = DateTime.UtcNow;
            IsCriticalError = isCritical;

            return UnitResult.Success<Error>();
        }

        public bool CanRetry() => RetryCount < MaxRetries && !IsCriticalError;

        public UnitResult<Error> Reset()
        {
            if (Status != ProcessingStatus.FAILED)
                return Error.Failure("processing.invalid.status", $"Can only retry from FAILED status, current status: {Status}");

            Status = ProcessingStatus.IN_PROGRESS;
            ProgressPercentage = 0;
            ErrorMessage = null;
            CompletedAt = null;
            IsCriticalError = false;

            foreach (var step in _steps)
            {
                step.Reset();
            }
                
            return UnitResult.Success<Error>();
        }

        public UnitResult<Error> ScheduleRetry(DateTime nextRetryAt)
        {
            if (Status != ProcessingStatus.FAILED)
                return Error.Failure("processing.invalid.status", $"Can only retry from FAILED status, current status: {Status}");

            if (IsCriticalError)
                return Error.Failure("processing.retry.critical", "Cannot retry critical error");

            if (RetryCount >= MaxRetries)
                return Error.Failure("processing.retry.exceeded", "Max retry exceeded");

            RetryCount++;
            NextRetryAt = nextRetryAt;

            return UnitResult.Success<Error>();
        }

        private void RecalculateProgress()
        {
            int totalProgress = _steps
                .Where(x => x.Status == StepStatus.COMPLETED)
                .Sum(x => x.Weight);

            ProgressPercentage = totalProgress;
        }

        private UnitResult<Error> Complete()
        {
            if (Status != ProcessingStatus.IN_PROGRESS)
                return Error.Failure("processing.invalid.status", $"Can only complete from IN_PROGRESS status, current status: {Status}");

            bool allStepsCompleted = _steps.All(x => x.Status == StepStatus.COMPLETED);
            if (!allStepsCompleted)
                return Error.Failure("processing.incomplete.status", $"Can only complete when all steps are completed, current status: {Status}");
            Status = ProcessingStatus.COMPLETED;
            CompletedAt = DateTime.UtcNow;
            ProgressPercentage = 100;

            return UnitResult.Success<Error>();
        }
    }

    public enum ProcessingStatus
    {
        IN_PROGRESS,
        COMPLETED,
        FAILED
    }
}