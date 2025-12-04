namespace MonitorLibrary.Reactive
{
    public class LogEvent
    {
        public LogLevel Level { get; }
        public string Message { get; }
        public DateTimeOffset Timestamp { get; }
        public Exception Exception { get; }

        public LogEvent(LogLevel level, string message, Exception exception = null)
        {
            Level = level;
            Message = message;
            Timestamp = DateTimeOffset.Now;
            Exception = exception;
        }

        public override string ToString() =>
            $"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{(Exception != null ? $"\n  Exception: {Exception}" : "")}";
    }

    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
    }
}
