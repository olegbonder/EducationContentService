using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.SharedKernel;

namespace FileService.VideoProcessing.FfmpegProcess
{
    public interface IFfmpegProcessRunner
    {
        Task<Result<VideoMetaData, Error>> ExtractMetadataAsync(
            string inputFileUrl,
            CancellationToken cancellationToken);

        Task<UnitResult<Error>> GenerateHlsAsync(
            string inputFileUrl,
            string outputDirectory,
            CancellationToken cancellationToken);
    }
}