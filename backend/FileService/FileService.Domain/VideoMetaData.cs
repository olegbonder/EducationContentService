using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain
{
    public class VideoMetaData
    {
        public TimeSpan Duration { get; }

        public int Width { get; }

        public int Height { get; }

        // EF Core
        private VideoMetaData()
        {
            
        }

        private VideoMetaData(TimeSpan duration, int width, int height)
        {
            Duration = duration;
            Width = width;
            Height = height;
        }

        public static Result<VideoMetaData, Error> Create(TimeSpan duration, int width, int height) 
        {
            if (duration <= TimeSpan.Zero)
                return GeneralErrors.ValueIsRequired("duration");

            return new VideoMetaData(duration, width, height);
        }
    }
}