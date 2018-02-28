using System;
using System.Collections.Concurrent;
namespace LogTest
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncLogger : IAsyncLogger
    {
        private readonly CancellationTokenSource _cancelToken;
        private readonly ILoggerDateProvider _loggerDateProvider;
        private readonly BlockingCollection<LogLine> _logLines = new BlockingCollection<LogLine>();
        private readonly IReadOnlyList<ILogWriter> _logWriters;

        public AsyncLogger(IReadOnlyList<ILogWriter> logWriters, ILoggerDateProvider loggerDateProvider)
        {
            _loggerDateProvider = loggerDateProvider;
            _logWriters = logWriters;
            _cancelToken = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cancelToken.IsCancellationRequested)
                {
                    var log = _logLines.Take(_cancelToken.Token);
                    await HandleByAllWriters(log);
                }
            }, _cancelToken.Token);
        }

        public void StopWithoutFlush()
        {
            _cancelToken.Cancel();
        }

        public async Task StopWithFlush()
        {
            _cancelToken.Cancel();
            while (_logLines.TryTake(out LogLine log))
            {
                await HandleByAllWriters(log);
            }
        }

        public void Write(string logText)
        {
            _logLines.Add(new LogLine(logText, _loggerDateProvider.Now));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancelToken?.Dispose();
                _logLines?.Dispose();
            }
        }

        private async Task HandleByAllWriters(LogLine log)
        {
            var logTask = _logWriters.Select(lw => lw.WriteLog(log));
            try
            {
                await Task.WhenAll(logTask);
            }
            catch (Exception)
            {
                // Silently swallow. In rare cases it's more important to not log the text than breaking the job. Consider retry-logic.
            }
        }
    }
}