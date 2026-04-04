using CSharpFunctionalExtensions;
using EducationContentService.Core.Database;
using EducationContentService.Domain.Shared;
using FileService.Contracts;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace EducationContentService.Core.Features.Lessons
{
    public sealed class AttachVideoEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPatch("/lessons/{lessonId:guid}/video", 
                async Task<EndpointResult<Guid>>(
                        [FromRoute] Guid lessonId,
                        [FromBody] AttachVideoRequest request, 
                        [FromServices] AttachVideoHanlder handler, 
                        CancellationToken cancellationToken) =>
                    await handler.Handle(lessonId, request, cancellationToken)
            );
        }
    }

    public record AttachVideoRequest(Guid? VideoId);

    public sealed class AttachVideoHanlder
    {
        private readonly ILogger<AttachVideoHanlder> _logger;
        private readonly ILessonsRepository _lessonsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IFileCommunicationService _fileCommunicationService;

        public AttachVideoHanlder(
            ILogger<AttachVideoHanlder> logger, 
            ILessonsRepository lessonsRepository,
            ITransactionManager transactionManager,
            IFileCommunicationService fileCommunicationService)
        {
            _logger = logger;
            _lessonsRepository = lessonsRepository;
            _transactionManager = transactionManager;
            _fileCommunicationService = fileCommunicationService;
        }

        public async Task<Result<Guid, Error>> Handle(
            Guid lessonId,
            AttachVideoRequest request, 
            CancellationToken cancellationToken)
        {
            Guid? videoId = request.VideoId;
            if (videoId.HasValue)
            {
                var existsResult = 
                    await _fileCommunicationService.CheckMediaAssetExists(videoId.Value, cancellationToken);
                if (existsResult.IsFailure)
                    return existsResult.Error;

                if (!existsResult.Value.Exists)
                    return EducationErrors.VideoAssetNotFound(videoId.Value);
            }
            var lessonResult = await _lessonsRepository.GetBy(l => l.Id == lessonId, cancellationToken);
            if (lessonResult.IsFailure)
                return lessonResult.Error;
            
            lessonResult.Value.UpdateVideoId(videoId);

            var result = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (result.IsFailure)
                return result.Error;

            _logger.LogInformation("Updated video for lesson {Id}", lessonResult.Value.Id);
            
            return lessonResult.Value.Id;
        }
    }
}