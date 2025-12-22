namespace Shared.SharedKernel.Exceptions
{
    public class FailureException : Exception
    {
        public Error Error { get; } = null!;

        public FailureException(Error error)
            : base(error.GetMessage())
        {
            
        }
        public FailureException(string message) 
            : base(message)
        {
        }

        public FailureException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
        public FailureException()
        {
        }
    }
}
