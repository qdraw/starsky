using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using starskycore.Interfaces;

namespace starskycore.Services
{
	public class ClearCacheCronTabService : CronTabBackgroundService
	{
		private readonly ILogger<ClearCacheCronTabService> _logger;

		public ClearCacheCronTabService(IScheduleConfig<ClearCacheCronTabService> config, ILogger<ClearCacheCronTabService> logger)
			: base(config.CronExpression, config.TimeZoneInfo)
		{
			_logger = logger;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("CronJob 3 starts.");
			return base.StartAsync(cancellationToken);
		}

		public override Task DoWork(CancellationToken cancellationToken)
		{
			_logger.LogInformation($"{DateTime.Now:hh:mm:ss} CronJob 3 is working.");
			return Task.CompletedTask;
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("CronJob 3 is stopping.");
			return base.StopAsync(cancellationToken);
		}
	}
	
}
