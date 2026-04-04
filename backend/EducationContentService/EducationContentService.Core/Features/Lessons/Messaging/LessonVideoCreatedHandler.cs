using EducationContentService.Core.Database;
using IntegrationEvents.Files.Events;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Core.Features.Lessons.Messaging
{
    public class LessonVideoCreatedHandler
    {
        private readonly ILessonsRepository _lessonsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<LessonVideoCreatedHandler> _logger;

        public LessonVideoCreatedHandler(
            ILessonsRepository lessonsRepository,
            ITransactionManager transactionManager,
            ILogger<LessonVideoCreatedHandler> logger)
        {
            _lessonsRepository = lessonsRepository;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public async Task Hadle(VideoCreated message, CancellationToken cancellationToken)
        {
            if (!message.EntityType.Equals("lesson", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            
            var lessonResult = await _lessonsRepository.GetBy(
                l => l.Id ==  message.EntityId,
                cancellationToken);

            if (lessonResult.IsFailure)
            {
                _logger.LogWarning(
                    "Lesson {LessonId} was not found for VideoCreated event. VideoId={VideoId}",
                    message.EntityId,
                    message.VideoId);
                return;
            }
            
            lessonResult.Value.UpdateVideoId(message.VideoId);
            
            await _transactionManager.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Attached video {VideoId} to lesson {LessonId}",
                message.VideoId,
                message.EntityId);
        }
    }
}