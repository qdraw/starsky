using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Backends;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.Backends;

[TestClass]
public sealed class RabbitMqQueueBackendTest
{
	private static (RabbitMqQueueBackend backend, FakeRabbitMqChannelAdapter adapter, FakeIWebLogger
		logger)
		CreateBackend()
	{
		var appSettings = new AppSettings();
		var adapter = new FakeRabbitMqChannelAdapter();
		var logger = new FakeIWebLogger();
		var backend = new RabbitMqQueueBackend(appSettings, logger, "TestQueue", adapter);
		return ( backend, adapter, logger );
	}

	[TestMethod]
	public async Task QueueJobAsync_Null_ThrowsArgumentNullException()
	{
		var (backend, _, _) = CreateBackend();
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
			await backend.QueueJobAsync(null!));
	}

	[TestMethod]
	public async Task QueueJobAsync_WithoutJobType_ThrowsArgumentException()
	{
		var (backend, _, _) = CreateBackend();
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await backend.QueueJobAsync(new BackgroundTaskQueueJob()));
	}

	[TestMethod]
	public async Task QueueJobAsync_PublishesSerializedPayload()
	{
		var (backend, adapter, _) = CreateBackend();
		var job = new BackgroundTaskQueueJob
		{
			JobType = "Rabbit.v1", MetaData = "meta", PayloadJson = "{\"k\":1}"
		};

		await backend.QueueJobAsync(job);

		Assert.HasCount(1, adapter.Published);
		var published = adapter.Published[0];
		Assert.AreEqual("TestQueue", published.QueueName);
		Assert.IsTrue(published.Persistent);

		var json = Encoding.UTF8.GetString(published.Body);
		var roundTrip = JsonSerializer.Deserialize<BackgroundTaskQueueJob>(json);
		Assert.IsNotNull(roundTrip);
		Assert.AreEqual(job.JobType, roundTrip.JobType);
		Assert.AreEqual(job.MetaData, roundTrip.MetaData);
	}

	[TestMethod]
	public void Count_UsesAdapterMessageCount()
	{
		var (backend, adapter, _) = CreateBackend();
		adapter.MessageCount = 7;

		var count = backend.Count();

		Assert.AreEqual(7, count);
	}

	[TestMethod]
	public async Task DequeueJobAsync_ValidMessage_AcksAndReturnsItem()
	{
		var (backend, adapter, _) = CreateBackend();
		var payload = JsonSerializer.Serialize(new BackgroundTaskQueueJob
		{
			JobType = "Rabbit.v1", PayloadJson = "ok"
		});
		adapter.Messages.Enqueue(new RabbitMqGetResult
		{
			DeliveryTag = 100, Body = Encoding.UTF8.GetBytes(payload)
		});

		var dequeued = await backend.DequeueJobAsync(CancellationToken.None);

		Assert.AreEqual("Rabbit.v1", dequeued.JobType);
		CollectionAssert.AreEqual(new List<ulong> { 100 }, adapter.Acked.ToList());
		Assert.IsEmpty(adapter.Nacked);
	}

	[TestMethod]
	public async Task DequeueJobAsync_InvalidJson_NacksAndContinues()
	{
		var (backend, adapter, logger) = CreateBackend();
		adapter.Messages.Enqueue(new RabbitMqGetResult
		{
			DeliveryTag = 10, Body = Encoding.UTF8.GetBytes("{")
		});

		var validPayload = JsonSerializer.Serialize(new BackgroundTaskQueueJob
		{
			JobType = "Rabbit.v1", PayloadJson = "ok"
		});
		adapter.Messages.Enqueue(new RabbitMqGetResult
		{
			DeliveryTag = 11, Body = Encoding.UTF8.GetBytes(validPayload)
		});

		var dequeued = await backend.DequeueJobAsync(CancellationToken.None);

		Assert.AreEqual("Rabbit.v1", dequeued.JobType);
		CollectionAssert.AreEqual(new List<ulong> { 10 },
			adapter.Nacked.Select(p => p.DeliveryTag).ToList());
		CollectionAssert.AreEqual(new List<ulong> { 11 }, adapter.Acked.ToList());
		Assert.Contains(p =>
			( p.Item2 ?? string.Empty ).Contains("Invalid message payload"), logger.TrackedExceptions);
	}

	[TestMethod]
	public async Task DequeueJobAsync_NullDeserializedPayload_NacksAndContinues()
	{
		var (backend, adapter, _) = CreateBackend();
		adapter.Messages.Enqueue(new RabbitMqGetResult
		{
			DeliveryTag = 20, Body = Encoding.UTF8.GetBytes("null")
		});

		var validPayload = JsonSerializer.Serialize(new BackgroundTaskQueueJob
		{
			JobType = "Rabbit.v1", PayloadJson = "ok"
		});
		adapter.Messages.Enqueue(new RabbitMqGetResult
		{
			DeliveryTag = 21, Body = Encoding.UTF8.GetBytes(validPayload)
		});

		var dequeued = await backend.DequeueJobAsync(CancellationToken.None);

		Assert.AreEqual("Rabbit.v1", dequeued.JobType);
		CollectionAssert.AreEqual(new List<ulong> { 20 },
			adapter.Nacked.Select(p => p.DeliveryTag).ToList());
		CollectionAssert.AreEqual(new List<ulong> { 21 }, adapter.Acked.ToList());
	}

	[TestMethod]
	public void Count_BrokerUnreachable_ReturnsZeroAndLogsWarning()
	{
		var (backend, adapter, logger) = CreateBackend();
		adapter.GetMessageCountException = new BrokerUnreachableException(new Exception("offline"));

		var count = backend.Count();

		Assert.AreEqual(0, count);
		Assert.Contains(p =>
			( p.Item2 ?? string.Empty ).Contains("Unable to read queue depth"), logger.TrackedWarnings);
	}

	[TestMethod]
	public void Count_OperationInterrupted_ReturnsZeroAndLogsWarning()
	{
		var (backend, adapter, logger) = CreateBackend();
		var shutdownArgs = new ShutdownEventArgs(ShutdownInitiator.Library, 541, "shutdown");
		adapter.GetMessageCountException = new OperationInterruptedException(shutdownArgs);

		var count = backend.Count();

		Assert.AreEqual(0, count);
		Assert.Contains(p =>
			( p.Item2 ?? string.Empty ).Contains("Unable to read queue depth"), logger.TrackedWarnings);
	}

	[TestMethod]
	public async Task DequeueJobAsync_CancelledToken_ThrowsOperationCanceledException()
	{
		var (backend, _, _) = CreateBackend();
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();

		await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
			await backend.DequeueJobAsync(cancellation.Token));
	}

	[TestMethod]
	public async Task DequeueJobAsync_EmptyQueueThenMessage_HitsDelayAndContinue()
	{
		var appSettings = new AppSettings
		{
			Queue = new AppSettingsQueueModel { DatabasePollIntervalInMilliseconds = 10 }
		};
		var adapter = new CompletingEmptyThenMessageAdapter();
		var logger = new FakeIWebLogger();
		var backend = new RabbitMqQueueBackend(appSettings, logger, "TestQueue", adapter);

		var validPayload = JsonSerializer.Serialize(new BackgroundTaskQueueJob
		{
			JobType = "Rabbit.v1", PayloadJson = "ok"
		});
		adapter.Messages.Enqueue(new RabbitMqGetResult
		{
			DeliveryTag = 100, Body = Encoding.UTF8.GetBytes(validPayload)
		});

		var dequeued = await backend.DequeueJobAsync(CancellationToken.None);

		Assert.AreEqual("Rabbit.v1", dequeued.JobType);
		Assert.IsTrue(adapter.GetWasCalledTwice,
			"TryGet should have been called at least twice (first empty, then with message)");
		CollectionAssert.AreEqual(new List<ulong> { 100 }, adapter.Acked.ToList());
	}

	[TestMethod]
	public async Task DequeueJobAsync_EmptyQueueWithCancellation_HitsDelayBeforeCancellation()
	{
		var appSettings = new AppSettings
		{
			Queue = new AppSettingsQueueModel { DatabasePollIntervalInMilliseconds = 1 }
		};
		var adapter = new AlwaysEmptyAdapter();
		var logger = new FakeIWebLogger();
		var backend = new RabbitMqQueueBackend(appSettings, logger, "TestQueue", adapter);

		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromMilliseconds(100));

		var sw = Stopwatch.StartNew();
		var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
			await backend.DequeueJobAsync(cts.Token));
		sw.Stop();
		Assert.AreEqual(cts.Token, exception.CancellationToken,
			"The cancellation should come from the test token");

		// Should have executed at least 1 delay iteration before cancellation
		Assert.IsGreaterThanOrEqualTo(50, sw.ElapsedMilliseconds,
			"Should have waited for at least one poll interval");
		Assert.IsTrue(adapter.GetWasCalled, "TryGet should have been called at least once");
	}
}

