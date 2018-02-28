namespace LogTest
{
    using System.Threading.Tasks;
    public interface ILogWriter
    {
        Task WriteLog(LogLine logLine);
    }
}
