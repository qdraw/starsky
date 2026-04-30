namespace starsky.foundation.worker.Interfaces;

public sealed class RabbitMqGetResult
{
	public ulong DeliveryTag { get; set; }
	public required byte[] Body { get; set; }
}

public interface IRabbitMqChannelAdapter
{
	int GetMessageCount(string queueName);
	void Publish(string queueName, byte[] body, bool persistent);
	RabbitMqGetResult? TryGet(string queueName);
	void Ack(ulong deliveryTag);
	void Nack(ulong deliveryTag, bool requeue);
}

