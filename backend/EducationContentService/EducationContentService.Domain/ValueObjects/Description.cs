using CSharpFunctionalExtensions;

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

        public static Result<Description, string> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > MAX_LENGTH)
            {
                return "Описание пустое или слишком длинное";
            }

            return new Description(value);
        }
    }
}
