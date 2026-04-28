using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Backends;
using starsky.foundation.worker.Interfaces;

namespace starskytest.starsky.foundation.worker.Backends;

[TestClass]
public sealed class RabbitMqChannelAdapterTest
{
	[TestMethod]
	public void PublicConstructor_CanInstantiateWithoutConnecting()
	{
		_ = new RabbitMqChannelAdapter(new AppSettings());
	}

	[TestMethod]
	public void GetMessageCount_ReusesOpenClient()
	{
		var createCount = 0;
		var fakeClient = new FakeRabbitMqChannelClient { IsOpenValue = true, MessageCount = 5 };
		var adapter = new RabbitMqChannelAdapter(_ =>
		{
			createCount++;
			return fakeClient;
		});

		var count1 = adapter.GetMessageCount("queue");
		var count2 = adapter.GetMessageCount("queue");

		Assert.AreEqual(5, count1);
		Assert.AreEqual(5, count2);
		Assert.AreEqual(1, createCount);
	}

	[TestMethod]
	public void GetMessageCount_RecreatesClosedClientAndDisposesOldOne()
	{
		var firstClient = new FakeRabbitMqChannelClient { IsOpenValue = true, MessageCount = 1 };
		var secondClient = new FakeRabbitMqChannelClient { IsOpenValue = true, MessageCount = 2 };
		var clients = new Queue<FakeRabbitMqChannelClient>(new[] { firstClient, secondClient });
		var createCount = 0;
		var adapter = new RabbitMqChannelAdapter(_ =>
		{
			createCount++;
			return clients.Dequeue();
		});

		var firstCount = adapter.GetMessageCount("queue");
		Assert.AreEqual(1, firstCount);
		firstClient.IsOpenValue = false;

		var secondCount = adapter.GetMessageCount("queue");
		Assert.AreEqual(2, secondCount);
		Assert.AreEqual(2, createCount);
		Assert.IsTrue(firstClient.DisposeCalled);
	}

	[TestMethod]
	public void Publish_ForwardsPayloadAndPersistence()
	{
		var fakeClient = new FakeRabbitMqChannelClient { IsOpenValue = true };
		var adapter = new RabbitMqChannelAdapter(_ => fakeClient);
		var body = Encoding.UTF8.GetBytes("hello");

		adapter.Publish("queue", body, true);

		Assert.AreEqual("queue", fakeClient.LastPublishedQueueName);
		CollectionAssert.AreEqual(body, fakeClient.LastPublishedBody!);
		Assert.IsTrue(fakeClient.LastPublishedPersistent);
	}

	[TestMethod]
	public void TryGet_ReturnsNullAndPayload()
	{
		var fakeClient = new FakeRabbitMqChannelClient { IsOpenValue = true };
		var adapter = new RabbitMqChannelAdapter(_ => fakeClient);

		Assert.IsNull(adapter.TryGet("queue"));

		fakeClient.NextResult = new RabbitMqGetResult
		{
			DeliveryTag = 42,
			Body = Encoding.UTF8.GetBytes("abc")
		};
		var result = adapter.TryGet("queue");
		Assert.IsNotNull(result);
		Assert.AreEqual((ulong)42, result.DeliveryTag);
	}

	[TestMethod]
	public void AckAndNack_ForwardToActiveClient()
	{
		var fakeClient = new FakeRabbitMqChannelClient { IsOpenValue = true, MessageCount = 1 };
		var adapter = new RabbitMqChannelAdapter(_ => fakeClient);
		_ = adapter.GetMessageCount("queue"); // ensure client is active

		adapter.Ack(11);
		adapter.Nack(22, true);

		Assert.AreEqual((ulong)11, fakeClient.LastAckDeliveryTag);
		Assert.AreEqual((ulong)22, fakeClient.LastNackDeliveryTag);
		Assert.IsTrue(fakeClient.LastNackRequeue);
	}
}

internal sealed class FakeRabbitMqChannelClient : IRabbitMqChannelClient
{
	public bool IsOpenValue { get; set; }
	public int MessageCount { get; set; }
	public bool DisposeCalled { get; private set; }

	public string? LastPublishedQueueName { get; private set; }
	public byte[]? LastPublishedBody { get; private set; }
	public bool LastPublishedPersistent { get; private set; }

	public RabbitMqGetResult? NextResult { get; set; }
	public ulong LastAckDeliveryTag { get; private set; }
	public ulong LastNackDeliveryTag { get; private set; }
	public bool LastNackRequeue { get; private set; }

	public bool IsOpen => IsOpenValue;

	public int GetMessageCount(string queueName)
	{
		return MessageCount;
	}

	public void Publish(string queueName, byte[] body, bool persistent)
	{
		LastPublishedQueueName = queueName;
		LastPublishedBody = body;
		LastPublishedPersistent = persistent;
	}

	public RabbitMqGetResult? TryGet(string queueName)
	{
		var value = NextResult;
		NextResult = null;
		return value;
	}

	public void Ack(ulong deliveryTag)
	{
		LastAckDeliveryTag = deliveryTag;
	}

	public void Nack(ulong deliveryTag, bool requeue)
	{
		LastNackDeliveryTag = deliveryTag;
		LastNackRequeue = requeue;
	}

	public void Dispose()
	{
		DisposeCalled = true;
	}
}



