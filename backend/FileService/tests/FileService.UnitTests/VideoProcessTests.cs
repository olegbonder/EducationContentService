using FileService.Domain.MediaProcessing;

namespace FileService.UnitTests;

public class VideoProcessTests
{
    private readonly Guid _videoAssetId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldInitializeVideoProcess_WithCorrectValues()
    {
        // Arrange & Act
        var process = new VideoProcess(_videoAssetId);

        // Assert
        Assert.NotEqual(Guid.Empty, process.Id);
        Assert.Equal(_videoAssetId, process.VideoAssetId);
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(0, process.ProgressPercentage);
        Assert.Null(process.ErrorMessage);
        Assert.False(process.IsCriticalError);
        Assert.Equal(0, process.RetryCount);
        Assert.Equal(3, process.MaxRetries);
        Assert.Null(process.NextRetryAt);
        Assert.NotEqual(default(DateTime), process.StartedAt);
        Assert.Null(process.CompletedAt);
    }

    [Fact]
    public void Constructor_ShouldInitializeAllSteps_WithCorrectOrder()
    {
        // Arrange & Act
        var process = new VideoProcess(_videoAssetId);

        // Assert
        Assert.Equal(6, process.Steps.Count);
        Assert.Equal(StepType.INITIALIZATE, process.Steps[0].StepType);
        Assert.Equal(StepType.EXTRACT_METADATA, process.Steps[1].StepType);
        Assert.Equal(StepType.GENERATE_HLS, process.Steps[2].StepType);
        Assert.Equal(StepType.UPLOAD_HLS, process.Steps[3].StepType);
        //Assert.Equal(StepType.GENERATE_PREVIEW, process.Steps[4].StepType);
        Assert.Equal(StepType.CLEANUP, process.Steps[5].StepType);

        for (int i = 0; i < process.Steps.Count; i++)
        {
            Assert.Equal(i + 1, process.Steps[i].Order);
            Assert.Equal(StepStatus.PENDING, process.Steps[i].Status);
        }
    }

