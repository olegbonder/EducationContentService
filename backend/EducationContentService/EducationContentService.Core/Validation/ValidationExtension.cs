using EducationContentService.Domain.Shared;
using FluentValidation.Results;
using System.Text.Json;

namespace EducationContentService.Core.Validation
{
    public static class ValidationExtension
    {
        public static Error ToError(this ValidationResult validationResult)
        {
            var validationErrors = validationResult.Errors;

            var errors = from validationError in validationErrors
                         let errorMessage = validationError.ErrorMessage
                         let error = JsonSerializer.Deserialize<Error>(errorMessage)
                         select error.Messages;

            return Error.Validation(errors.SelectMany(e => e));

        }
    }
}
