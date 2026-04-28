using System;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Backends.Interfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Backends;

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
