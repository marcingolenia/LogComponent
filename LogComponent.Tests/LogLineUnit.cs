using System;
using System.ComponentModel;
using LogTest;
using Xunit;

namespace LogComponent.Tests
{
    [Category("Unit")]
    public class LogLineUnit
    {
        [Fact]
        public void LogLine_Text_Gets_String_InCorrectFormat()
        {
            var cut = new LogLine("Log", DateTime.Today);
            Assert.Equal(cut.LogText, $"{DateTime.Today:yyyy-MM-dd HH:mm:ss:fff}\tLog.\t{Environment.NewLine}");
        }
    }
}
