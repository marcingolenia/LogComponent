namespace LogTest
{
    using System;
    public class LoggerDateProvider : ILoggerDateProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
