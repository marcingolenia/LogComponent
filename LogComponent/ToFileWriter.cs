namespace LogTest
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    public class ToFileWriter : ILogWriter
    {
        private readonly string _logfileNamePrefix;
        private readonly string _logFilesPath;

        public ToFileWriter(string logsFilesPath, string logFileNamePrefix)
        {
            _logFilesPath = logsFilesPath;
            _logfileNamePrefix = logFileNamePrefix;
            Directory.CreateDirectory(logsFilesPath);
        }

        public async Task WriteLog(LogLine logLine)
        {
            using (var stream = File.Open(
                Path.Combine(_logFilesPath, $"{_logfileNamePrefix}_{logLine.LogTimestamp:yyyyMMdd}.log"), FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite))
            {

                var encodedText = Encoding.UTF8.GetBytes(logLine.LogText);
                await stream.WriteAsync(encodedText, 0, encodedText.Length);
            }
        }
    }
}