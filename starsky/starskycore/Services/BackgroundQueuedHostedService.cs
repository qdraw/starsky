using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starskycore.Interfaces;

namespace starskycore.Services
{
    #region snippet1
    
    [Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
    public class BackgroundQueuedHostedService : BackgroundService
    {
        public BackgroundQueuedHostedService(IBackgroundTaskQueue taskQueue)
        {
            TaskQueue = taskQueue;
        }

        private IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {

            Console.WriteLine("Queued Hosted Service is starting.");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred executing {nameof(workItem)}.");
	                Console.WriteLine(ex);
                }
            }

            Console.WriteLine("Queued Hosted Service is stopping.");
        }
    }
    #endregion
}
