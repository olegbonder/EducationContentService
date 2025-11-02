using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationContentService.Domain.Shared
{
    public sealed record Error
    {
        public string Code { get; }

        public string Message { get; }

        public ErrorType ErrorType { get; }
        public string? InvalidField { get; }

        private Error(string code, string message, ErrorType errorType, string? invalidField)
        {
            Code = code;
            Message = message;
            ErrorType = errorType;
            InvalidField = invalidField;
        }

        public static Error Validation(string code, string message, string invalidField) =>
            new Error(code, message, ErrorType.VALIDATION, invalidField);
    }

    public enum ErrorType
    {
        VALIDATION,
        NOT_FOUND,
        FAILURE,
        AUTHENTIFICATION,
        AUTHORIZATION,
    }
}
