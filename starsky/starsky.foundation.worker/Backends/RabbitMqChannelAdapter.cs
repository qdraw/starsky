using RabbitMQ.Client;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Backends;

public sealed class RabbitMqChannelAdapter(
	AppSettings appSettings) : IRabbitMqChannelAdapter
{
	private IConnection? _connection;
	private IModel? _channel;

	public int GetMessageCount(string queueName)
	{
		var channel = EnsureChannel(queueName);
		var state = channel.QueueDeclarePassive(queueName);
		return (int)state.MessageCount;
	}

	public void Publish(string queueName, byte[] body, bool persistent)
	{
		var channel = EnsureChannel(queueName);
		var properties = channel.CreateBasicProperties();
		properties.Persistent = persistent;
		channel.BasicPublish(string.Empty, queueName, properties, body);
	}

	public RabbitMqGetResult? TryGet(string queueName)
	{
		var channel = EnsureChannel(queueName);
		var result = channel.BasicGet(queueName, false);
		if ( result == null )
		{
			return null;
		}

		return new RabbitMqGetResult
		{
			DeliveryTag = result.DeliveryTag,
			Body = result.Body.ToArray()
		};
	}

	public void Ack(ulong deliveryTag)
	{
		_channel?.BasicAck(deliveryTag, false);
	}

	public void Nack(ulong deliveryTag, bool requeue)
	{
		_channel?.BasicNack(deliveryTag, false, requeue);
	}

	private IModel EnsureChannel(string queueName)
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


