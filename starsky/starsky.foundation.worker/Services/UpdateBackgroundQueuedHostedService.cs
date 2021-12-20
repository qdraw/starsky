using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.worker.Services
{
	[Service(typeof(IHostedService),
		InjectionLifetime = InjectionLifetime.Singleton)]
	public class UpdateBackgroundQueuedHostedService : BackgroundService
	{
		private readonly IWebLogger _logger;
		
		// ReSharper disable once SuggestBaseTypeForParameterInConstructor
		public UpdateBackgroundQueuedHostedService(IUpdateBackgroundTaskQueue taskQueue,
			IWebLogger logger)
		{
			TaskQueue = taskQueue;
			_logger = logger;
		}

		private IBaseBackgroundTaskQueue TaskQueue { get; }

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return ProcessTaskQueue.ProcessTaskQueueAsync(TaskQueue, _logger, stoppingToken);
		}
	}
}
