using System.Text.Json.Serialization;

namespace Shared.SharedKernel
{
    public record ErrorMessage(string Code, string Message, string? InvalidField);
    public sealed record Error
    {
        public IReadOnlyList<ErrorMessage> Messages { get; } = [];

        public ErrorType ErrorType { get; }

        [JsonConstructor]
        private Error(IReadOnlyList<ErrorMessage> messages, ErrorType errorType)
        {
            Messages = messages.ToArray();
            ErrorType = errorType;
        }

        private Error(IEnumerable<ErrorMessage> messages, ErrorType errorType)
        {
            Messages = messages.ToArray();
            ErrorType = errorType;
        }

        public string GetMessage() => string.Join(";", Messages.Select(m => m.ToString()));

        public static Error Validation(string code, string message, string? invalidField = null) =>
            new Error([new ErrorMessage(code, message, invalidField)], ErrorType.VALIDATION);

        public static Error NotFound(string code, string message) =>
            new Error([new ErrorMessage(code, message, null)], ErrorType.NOT_FOUND);

        public static Error Failure(string code, string message) =>
            new Error([new ErrorMessage(code, message, null)], ErrorType.FAILURE);

        public static Error Conflict(string code, string message) =>
            new Error([new ErrorMessage(code, message, null)], ErrorType.CONFLICT);

        public static Error Authentification(string code, string message) =>
            new Error([new ErrorMessage(code, message, null)], ErrorType.AUTHENTIFICATION);

        public static Error Authorization(string code, string message) =>
            new Error([new ErrorMessage(code, message, null)], ErrorType.AUTHORIZATION);

        public static Error Validation(params IEnumerable<ErrorMessage> errors) =>
            new Error(errors, ErrorType.VALIDATION);

        public static Error NotFound(params IEnumerable<ErrorMessage> errors) =>
            new Error(errors, ErrorType.NOT_FOUND);

        public static Error Failure(params IEnumerable<ErrorMessage> errors) =>
            new Error(errors, ErrorType.FAILURE);

        public static Error Conflict(params IEnumerable<ErrorMessage> errors) =>
            new Error(errors, ErrorType.CONFLICT);

        public static Error Authentification(params IEnumerable<ErrorMessage> errors) =>
            new Error(errors, ErrorType.AUTHENTIFICATION);

        public static Error Authorization(params IEnumerable<ErrorMessage> errors) =>
            new Error(errors, ErrorType.AUTHORIZATION);
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
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
