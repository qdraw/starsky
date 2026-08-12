using System.Diagnostics.CodeAnalysis;
using System.Threading;
using RabbitMQ.Client;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Backends.Interfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Backends;

[ExcludeFromCodeCoverage]
internal sealed class RabbitMqChannelClient : IRabbitMqChannelClient
{
	private readonly IChannel _channel;
	private readonly IConnection _connection;

	public RabbitMqChannelClient(AppSettingsRabbitMqModel settings, string queueName)
	{
		var factory = new ConnectionFactory
		{
			HostName = settings.Host,
			Port = settings.Port,
			UserName = settings.Username,
			Password = settings.Password,
			VirtualHost = settings.VirtualHost
		};

		_connection = factory.CreateConnectionAsync(CancellationToken.None).GetAwaiter()
			.GetResult();
		_channel = _connection.CreateChannelAsync(cancellationToken: CancellationToken.None)
			.GetAwaiter()
			.GetResult();
		_channel.QueueDeclareAsync(queueName, true, false,
			false, null, false, false, CancellationToken.None).GetAwaiter().GetResult();
	}

	public bool IsOpen => _connection.IsOpen && _channel.IsOpen;

	public int GetMessageCount(string queueName)
	{
		var state = _channel.QueueDeclarePassiveAsync(queueName, CancellationToken.None)
			.GetAwaiter()
			.GetResult();
		return ( int ) state.MessageCount;
	}

	public void Publish(string queueName, byte[] body, bool persistent)
	{
		var properties = new BasicProperties { Persistent = persistent };
		_channel.BasicPublishAsync(string.Empty, queueName, false, properties, body,
				CancellationToken.None)
			.AsTask().GetAwaiter().GetResult();
	}

	public RabbitMqGetResult? TryGet(string queueName)
	{
		var result = _channel.BasicGetAsync(queueName, false, CancellationToken.None).GetAwaiter()
			.GetResult();
		if ( result == null )
		{
			return null;
		}

		return new RabbitMqGetResult
		{
			DeliveryTag = result.DeliveryTag, Body = result.Body.ToArray()
		};
	}

	public void Ack(ulong deliveryTag)
	{
		_channel.BasicAckAsync(deliveryTag, false, CancellationToken.None).AsTask().GetAwaiter()
			.GetResult();
	}

	public void Nack(ulong deliveryTag, bool requeue)
	{
		_channel.BasicNackAsync(deliveryTag, false, requeue, CancellationToken.None).AsTask()
			.GetAwaiter().GetResult();
	}

	public void Dispose()
	{
		_channel.Dispose();
		_connection.Dispose();
	}
}
