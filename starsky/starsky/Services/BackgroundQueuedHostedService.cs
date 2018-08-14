using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace starsky.Services
{
    #region snippet1
    public class BackgroundQueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;

        public BackgroundQueuedHostedService(IBackgroundTaskQueue taskQueue, 
            ILoggerFactory loggerFactory)
        {
            TaskQueue = taskQueue;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(
            CancellationToken cancellationToken)
        {

            Console.WriteLine("Queued Hosted Service is starting.");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(cancellationToken);

                try
                {
                    await workItem(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred executing {nameof(workItem)}.");
                }
            }

            Console.WriteLine("Queued Hosted Service is stopping.");
        }
    }
    #endregion
}
