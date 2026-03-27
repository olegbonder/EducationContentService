using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IVideoProcessingRepository
{
    void Add(VideoProcess videoProcess);
    
    Task<Result<VideoProcess, Error>> GetBy(Expression<Func<VideoProcess, bool>> predicate, CancellationToken cancellationToken);
}