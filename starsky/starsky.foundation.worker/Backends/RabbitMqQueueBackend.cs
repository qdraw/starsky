using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Exceptions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Backends;

public sealed class RabbitMqQueueBackend(
	AppSettings appSettings,
	IWebLogger logger,
	string queueName,
	IRabbitMqChannelAdapter? adapter = null) : IBaseBackgroundTaskQueue
{
	private readonly object _sync = new();
	private readonly IRabbitMqChannelAdapter _adapter = adapter ??
	                                                    new RabbitMqChannelAdapter(appSettings);
	private readonly int _pollIntervalInMilliseconds =
		Math.Max(100, appSettings.Queue.DatabasePollIntervalInMilliseconds);

	public int Count()
	{
		try
		{
			lock ( _sync )
			{
				return _adapter.GetMessageCount(queueName);
			}
		}
		catch ( Exception exception ) when (exception is BrokerUnreachableException or OperationInterruptedException)
		{
			logger.LogWarning(exception,
				$"[RabbitMqQueueBackend] Unable to read queue depth for queue {queueName}");
			return 0;
		}
	}

	public ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		ArgumentNullException.ThrowIfNull(job);
		if ( string.IsNullOrWhiteSpace(job.JobType) )
		{
			throw new ArgumentException("JobType is required", nameof(job));
		}

		QueueJobTenantEnforcer.ValidateTenantOrThrow(job, logger, queueName);

		lock ( _sync )
		{
			var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));
			_adapter.Publish(queueName, body, true);
		}

		return ValueTask.CompletedTask;
	}

	public async ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		while ( !cancellationToken.IsCancellationRequested )
		{
			RabbitMqGetResult? result;
			lock ( _sync )
			{
				result = _adapter.TryGet(queueName);
			}

			if ( result == null )
			{
				await Task.Delay(_pollIntervalInMilliseconds, cancellationToken);
				continue;
			}

			var json = Encoding.UTF8.GetString(result.Body);
			BackgroundTaskQueueJob? queueJob;
			try
			{
				queueJob = JsonSerializer.Deserialize<BackgroundTaskQueueJob>(json);
			}
			catch ( JsonException exception )
			{
				logger.LogError(exception,
					$"[RabbitMqQueueBackend] Invalid message payload for queue {queueName}");
				lock ( _sync )
				{
					_adapter.Nack(result.DeliveryTag, false);
				}

				continue;
			}

			if ( queueJob == null )
			{
				lock ( _sync )
				{
					_adapter.Nack(result.DeliveryTag, false);
				}

				continue;
			}

			lock ( _sync )
			{
				_adapter.Ack(result.DeliveryTag);
			}

			return queueJob;
		}

		throw new OperationCanceledException(cancellationToken);
	}
}