internal sealed class FakeRabbitMqChannelAdapter : IRabbitMqChannelAdapter
{
	public Exception? GetMessageCountException { get; set; }
	public int MessageCount { get; set; }
	public List<(string QueueName, byte[] Body, bool Persistent)> Published { get; } = [];
	public Queue<RabbitMqGetResult> Messages { get; } = new();
	public List<ulong> Acked { get; } = [];
	public List<(ulong DeliveryTag, bool Requeue)> Nacked { get; } = [];

	public int GetMessageCount(string queueName)
	{
		if ( GetMessageCountException != null )
		{
			throw GetMessageCountException;
		}

		return MessageCount;
	}

	public void Publish(string queueName, byte[] body, bool persistent)
	{
		Published.Add(( queueName, body, persistent ));
	}

	public RabbitMqGetResult? TryGet(string queueName)
	{
		return Messages.Count > 0 ? Messages.Dequeue() : null;
	}

	public void Ack(ulong deliveryTag)
	{
		Acked.Add(deliveryTag);
	}

	public void Nack(ulong deliveryTag, bool requeue)
	{
		Nacked.Add(( deliveryTag, requeue ));
	}
}

internal sealed class CompletingEmptyThenMessageAdapter : IRabbitMqChannelAdapter
{
	public bool GetWasCalledTwice { get; private set; }
	public List<ulong> Acked { get; } = [];
	public Queue<RabbitMqGetResult> Messages { get; } = new();
	private int _callCount;

	public int GetMessageCount(string queueName)
	{
		return 0;
	}

	public void Publish(string queueName, byte[] body, bool persistent)
	{
	}

	public void Ack(ulong deliveryTag)
	{
		Acked.Add(deliveryTag);
	}

	public void Nack(ulong deliveryTag, bool requeue)
	{
	}

	public RabbitMqGetResult? TryGet(string queueName)
	{
		_callCount++;
		if ( _callCount > 1 )
		{
			GetWasCalledTwice = true;
			return Messages.Count > 0 ? Messages.Dequeue() : null;
		}

		return null;
	}
}

internal sealed class AlwaysEmptyAdapter : IRabbitMqChannelAdapter
{
	public bool GetWasCalled { get; private set; }

	public int GetMessageCount(string queueName)
	{
		return 0;
	}

	public void Publish(string queueName, byte[] body, bool persistent)
	{
	}

	public void Ack(ulong deliveryTag)
	{
	}

	public void Nack(ulong deliveryTag, bool requeue)
	{
	}

	public RabbitMqGetResult? TryGet(string queueName)
	{
		GetWasCalled = true;
		return null;
	}
}
