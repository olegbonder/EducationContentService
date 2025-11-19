using Core.Validation;
using CSharpFunctionalExtensions;
using EducationContentService.Contracts;
using EducationContentService.Core.Database;
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
    public sealed class UpdateLessonInfoValidator : AbstractValidator<UpdateLessonInfoRequest>
    {
        public UpdateLessonInfoValidator()
        {
            RuleFor(l => l.Title)
                .MustBeValueObject(Title.Create);

            RuleFor(l => l.Description)
                .MustBeValueObject(Description.Create);
        }
    }

    public sealed class UpdateInfoEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPatch("/lessons/{lessonId:guid}",
                async Task<EndpointResult<Guid>> (
                    [FromRoute] Guid lessonId,
                    [FromBody] UpdateLessonInfoRequest request,
                    [FromServices] UpdateInfoHanlder handler,
                    CancellationToken cancellationToken) =>
                        await handler.Handle(lessonId, request, cancellationToken)
            );
        }
    }

    public sealed class UpdateInfoHanlder
    {
        private readonly ILogger<UpdateInfoHanlder> _logger;
        private readonly ITransactionManager _transactionManager;
        private readonly ILessonsRepository _lessonsRepository;
        private readonly IValidator<UpdateLessonInfoRequest> _validator;

        public UpdateInfoHanlder(
            ILogger<UpdateInfoHanlder> logger,
            ITransactionManager transactionManager,
            ILessonsRepository lessonsRepository,
            IValidator<UpdateLessonInfoRequest> validator)
        {
            _logger = logger;
            _transactionManager = transactionManager;
            _lessonsRepository = lessonsRepository;
            _validator = validator;
        }

        public async Task<Result<Guid, Error>> Handle(
            Guid lessonId,
            UpdateLessonInfoRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid == false)
            {
                return validationResult.ToError();
            }
            var title = Title.Create(request.Title).Value;

            var description = Description.Create(request.Description).Value;

            var result = await _lessonsRepository.GetBy(l => l.Id == lessonId, cancellationToken);
            if (result.IsFailure)
            {
                return result.Error;
            }

            var lesson = result.Value;

            lesson.UpdateInfo(title, description);

            await _transactionManager.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created lesson {Id}", lesson.Id);

            return lesson.Id;

        }
    }
}
