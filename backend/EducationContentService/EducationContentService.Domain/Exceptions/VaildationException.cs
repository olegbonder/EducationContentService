using EducationContentService.Domain.Shared;

namespace EducationContentService.Domain.Exceptions
{
    public class VaildationException : Exception
    {
        public Error Error { get; } = null!;

        public VaildationException(Error error)
            : base(error.GetMessage())
        {
            
        }
        public VaildationException(string message) 
            : base(message)
        {
        }

        public VaildationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
        public VaildationException()
        {
        }
    }
}
