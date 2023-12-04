using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeControllers.Loggers
{
    public class ConsoleLogger : ILogger, IDisposable
    {
        private ConcurrentQueue<string> messages;
        private bool isEnd;
        private Task loggingTask;

        public ConsoleLogger()
        {
            messages = new ConcurrentQueue<string>();
            loggingTask = Task.Run(() => Logging());
        }

        public void Log(string message)
        {
            messages.Enqueue($"{DateTime.Now.ToLocalTime()} | { message }");
        }

        public void Dispose()
        {
            isEnd = true;
            loggingTask.Wait();
            loggingTask.Dispose();
        }

        private void Logging()
        {
            while(!isEnd)
            {
                if (messages.TryDequeue(out var message))
                {
                    Console.WriteLine();
                }
            }
        }
    }
}
