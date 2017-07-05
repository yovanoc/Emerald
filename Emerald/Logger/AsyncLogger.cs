using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Emerald.Logger
{
    public class AsyncLogger : IDisposable
    {
        public delegate void MessagesLoggedEventHandler(LogMessage message);

        private readonly Task _loggingTask;
        private readonly BlockingCollection<LogMessage> _queue;

        public AsyncLogger()
        {
            _queue = new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>());
            _loggingTask = Task.Factory.StartNew(ConsumeQueueItem);
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _loggingTask.Wait();
        }

        public event MessagesLoggedEventHandler MessageLogged;

        public void Log(LogMessage message)
        {
            _queue.Add(message);
        }

        private void ConsumeQueueItem()
        {
            foreach (var message in _queue.GetConsumingEnumerable())
                MessageLogged?.Invoke(message);
        }
    }
}