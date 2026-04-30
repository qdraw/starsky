using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Backends;

public sealed class DatabaseQueueBackend(
	IServiceScopeFactory scopeFactory,
	AppSettings appSettings,
	IWebLogger logger,
	string queueName) : IBaseBackgroundTaskQueue
{
	private readonly int _databasePollIntervalInMilliseconds =
		Math.Max(100, appSettings.Queue.DatabasePollIntervalInMilliseconds);

	public int Count()
	{
		using var scope = scopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		return context.QueueItems.Count(item =>
			item.QueueName == queueName && item.Status == QueueItemStatus.Pending);
	}

	public async ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		ArgumentNullException.ThrowIfNull(job);
		if ( string.IsNullOrWhiteSpace(job.JobType) )
		{
			throw new ArgumentException("JobType is required", nameof(job));
		}

		using var scope = scopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		context.QueueItems.Add(new QueueItem
		{
			QueueName = queueName,
			JobId = job.JobId,
			MetaData = job.MetaData,
			TraceParentId = job.TraceParentId,
			PriorityLane = job.PriorityLane,
			JobType = job.JobType,
			PayloadJson = job.PayloadJson,
			Status = QueueItemStatus.Pending,
			CreatedAtUtc = job.CreatedAtUtc == default ? DateTime.UtcNow : job.CreatedAtUtc
		});
		await context.SaveChangesAsync();
	}

	public async ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		while ( !cancellationToken.IsCancellationRequested )
		{
			using var scope = scopeFactory.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var candidates = await context.QueueItems
				.Where(item => item.QueueName == queueName && item.Status == QueueItemStatus.Pending)
				.OrderBy(item => item.CreatedAtUtc)
				.ThenBy(item => item.Id)
				.Take(10)
				.ToListAsync(cancellationToken);

			if ( candidates.Count == 0 )
			{
				await Task.Delay(_databasePollIntervalInMilliseconds, cancellationToken);
				continue;
			}

			foreach ( var candidate in candidates )
			{
				candidate.Status = QueueItemStatus.Processing;
				candidate.ClaimedAtUtc = DateTime.UtcNow;

				try
				{
					await context.SaveChangesAsync(cancellationToken);

					var queueJob = new BackgroundTaskQueueJob
					{
						JobId = candidate.JobId,
						MetaData = candidate.MetaData,
						TraceParentId = candidate.TraceParentId,
						PriorityLane = candidate.PriorityLane,
						JobType = candidate.JobType,
						PayloadJson = candidate.PayloadJson,
						CreatedAtUtc = candidate.CreatedAtUtc
					};

					// Mimic in-memory semantics: remove from durable queue once dequeued.
					context.QueueItems.Remove(candidate);
					await context.SaveChangesAsync(cancellationToken);
					return queueJob;
				}
				catch ( DbUpdateConcurrencyException concurrencyException )
				{
					logger.LogInformation(concurrencyException,
						$"[DatabaseQueueBackend] Queue claim race detected for queue {queueName}");
					context.ChangeTracker.Clear();
				}
			}
		}

		throw new OperationCanceledException(cancellationToken);
	}
}


