using CSharpFunctionalExtensions;
using EducationContentService.Domain.Lesson;
using EducationContentService.Domain.Shared;
using EducationContentService.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Core.Features.Lessons
{
    public record CreateLessonRequest(string Title, string Description);
    public class CreateEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("api/lessons", async ([FromBody] CreateLessonRequest request, [FromServices] CreateHanlder handler) =>
            {
                await handler.Handle(request);
            });
        }
    }

    public sealed class CreateHanlder
    {
        private readonly ILogger<CreateHanlder> _logger;
        private readonly ILessonsRepository _lessonsRepository;

        public CreateHanlder(ILogger<CreateHanlder> logger, ILessonsRepository lessonsRepository)
        {
            _logger = logger;
            _lessonsRepository = lessonsRepository;
        }

        public async Task<Result<Guid, Error>> Handle(CreateLessonRequest request)
        {
            Result<Title, Error> titleResult = Title.Create(request.Title);
            if (titleResult.IsFailure)
            {
                return titleResult.Error;
            }

            Result<Description, Error> descriptionResult = Description.Create(request.Description);
            if (descriptionResult.IsFailure)
            {
                return descriptionResult.Error;
            }
            var lesson = new Lesson(Guid.NewGuid(), titleResult.Value, descriptionResult.Value);

            var result = await _lessonsRepository.AddAsync(lesson);
            if (result.IsFailure)
            {
                return result.Error;
            }

            _logger.LogInformation("Created lesson {Id}", lesson.Id);
            
            return lesson.Id;
        }
    }
}
