using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.Hosting;

namespace starskycore.Services
{
	/// <summary>
	/// Handling cron in the background as IHostedService
	/// @see: https://codeburst.io/schedule-cron-jobs-using-hostedservice-in-asp-net-core-e17c47ba06
	/// </summary>
	public abstract class CronTabBackgroundService : IHostedService, IDisposable
	{
		private System.Timers.Timer _timer;
		private readonly CronExpression _expression;
		private readonly TimeZoneInfo _timeZoneInfo;

		protected CronTabBackgroundService(string cronExpression, TimeZoneInfo timeZoneInfo)
		{
			_expression = CronExpression.Parse(cronExpression);
			_timeZoneInfo = timeZoneInfo;
		}

		public virtual async Task StartAsync(CancellationToken cancellationToken)
		{
			await ScheduleJob(cancellationToken);
		}

		protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
		{
			var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
			if (next.HasValue)
			{
				var delay = next.Value - DateTimeOffset.Now;
				_timer = new System.Timers.Timer(delay.TotalMilliseconds);
				_timer.Elapsed += async (sender, args) =>
				{
					_timer.Stop();  // reset timer
					await DoWork(cancellationToken);
					await ScheduleJob(cancellationToken);    // reschedule next
				};
				_timer.Start();
			}
			await Task.CompletedTask;
		}

		public virtual async Task DoWork(CancellationToken cancellationToken)
		{
		}

		public virtual async Task StopAsync(CancellationToken cancellationToken)
		{
			_timer?.Stop();
			await Task.CompletedTask;
		}

		public virtual void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
