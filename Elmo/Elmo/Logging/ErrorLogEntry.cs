namespace Elmo.Logging
{
    public class ErrorLogEntry
    {
        public string Id { get; }
        public Error Error { get; }

        public ErrorLogEntry(string id, Error error)
        {
            Id = id;
            Error = error;
        }
    }
}
