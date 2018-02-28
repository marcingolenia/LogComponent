namespace LogUsers
{
    using System;
    using System.Threading.Tasks;
    using LogTest;

    class Program
    {
        static void Main(string[] args)
        {
            RunAppAsync().Wait();
        }

        private static async Task RunAppAsync()
        {
            IAsyncLogger logger = new AsyncLogger(new ILogWriter[] { new ToFileWriter("C:\\Logs", "logs") }, new LoggerDateProvider());

            for (int i = 0; i < 15; i++)
            {
                logger.Write("Number with Flush: " + i.ToString());
            }

            await logger.StopWithFlush();
            IAsyncLogger logger2 = new AsyncLogger(new ILogWriter[] { new ToFileWriter("C:\\Logs", "logs") }, new LoggerDateProvider());

            for (int i = 50; i > 0; i--)
            {
                logger2.Write("Number with No flush: " + i.ToString());
            }

            logger2.StopWithoutFlush();

            Console.ReadKey();
        }
    }
}
