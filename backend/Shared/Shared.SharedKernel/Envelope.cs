namespace Shared.SharedKernel
{
    public record Envelope
    {
        public object? Result { get; }
        public Error? Error { get; }
        public bool IsError => Error != null;
        public DateTime TimeGenerated { get; }

        private Envelope(object? result, Error? error)
        {
            Result = result;
            Error = error;
            TimeGenerated = DateTime.UtcNow;
        }

        public static Envelope Ok(object? result = null) =>
            new(result, null);

        public static Envelope Fail(Error error) =>
            new(null, error);
    }

    public record Envelope<T>
    {
        public T? Result { get; }
        public Error? Error { get; }
        public bool IsError => Error != null;
        public DateTime TimeGenerated { get; }

        private Envelope(T? result, Error? error)
        {
            Result = result;
            Error = error;
            TimeGenerated = DateTime.Now;
        }

        public static Envelope<T> Ok(T? result) =>
            new(result, null);

        public static Envelope<T> Fail(Error error) =>
            new(default, error);
    }
}
