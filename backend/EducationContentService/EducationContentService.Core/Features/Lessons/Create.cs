using Core.Validation;
using CSharpFunctionalExtensions;
using EducationContentService.Contracts;
using EducationContentService.Domain.Lesson;
using EducationContentService.Domain.ValueObjects;
using FluentValidation;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace EducationContentService.Core.Features.Lessons
{
    public sealed class CreateLessonRequestValidator : AbstractValidator<CreateLessonRequest>
    {
        public CreateLessonRequestValidator()
        {
            RuleFor(l => l.Title)
                .MustBeValueObject(Title.Create);

            RuleFor(l => l.Description)
                .MustBeValueObject(Description.Create);
        }
    }

    public sealed class CreateEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("/lessons", 
                async Task<EndpointResult<Guid>>([FromBody] CreateLessonRequest request, 
                [FromServices] CreateHanlder handler, 
                CancellationToken cancellationToken) =>
                    await handler.Handle(request, cancellationToken)
            );
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

            var result = await _lessonsRepository.AddAsync(lesson, cancellationToken);
            if (result.IsFailure)
            {
                return result.Error;
            }

            _logger.LogInformation("Created lesson {Id}", lesson.Id);
            
            return lesson.Id;
        }
    }
}
