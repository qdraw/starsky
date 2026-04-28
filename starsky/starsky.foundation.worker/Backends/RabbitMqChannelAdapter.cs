using System;
using System.Diagnostics.CodeAnalysis;
using RabbitMQ.Client;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Backends;

internal interface IRabbitMqChannelClient : IDisposable
{
	bool IsOpen { get; }
	int GetMessageCount(string queueName);
	void Publish(string queueName, byte[] body, bool persistent);
	RabbitMqGetResult? TryGet(string queueName);
	void Ack(ulong deliveryTag);
	void Nack(ulong deliveryTag, bool requeue);
}

public sealed class RabbitMqChannelAdapter : IRabbitMqChannelAdapter
{
	private readonly Func<string, IRabbitMqChannelClient> _createChannel;
	private IRabbitMqChannelClient? _channelClient;

	public RabbitMqChannelAdapter(AppSettings appSettings)
		: this(queueName => new RabbitMqChannelClient(appSettings.Queue.RabbitMq, queueName))
	{
	}

	internal RabbitMqChannelAdapter(Func<string, IRabbitMqChannelClient> createChannel)
	{
		_createChannel = createChannel;
	}

	public int GetMessageCount(string queueName)
	{
		return EnsureChannel(queueName).GetMessageCount(queueName);
	}

	public void Publish(string queueName, byte[] body, bool persistent)
	{
		EnsureChannel(queueName).Publish(queueName, body, persistent);
	}

	public RabbitMqGetResult? TryGet(string queueName)
	{
		return EnsureChannel(queueName).TryGet(queueName);
	}

	public void Ack(ulong deliveryTag)
	{
		_channelClient?.Ack(deliveryTag);
	}

	public void Nack(ulong deliveryTag, bool requeue)
	{
		_channelClient?.Nack(deliveryTag, requeue);
	}

	private IRabbitMqChannelClient EnsureChannel(string queueName)
	{
		if ( _channelClient?.IsOpen == true )
		{
			return _channelClient;
		}

		_channelClient?.Dispose();
		_channelClient = _createChannel(queueName);
		return _channelClient;
	}
}

[ExcludeFromCodeCoverage]
internal sealed class RabbitMqChannelClient : IRabbitMqChannelClient
{
	private readonly IConnection _connection;
	private readonly IModel _channel;

	public RabbitMqChannelClient(AppSettingsRabbitMqModel settings, string queueName)
	{
		var factory = new ConnectionFactory
		{
			HostName = settings.Host,
			Port = settings.Port,
			UserName = settings.Username,
			Password = settings.Password,
			VirtualHost = settings.VirtualHost,
			DispatchConsumersAsync = true
		};

		_connection = factory.CreateConnection();
		_channel = _connection.CreateModel();
		_channel.QueueDeclare(queue: queueName, durable: true, exclusive: false,
			autoDelete: false, arguments: null);
	}

	public bool IsOpen => _connection.IsOpen && _channel.IsOpen;

	public int GetMessageCount(string queueName)
	{
		var state = _channel.QueueDeclarePassive(queueName);
		return (int)state.MessageCount;
	}

	public void Publish(string queueName, byte[] body, bool persistent)
	{
		var properties = _channel.CreateBasicProperties();
		properties.Persistent = persistent;
		_channel.BasicPublish(string.Empty, queueName, properties, body);
	}

	public RabbitMqGetResult? TryGet(string queueName)
	{
		var result = _channel.BasicGet(queueName, false);
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
		_channel.BasicAck(deliveryTag, false);
	}

	public void Nack(ulong deliveryTag, bool requeue)
	{
		_channel.BasicNack(deliveryTag, false, requeue);
	}

	public void Dispose()
	{
		_channel.Dispose();
		_connection.Dispose();
	}
}


