using Microsoft.Extensions.Logging;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

[DisallowConcurrentExecution]
public class VideoProcessingJob : IJob
{
    private readonly ILogger<VideoProcessingJob> _logger;
    private readonly IVideoProcessingService _videoProcessingService;
    public static readonly JobKey VideoAssetIdKey = new("VideoAssetId");

    public VideoProcessingJob(
        ILogger<VideoProcessingJob> logger,
        IVideoProcessingService videoProcessingService)
    {
        _logger = logger;
        _videoProcessingService = videoProcessingService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.MergedJobDataMap;
        var videoAssetId = dataMap.GetGuid(VideoAssetIdKey.Name);
        _logger.LogInformation("Starting Video processing job for VideoAssetId: {VideoAssetId}", videoAssetId);
        var result = await _videoProcessingService.ProcessVideoAsync(videoAssetId);
        if (result.IsFailure)
        {
            _logger.LogError(
                "Video processing job failed for {VideoAssetId} with error: {ErrorMessage}",
                videoAssetId,
                result.Error);

            throw new JobExecutionException(refireImmediately: false);
        }
    }
}