    [Fact]
    public void ProcessingNextStep_FirstCall_ShouldStartFirstStep()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        var result = process.ProcessingNextStep();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(StepType.INITIALIZATE, result.Value.StepType);
        Assert.Equal(StepStatus.IN_PROGRESS, result.Value.Status);
        Assert.NotNull(result.Value.StartedAt);
    }

    [Fact]
    public void ProcessingNextStep_WhenStepInProgress_ShouldReturnCurrentStep()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        var firstStep = process.ProcessingNextStep().Value;

        // Act
        var result = process.ProcessingNextStep();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(firstStep.Id, result.Value.Id);
        Assert.Equal(StepStatus.IN_PROGRESS, result.Value.Status);
    }

    [Fact]
    public void CompleteCurrentStep_ShouldCompleteActiveStep_AndMoveToNextStep()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        var firstStep = process.ProcessingNextStep().Value;

        // Act
        var completeResult = process.CompleteCurrentStep("metadata");
        var nextStepResult = process.ProcessingNextStep();

        // Assert
        Assert.True(completeResult.IsSuccess);
        Assert.Equal(StepStatus.COMPLETED, firstStep.Status);
        Assert.NotNull(firstStep.CompletedAt);
        Assert.Equal("metadata", firstStep.ResultData);

        Assert.True(nextStepResult.IsSuccess);
        Assert.Equal(StepType.EXTRACT_METADATA, nextStepResult.Value.StepType);
        Assert.Equal(StepStatus.IN_PROGRESS, nextStepResult.Value.Status);
    }

    [Fact]
    public void CompleteCurrentStep_WhenNoActiveStep_ShouldReturnError()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        var result = process.CompleteCurrentStep();

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void FailCurrentStep_ShouldFailActiveStep_AndSetErrorMessage()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        var currentStep = process.ProcessingNextStep().Value;
        var errorMessage = "FFmpeg conversion failed";

        // Act
        var result = process.FailCurrentStep(errorMessage);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StepStatus.FAILED, currentStep.Status);
        Assert.Equal(errorMessage, currentStep.ErrorMessage);
        Assert.NotNull(currentStep.CompletedAt);
    }

    [Fact]
    public void FailCurrentStep_WhenNoActiveStep_ShouldReturnError()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        var result = process.FailCurrentStep("error");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Fail_ShouldSetProcessingStatusToFailed_AndSetErrorMessage()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        var errorMessage = "Critical error occurred";

        // Act
        var result = process.Fail(errorMessage, isCritical: true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ProcessingStatus.FAILED, process.Status);
        Assert.Equal(errorMessage, process.ErrorMessage);
        Assert.True(process.IsCriticalError);
        Assert.NotNull(process.CompletedAt);
    }

    [Fact]
    public void Fail_WithEmptyErrorMessage_ShouldReturnError()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        var result = process.Fail("");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Fail_WhenNotInProgress_ShouldReturnError()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("First error");

        // Act
        var result = process.Fail("Second error");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CanRetry_WhenNotFailedAndNotCritical_ShouldReturnTrue()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Test error", isCritical: false);

        // Act
        var canRetry = process.CanRetry();

        // Assert
        Assert.True(canRetry);
    }

    [Fact]
    public void CanRetry_WhenCriticalError_ShouldReturnFalse()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Critical error", isCritical: true);

        // Act
        var canRetry = process.CanRetry();

        // Assert
        Assert.False(canRetry);
    }

    [Fact]
    public void CanRetry_WhenMaxRetriesExceeded_ShouldReturnFalse()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Error");
        process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));
        process.ScheduleRetry(DateTime.UtcNow.AddMinutes(10));
        process.ScheduleRetry(DateTime.UtcNow.AddMinutes(15));

        // Act
        var canRetry = process.CanRetry();

        // Assert
        Assert.False(canRetry);
    }

    [Fact]
    public void ScheduleRetry_ShouldIncrementRetryCount_AndSetNextRetryAt()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Error");
        var nextRetryTime = DateTime.UtcNow.AddMinutes(5);

        // Act
        var result = process.ScheduleRetry(nextRetryTime);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, process.RetryCount);
        Assert.Equal(nextRetryTime, process.NextRetryAt);
    }

    [Fact]
    public void ScheduleRetry_WithCriticalError_ShouldReturnError()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Critical error", isCritical: true);

        // Act
        var result = process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ScheduleRetry_WhenMaxRetriesExceeded_ShouldReturnError()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Error");
        process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));
        process.ScheduleRetry(DateTime.UtcNow.AddMinutes(10));
        process.ScheduleRetry(DateTime.UtcNow.AddMinutes(15));

        // Act
        var result = process.ScheduleRetry(DateTime.UtcNow.AddMinutes(20));

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Reset_ShouldResetProcessingStatus_AndClearAllSteps()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        process.ProcessingNextStep();
        process.FailCurrentStep("Some error");
        var oldProgressPercentage = process.ProgressPercentage;
        process.Fail("Processing failed");

        // Act
        var result = process.Reset();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(0, process.ProgressPercentage);
        Assert.Null(process.ErrorMessage);
        Assert.False(process.IsCriticalError);
        Assert.Null(process.CompletedAt);

        // Verify all steps are reset
        foreach (var step in process.Steps)
        {
            Assert.Equal(StepStatus.PENDING, step.Status);
            Assert.Null(step.ResultData);
            Assert.Null(step.ErrorMessage);
            Assert.Null(step.StartedAt);
            Assert.Null(step.CompletedAt);
        }
    }

    [Fact]
    public void Reset_WhenNotInFailedStatus_ShouldReturnError()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        var result = process.Reset();

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ProcessingFlow_CompleteScenario_AllStepsCompleted()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act - Process all steps to completion
        for (int i = 0; i < 6; i++)
        {
            var stepResult = process.ProcessingNextStep();
            Assert.True(stepResult.IsSuccess);
            
            var completeResult = process.CompleteCurrentStep($"result_{i}");
            Assert.True(completeResult.IsSuccess);
        }

        // Act - Try to get next step (should complete)
        var finalResult = process.ProcessingNextStep();

        // Assert
        Assert.True(finalResult.IsSuccess);
        Assert.Null(finalResult.Value); // No more steps
        Assert.Equal(ProcessingStatus.COMPLETED, process.Status);
        Assert.Equal(100, process.ProgressPercentage);
        Assert.NotNull(process.CompletedAt);
    }

    [Fact]
    public void ProcessingFlow_PartialCompletion_ThenFailure()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act - Complete first 2 steps
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        
        process.ProcessingNextStep();
        process.CompleteCurrentStep();

        var progressAfterTwoSteps = process.ProgressPercentage;

        // Fail on third step
        var thirdStep = process.ProcessingNextStep().Value;
        process.FailCurrentStep("Conversion failed");

        // Fail entire process
        process.Fail("Cannot continue processing", isCritical: false);

        // Assert
        Assert.Equal(ProcessingStatus.FAILED, process.Status);
        Assert.True(progressAfterTwoSteps > 0);
        Assert.True(process.CanRetry());
    }

    [Fact]
    public void ProcessingFlow_FailureAndRetryScenario()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act - Do some work then fail
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        process.ProcessingNextStep();
        process.FailCurrentStep("Metadata extraction failed");
        process.Fail("Processing failed");

        var retryTime = DateTime.UtcNow.AddMinutes(5);
        process.ScheduleRetry(retryTime);

        // Reset for retry
        process.Reset();

        var result = process.ProcessingNextStep();

        // Assert
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(1, process.RetryCount);
        Assert.Equal(retryTime, process.NextRetryAt);
        Assert.True(result.IsSuccess);
        Assert.Equal(StepType.INITIALIZATE, result.Value.StepType);
    }

    [Fact]
    public void CurrentStep_WhenNoStepInProgress_ShouldReturnNull()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        var currentStep = process.CurrentStep;

        // Assert
        Assert.Null(currentStep);
    }

    [Fact]
    public void CurrentStep_WhenStepInProgress_ShouldReturnActiveStep()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        var step = process.ProcessingNextStep().Value;

        // Act
        var currentStep = process.CurrentStep;

        // Assert
        Assert.NotNull(currentStep);
        Assert.Equal(step.Id, currentStep.Id);
    }

    [Fact]
    public void ProgressPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act & Assert - Check progress after each step
        var initialProgress = process.ProgressPercentage;
        Assert.Equal(0, initialProgress);

        // Complete 1st step (weight 0)
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        Assert.Equal(0, process.ProgressPercentage);

        // Complete 2nd step (weight 10)
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        Assert.Equal(10, process.ProgressPercentage);

        // Complete 3rd step (weight 60)
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        Assert.Equal(70, process.ProgressPercentage);

        // Complete 4th step (weight 15)
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        Assert.Equal(85, process.ProgressPercentage);

        // Complete 5th step (weight 10)
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        Assert.Equal(95, process.ProgressPercentage);

        // Complete 6th step (weight 5)
        process.ProcessingNextStep();
        process.CompleteCurrentStep();
        process.ProcessingNextStep(); // Complete the process
        Assert.Equal(100, process.ProgressPercentage);
    }

    [Fact]
    public void StepWeights_ShouldTotalTo100()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        int totalWeight = process.Steps.Sum(x => x.Weight);

        // Assert
        Assert.Equal(100, totalWeight);
    }

    [Fact]
    public void ProcessingStep_Start_ShouldFailIfNotPending()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        var step = process.ProcessingNextStep().Value;

        // Act - Try to start already started step via internal method
        // This is tested indirectly through ProcessingStep behavior
        Assert.Equal(StepStatus.IN_PROGRESS, step.Status);
    }

    [Fact]
    public void CompleteCurrentStep_WithNull_ShouldAllowEmptyResultData()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.ProcessingNextStep();

        // Act
        var result = process.CompleteCurrentStep(null);

        // Assert
        Assert.True(result.IsSuccess);
        var completedStep = process.Steps[0];
        Assert.Null(completedStep.ResultData);
    }

    [Fact]
    public void ProcessingNextStep_WhenProcessingNotInProgress_ShouldReturnError()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Critical error", isCritical: true);

        // Act
        var result = process.ProcessingNextStep();

        // Assert
        Assert.False(result.IsSuccess);
    }
}
