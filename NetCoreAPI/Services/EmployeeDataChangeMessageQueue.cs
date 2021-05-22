using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreAPI.Services
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(WorkItem item);
        void QueueDataChangeMessage(Message message);

        Task<WorkItem> DequeueAsync(CancellationToken cancellationToken);
        public int Size();
        public void PrintQueueInfo();
        //Task<Func<CancellationToken, Task>> DequeueAsync(
        //    CancellationToken cancellationToken);
    }
    public class EmployeeDataChangeMessageQueue : IBackgroundTaskQueue
    {
        private ConcurrentQueue<WorkItem> _workItems = new ConcurrentQueue<WorkItem>();
        private ConcurrentQueue<Message> _message = new ConcurrentQueue<Message>();

        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        public int workItems = 0;
        private IServiceLog logger;

        public EmployeeDataChangeMessageQueue(IServiceLog log)
        {
            logger = log;
        }

        public void QueueBackgroundWorkItem(WorkItem workItem)
        {
            if (workItem == null)
            {
                //throw new ArgumentNullException(nameof(workItem));
                return;
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
            logger.Log(Level.Trace, "Added a task to queue: type = " + workItem.Type + ", tasks in queue = " + _workItems.Count);
        }
        public async Task<WorkItem> DequeueAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }

        public void QueueDataChangeMessage(Message message)
        {
            if (message == null)
            {
                return;
            }

            _message.Enqueue(message);
            _signal.Release();
            logger.Log(Level.Trace, "Added a message to queue, messages in queue = " + _message.Count);
        }

        public int Size()
        {
            return _workItems.Count;
        }

        public void PrintQueueInfo()
        {
            Console.WriteLine($"Total workitems in queue (to-be-checked-out) = {_workItems.Count}");
        }

    }

    public class WorkItem
    {
        public Func<CancellationToken, Task> start;
        public String Type;

        public WorkItem(Func<CancellationToken, Task> work, String type)
        {
            start = work;
            Type = type;
        }
    }

    public class Message
    {
        public Func<CancellationToken, Task> start;
        public String Type;

        public Message(Func<CancellationToken, Task> work, String type)
        {
            start = work;
            Type = type;
        }
    }
}
