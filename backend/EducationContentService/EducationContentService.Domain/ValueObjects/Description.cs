using CSharpFunctionalExtensions;
using Shared.SharedKernel;
using System.Text.RegularExpressions;

namespace EducationContentService.Domain.ValueObjects
{
    public record Description
    {
        public const int MAX_LENGTH = 2000;
        public string Value { get; }

        public Description(string value)
        {
            Value = value;
        }

        public static Result<Description, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return GeneralErrors.ValueIsInvalid("описание");
            }

            var normalized = Regex.Replace(value.Trim(), @"\s+", " ");

            if (normalized.Length > MAX_LENGTH)
            {
                return GeneralErrors.ValueIsInvalid("title");
            }

            return new Description(value);
        }
    }
}
