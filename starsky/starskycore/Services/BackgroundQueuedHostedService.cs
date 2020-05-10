using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starskycore.Services
{
    #region snippet1
    
    [Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
    public class BackgroundQueuedHostedService : BackgroundService
    {
	    private ITelemetryService _telemetryService;

	    public BackgroundQueuedHostedService(IBackgroundTaskQueue taskQueue, ITelemetryService telemetryService = null)
        {
            TaskQueue = taskQueue;
            _telemetryService = telemetryService;
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
                catch (Exception exception)
                {
                    Console.WriteLine($"Error occurred executing {nameof(workItem)}.");
	                Console.WriteLine(exception);
	                _telemetryService?.TrackException(exception);
                }
            }

            Console.WriteLine("Queued Hosted Service is stopping.");
        }
    }
    #endregion
}
