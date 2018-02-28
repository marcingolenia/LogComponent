namespace LogTest
{
    using System;
    public class LogLine
    {
        private readonly string _text;
        public DateTime LogTimestamp { get; }

        public LogLine(string text, DateTime logTimestamp)
        {
            _text = text;
            LogTimestamp = logTimestamp;
        }

        public virtual string LogText =>
            $"{LogTimestamp:yyyy-MM-dd HH:mm:ss:fff}\t{_text}.\t{Environment.NewLine}";
    }
}