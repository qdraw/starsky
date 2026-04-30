using System;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Backends.Interfaces;

internal interface IRabbitMqChannelClient : IDisposable
{
	bool IsOpen { get; }
	int GetMessageCount(string queueName);
	void Publish(string queueName, byte[] body, bool persistent);
	RabbitMqGetResult? TryGet(string queueName);
	void Ack(ulong deliveryTag);
	void Nack(ulong deliveryTag, bool requeue);
}
