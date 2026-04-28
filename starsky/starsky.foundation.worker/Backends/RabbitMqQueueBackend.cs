using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Backends;

public sealed class RabbitMqQueueBackend(
	AppSettings appSettings,
	IWebLogger logger,
	string queueName) : IBaseBackgroundTaskQueue
{
	private readonly object _sync = new();
	private readonly int _pollIntervalInMilliseconds =
		Math.Max(100, appSettings.Queue.DatabasePollIntervalInMilliseconds);

	private IConnection? _connection;
	private IModel? _channel;

	public int Count()
	{
		try
		{
			lock ( _sync )
			{
				var channel = EnsureChannel();
				var state = channel.QueueDeclarePassive(queueName);
				return (int)state.MessageCount;
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

		lock ( _sync )
		{
			var channel = EnsureChannel();
			var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));
			var properties = channel.CreateBasicProperties();
			properties.Persistent = true;

			channel.BasicPublish(
				exchange: string.Empty,
				routingKey: queueName,
				basicProperties: properties,
				body: body);
		}

		return ValueTask.CompletedTask;
	}

	public async ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		while ( !cancellationToken.IsCancellationRequested )
		{
			BasicGetResult? result;
			lock ( _sync )
			{
				var channel = EnsureChannel();
				result = channel.BasicGet(queueName, false);
			}

			if ( result == null )
			{
				await Task.Delay(_pollIntervalInMilliseconds, cancellationToken);
				continue;
			}

			var json = Encoding.UTF8.GetString(result.Body.ToArray());
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
					_channel?.BasicNack(result.DeliveryTag, false, false);
				}

				continue;
			}

			if ( queueJob == null )
			{
				lock ( _sync )
				{
					_channel?.BasicNack(result.DeliveryTag, false, false);
				}

				continue;
			}

			lock ( _sync )
			{
				_channel?.BasicAck(result.DeliveryTag, false);
			}

			return queueJob;
		}

		throw new OperationCanceledException(cancellationToken);
	}

	private IModel EnsureChannel()
	{
		if ( _connection?.IsOpen == true && _channel?.IsOpen == true )
		{
			return _channel;
		}

		_channel?.Dispose();
		_connection?.Dispose();

		var rabbitMqSettings = appSettings.Queue.RabbitMq;
		var factory = new ConnectionFactory
		{
			HostName = rabbitMqSettings.Host,
			Port = rabbitMqSettings.Port,
			UserName = rabbitMqSettings.Username,
			Password = rabbitMqSettings.Password,
			VirtualHost = rabbitMqSettings.VirtualHost,
			DispatchConsumersAsync = true
		};

		_connection = factory.CreateConnection();
		_channel = _connection.CreateModel();
		_channel.QueueDeclare(queue: queueName, durable: true, exclusive: false,
			autoDelete: false, arguments: null);

		return _channel;
	}
}

