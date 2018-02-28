namespace LogTest
{
    using System;
    using System.Threading.Tasks;

    public interface IAsyncLogger: IDisposable
    {
        void StopWithoutFlush();
        Task StopWithFlush();
        void Write(string text);
    }
}