using System;

namespace Emerald.Logger
{
    public class LogMessage
    {
        private LogMessage(LogType logType, string message, string sourceName, Exception exception = null)
        {
            LogType = logType;
            Message = message;
            Source = sourceName;
            Timestamp = DateTime.Now;
            Exception = exception;
        }

        private LogMessage(LogType logType, string message) : this(logType, message, string.Empty)
        {
        }

        public LogType LogType { get; }
        public string Message { get; }
        public string Source { get; }
        public DateTime Timestamp { get; }
        public Exception Exception { get; }

        public static LogMessage CreateSimple(LogType logType, string message)
        {
            return new LogMessage(logType, message);
        }

        public static LogMessage CreateSimple(LogType logType, string message, string sourceName)
        {
            return new LogMessage(logType, message, sourceName);
        }

        public static LogMessage CreateError(string message, Exception exception)
        {
            return new LogMessage(LogType.Error, message, exception.Source, exception);
        }

        public override string ToString()
        {
            if (Exception != null && !string.IsNullOrEmpty(Source))
                return $"[{Timestamp:T}][{LogType}] {Exception.GetType().Name} thrown from {Source}";

            return string.IsNullOrEmpty(Source)
                ? $"[{Timestamp:T}][{LogType}] {Message}"
                : $"[{Timestamp:T}][{LogType}] <{Source}> {Message}";
        }
    }
}