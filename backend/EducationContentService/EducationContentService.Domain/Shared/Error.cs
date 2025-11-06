using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationContentService.Domain.Shared
{
    public record ErrorMessage(string Code, string Message, string? InvalidField);
    public sealed record Error
    {
        public IReadOnlyList<ErrorMessage> Messages { get; } = [];

        public ErrorType ErrorType { get; }

        private Error(IEnumerable<ErrorMessage> messages, ErrorType errorType)
        {
            Messages = messages.ToArray();
            ErrorType = errorType;
        }

        public static Error Validation(params IEnumerable<ErrorMessage> messages) =>
            new Error(messages, ErrorType.VALIDATION);

        public static Error NotFound(params IEnumerable<ErrorMessage> messages) =>
            new Error(messages, ErrorType.NOT_FOUND);

        public static Error Failure(params IEnumerable<ErrorMessage> messages) =>
            new Error(messages, ErrorType.FAILURE);

        public static Error Conflict(params IEnumerable<ErrorMessage> messages) =>
            new Error(messages, ErrorType.CONFLICT);

        public static Error Authentification(params IEnumerable<ErrorMessage> messages) =>
            new Error(messages, ErrorType.AUTHENTIFICATION);

        public static Error Authorization(params IEnumerable<ErrorMessage> messages) =>
            new Error(messages, ErrorType.AUTHORIZATION);            
    }

    public enum ErrorType
    {
        VALIDATION,
        NOT_FOUND,
        FAILURE,
        CONFLICT,
        AUTHENTIFICATION,
        AUTHORIZATION,
    }
}
