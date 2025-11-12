using EducationContentService.Core.Validation;
using EducationContentService.Domain.Shared;
using EducationContentService.Domain.ValueObjects;
using FluentValidation;

namespace EducationContentService.Core.Features.Lessons
{
    public class CreateLessonRequestValidator: AbstractValidator<CreateLessonRequest>
    {
        public CreateLessonRequestValidator()
        {
            RuleFor(l => l.Title)
                .MustBeValueObject(Title.Create);

            RuleFor(l => l.Description)
                .MustBeValueObject(Description.Create);

            RuleFor(l => l.StartDate)
                .NotEmpty().WithError(GeneralErrors.ValueIsInvalid(nameof(CreateLessonRequest.StartDate)));
        }
    }
}
