using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LogTest;
using Xunit;
using Moq;

namespace LogComponent.Tests
{
    [Category("Integration")]
    public class AsyncLoggerIntegrationTests
    {
        [Fact]
        public async Task AsyncLogger_WritesLogsLines_To_File()
        {
            // arrange
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            using (var cut = new AsyncLogger(new List<ILogWriter> { new ToFileWriter(logsPath, "testLog") }, new LoggerDateProvider()))
            {
                // act
                cut.Write("random log 1");
                cut.Write("random log 2");
                await Task.Delay(50); // some time to get the job done by the other thread.
            }

            await Task.Delay(100);
            // assert
            Assert.Equal(2, File.ReadAllLines(Directory.GetFiles(logsPath)[0]).Length);
            Directory.Delete(logsPath, true);
        }

        [Theory]
        [InlineData(1500)]
        [InlineData(150)]
        [InlineData(50)]
        public async Task StopWithoutFlush_Rejects_NotSavedLogs(int logsCount)
        {
            // arrange
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            using (var cut = new AsyncLogger(new List<ILogWriter> { new ToFileWriter(logsPath, "testLog") },
                new LoggerDateProvider()))
            {
                var logTasks = Enumerable.Range(1, logsCount)
                    .Select(i => Task.Run(() => { cut.Write($"LogMessage number {i}"); }));
                await Task.WhenAll(logTasks);
                // act
                cut.StopWithoutFlush();
            }
            await Task.Delay(100);
            // assert (Two things can happen - file did not make it to emerge yet or it did but without writing all logs.
            if (File.Exists(Path.Combine(logsPath, $"testLog_{DateTime.Today:yyyyMMdd}.log")))
            {
                Assert.NotEqual(logsCount, File.ReadAllLines(Directory.GetFiles(logsPath)[0]).Length);
            }
            Directory.Delete(logsPath, true);
        }

        [Theory]
        [InlineData(150)]
        [InlineData(50)]
        public async Task StopWithoutFlush_When_Logs_Are_AlreadySaved_SimplyDoesNothing(int logsCount)
        {
            // arrange
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            using (var cut = new AsyncLogger(new List<ILogWriter> {new ToFileWriter(logsPath, "testLog")},
                new LoggerDateProvider()))
            {
                var logTasks = Enumerable.Range(1, logsCount)
                    .Select(i => Task.Run(() => { cut.Write($"LogMessage number {i}"); }));
                await Task.WhenAll(logTasks);
                await Task.Delay(2000); // Give some time so we are sure all logs will be written;
                // act 
                cut.StopWithoutFlush();
            }
            await Task.Delay(100);
            // assert
            Assert.Equal(logsCount, File.ReadAllLines(Directory.GetFiles(logsPath)[0]).Length);
            Directory.Delete(logsPath, true);
        }

        [Theory]
        [InlineData(150)]
        [InlineData(50)]
        public async Task StopWithFlush_Saves_PendingsLogs_AndStops(int logsCount)
        {
            // arrange
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            using (var cut = new AsyncLogger(new List<ILogWriter> { new ToFileWriter(logsPath, "testLog") },
                new LoggerDateProvider()))
            {
                var logTasks = Enumerable.Range(1, logsCount)
                    .Select(i => Task.Run(() => { cut.Write($"LogMessage number {i}"); }));
                await Task.WhenAll(logTasks);
                // act
                await cut.StopWithFlush();
            }
            var lines = File.ReadAllLines(Directory.GetFiles(logsPath)[0]).Length;
            Assert.Equal(logsCount, lines);
            Directory.Delete(logsPath, true);
        }

        [Fact]
        public async Task AsyncLogger_CreatesNewLogFile_AfterMidnight()
        {
            // arrange
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            using (var beforeMidnightCut = new AsyncLogger(new List<ILogWriter> { new ToFileWriter(logsPath, "testLog") },
                new LoggerDateProvider()))
            {
                beforeMidnightCut.Write("BeforeMidnight");
                await beforeMidnightCut.StopWithFlush();
            }
            var loggerDate = Mock.Of<ILoggerDateProvider>(m => m.Now == DateTime.Now.Date.AddDays(1).AddSeconds(1).Date);
            using (var afterMidnightCut =
                new AsyncLogger(new List<ILogWriter> { new ToFileWriter(logsPath, "testLog") }, loggerDate))
            {
                // act
                afterMidnightCut.Write("AfterMidnight");
                await afterMidnightCut.StopWithFlush();
            }
            await Task.Delay(100);
            // assert
            Assert.Equal(2, Directory.GetFiles(logsPath).Length);
            Directory.Delete(logsPath, true);
        }

        [Fact]
        public async Task AsyncLogger_UsesSameLogFile_JustBeforeMidnight()
        {
            // arrange
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            using (var beforeMidnightCut = new AsyncLogger(new List<ILogWriter> { new ToFileWriter(logsPath, "testLog") },
                new LoggerDateProvider()))
            {
                beforeMidnightCut.Write("BeforeMidnight");
                await beforeMidnightCut.StopWithFlush();
            }
            var loggerDate = Mock.Of<ILoggerDateProvider>(m => m.Now == DateTime.Now.Date.AddSeconds(-1).AddDays(1).Date);
            using (var afterMidnightCut =
                new AsyncLogger(new List<ILogWriter> { new ToFileWriter(logsPath, "testLog") }, loggerDate))
            {
                // act
                afterMidnightCut.Write("AfterMidnight");
                await afterMidnightCut.StopWithFlush();
            }
            await Task.Delay(100);
            // assert
            Assert.Single(Directory.GetFiles(logsPath));
            Directory.Delete(logsPath, true);
        }
    }
}
