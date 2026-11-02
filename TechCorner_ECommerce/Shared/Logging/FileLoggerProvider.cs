using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace TechCorner_ECommerce.Logging {
    public class FileLoggerProvider : ILoggerProvider {
        private readonly FileLoggerOptions options;
        private readonly ConcurrentDictionary<string, FileLogger> loggers = new();
        private readonly object writeLock = new();

        public FileLoggerProvider(IOptions<FileLoggerOptions> options) {
            this.options = options.Value;
        }

        public ILogger CreateLogger(string categoryName) {
            return loggers.GetOrAdd(categoryName, name => new FileLogger(name, options, writeLock));
        }

        public void Dispose() {
            loggers.Clear();
        }
    }
}
