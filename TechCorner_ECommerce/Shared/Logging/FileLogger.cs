using System.Globalization;

namespace TechCorner_ECommerce.Logging {
    public class FileLogger : ILogger {
        private readonly string categoryName;
        private readonly FileLoggerOptions options;
        private readonly object writeLock;

        public FileLogger(string categoryName, FileLoggerOptions options, object writeLock) {
            this.categoryName = categoryName;
            this.options = options;
            this.writeLock = writeLock;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return options.Enabled && logLevel >= options.MinimumLevel;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) {
            if (!IsEnabled(logLevel)) {
                return;
            }

            var message = formatter(state, exception);

            if (string.IsNullOrWhiteSpace(message) && exception == null) {
                return;
            }

            var now = DateTimeOffset.Now;
            var logLine = FormatLogLine(now, logLevel, eventId, message, exception);
            var logPath = GetLogPath(now);

            lock (writeLock) {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, logLine);
            }
        }

        private string GetLogPath(DateTimeOffset now) {
            var fileName = $"{options.FileNamePrefix}-{now:yyyyMMdd}.log";
            return Path.Combine(AppContext.BaseDirectory, options.DirectoryPath, fileName);
        }

        private string FormatLogLine(
            DateTimeOffset now,
            LogLevel logLevel,
            EventId eventId,
            string message,
            Exception? exception) {
            var timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture);
            var logLine = $"{timestamp} [{logLevel}] {categoryName} ({eventId.Id}) {message}{Environment.NewLine}";

            if (exception != null) {
                logLine += exception + Environment.NewLine;
            }

            return logLine;
        }
    }
}
