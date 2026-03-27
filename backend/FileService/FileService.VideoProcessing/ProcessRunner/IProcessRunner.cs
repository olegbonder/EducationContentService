using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.VideoProcessing.ProcessRunner
{
    public interface IProcessRunner
    {
        Task<Result<ProcessResult, Error>> RunAsync(
            ProcessCommand command,
            Action<string>? onOutput = null,
            CancellationToken cancellationToken = default);
    }
}