using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using LogTest;
using Moq;
using Xunit;

namespace LogComponent.Tests
{
    [Category("Unit")]
    public class AsyncLoggerUnitTests
    {
        [Fact]
        public void AsyncLogger_AfterDispose_CannotCall_StopWithoutFlush()
        {
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            var cut = new AsyncLogger(new[] { new ToFileWriter(logsPath, "testLog") }, new LoggerDateProvider());
            cut.Dispose();
            Assert.Throws<ObjectDisposedException>(() => cut.StopWithoutFlush());
        }

        [Fact]
        public async Task AsyncLogger_AfterDispose_CannotCall_StopWithFlush()
        {
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            var cut = new AsyncLogger(new[] { new ToFileWriter(logsPath, "testLog") }, new LoggerDateProvider());
            cut.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await cut.StopWithFlush());
        }

        [Fact]
        public void AsyncLogger_AfterDispose_Cannot_WriteLogs()
        {
            var logsPath = Path.Combine(Path.GetTempPath(), $"IntegrationTests-{Guid.NewGuid()}");
            var cut = new AsyncLogger(new[] { new ToFileWriter(logsPath, "testLog") }, new LoggerDateProvider());
            cut.Dispose();
            Assert.Throws<ObjectDisposedException>(() => cut.Write("I like kittens"));
        }

        [Fact]
        public async Task AsyncLogger_Silently_Swallows_Exception_AndConitnous_When_LogWriterThrowsException()
        {
            // arrange
            var logWriterMock = new Mock<ILogWriter>();
            logWriterMock.Setup(mock => mock.WriteLog(It.IsAny<LogLine>())).Throws(new Exception("I am a random Error! Say Hi :)"));
            var cut = new AsyncLogger(new[] { logWriterMock.Object }, new LoggerDateProvider());
            // act & assert
            cut.Write("Nothing happens...");
            await cut.StopWithFlush();
        }
    }
}
