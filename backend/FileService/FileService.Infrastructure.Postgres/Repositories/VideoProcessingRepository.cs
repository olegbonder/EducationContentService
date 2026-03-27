using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Infrastructure.Postgres.Repositories
{
    public class VideoProcessingRepository : IVideoProcessingRepository
    {
        private readonly FileServiceDbContext _context;
        private readonly ILogger<VideoProcessingRepository> _logger;

        public VideoProcessingRepository(
            FileServiceDbContext context,
            ILogger<VideoProcessingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add(VideoProcess videoProcess)
        {            
            _context.VideoProcesses.Add(videoProcess);
        }

        public async Task<Result<VideoProcess, Error>> GetBy(
            Expression<Func<VideoProcess, bool>> predicate, 
            CancellationToken cancellationToken)
        {
            var videoProcess = await _context.VideoProcesses.FirstOrDefaultAsync(predicate, cancellationToken);
            if (videoProcess == null)
                return GeneralErrors.NotFound(null, "video_process");

            return videoProcess;
        }
    }
}