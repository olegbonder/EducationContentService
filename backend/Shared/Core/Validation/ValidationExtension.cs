using FluentValidation.Results;
using Shared.SharedKernel;
using System.Text.Json;

namespace Core.Validation
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
