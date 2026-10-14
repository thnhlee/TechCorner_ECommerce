namespace TechCorner_ECommerce.Logging {
    public class FileLoggerOptions {
        public bool Enabled { get; set; }
        public string DirectoryPath { get; set; } = "Logs";
        public string FileNamePrefix { get; set; } = "techcorner";
        public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;
    }
}
