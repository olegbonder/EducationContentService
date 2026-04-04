using EducationContentService.Core.Database;
using IntegrationEvents.Files.Events;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Core.Features.Lessons.Messaging
{
    public class LessonVideoCreatedHandler
    {
        public static async Task Handle(
            VideoCreated message, 
            ILessonsRepository lessonsRepository,
            ITransactionManager transactionManager,
            ILogger<LessonVideoCreatedHandler> logger,
            CancellationToken cancellationToken)
        {
            if (!message.EntityType.Equals("lesson", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var lessonResult = await lessonsRepository.GetBy(
                l => l.Id == message.EntityId,
                cancellationToken);

            if (lessonResult.IsFailure)
            {
                logger.LogWarning(
                    "Lesson {LessonId} was not found for VideoCreated event. VideoId={VideoId}",
                    message.EntityId,
                    message.VideoId);
                return;
            }

            lessonResult.Value.UpdateVideoId(message.VideoId);

            await transactionManager.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Attached video {VideoId} to lesson {LessonId}",
                message.VideoId,
                message.EntityId);
        }
    }
}