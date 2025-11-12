using CSharpFunctionalExtensions;
using EducationContentService.Core.Endpoints;
using EducationContentService.Core.Validation;
using EducationContentService.Domain.Lesson;
using EducationContentService.Domain.Shared;
using EducationContentService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Core.Features.Lessons
{
    public record CreateLessonRequest(string Title, string Description, DateTime StartDate);
    public class CreateEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("api/lessons", 
                async Task<Microsoft.AspNetCore.Http.IResult>([FromBody] CreateLessonRequest request, 
                [FromServices] CreateHanlder handler, 
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(request, cancellationToken);

                return result.IsFailure 
                    ? Results.BadRequest(Envelope.Fail(result.Error))
                    : new SuccessResult() .Ok(Envelope.Ok(request.Title));
            });
        }
    }

    public sealed class CreateHanlder
    {
        private readonly ILogger<CreateHanlder> _logger;
        private readonly ILessonsRepository _lessonsRepository;
        private readonly IValidator<CreateLessonRequest> _validator;

        public CreateHanlder(
            ILogger<CreateHanlder> logger, 
            ILessonsRepository lessonsRepository,
            IValidator<CreateLessonRequest> validator)
        {
            _logger = logger;
            _lessonsRepository = lessonsRepository;
            _validator = validator;
        }

        public async Task<Result<Guid, Error>> Handle(
            CreateLessonRequest request, 
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid == false)
            {
                return validationResult.ToError();
            }
            var title = Title.Create(request.Title).Value;

            var description = Description.Create(request.Description).Value;

            var lesson = new Lesson(Guid.NewGuid(), title, description);

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
