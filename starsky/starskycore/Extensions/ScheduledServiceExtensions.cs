using System;
using Microsoft.Extensions.DependencyInjection;
using starskycore.Interfaces;
using starskycore.Services;

namespace starskycore.Extensions
{
	public static class ScheduledServiceExtensions
	{
		public static IServiceCollection AddCronTab<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : CronTabBackgroundService
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options), @"Please provide Schedule Configurations.");
			}
			var config = new ScheduleConfig<T>();
			options.Invoke(config);
			if (string.IsNullOrWhiteSpace(config.CronExpression))
			{
				throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), @"Empty Cron Expression is not allowed.");
			}

			services.AddSingleton<IScheduleConfig<T>>(config);
			services.AddHostedService<T>();
			return services;
		}
	}
}
