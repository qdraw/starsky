using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Protocol;
using starsky.foundation.connect.Protocol.Generated;

namespace starskytest.starsky.foundation.connect.Protocol;

[TestClass]
public sealed class BepConnectionTest
{
	[TestMethod]
	public void Constructor_SetsDeviceIdProperties()
	{
		using var stream = new MemoryStream();
		var bytes = new byte[] { 1, 2, 3 };
		using var conn = new BepConnection("ABCDE", bytes, stream);

		Assert.AreEqual("ABCDE", conn.DeviceId);
		CollectionAssert.AreEqual(bytes, conn.DeviceIdBytes);
	}

	[TestMethod]
	public void LastSentAndReceived_AreInitialisedToNow()
	{
		using var stream = new MemoryStream();
		var before = DateTimeOffset.UtcNow;
		using var conn = new BepConnection("X", [], stream);
		var after = DateTimeOffset.UtcNow;

		Assert.IsTrue(conn.LastSentUtc >= before && conn.LastSentUtc <= after);
		Assert.IsTrue(conn.LastReceivedUtc >= before && conn.LastReceivedUtc <= after);
	}

	[TestMethod]
	public async Task SendMessageAsync_WritesToStreamAndUpdatesLastSent()
	{
		using var stream = new MemoryStream();
		using var conn = new BepConnection("X", [], stream);

		var before = DateTimeOffset.UtcNow;
		await conn.SendMessageAsync(new Ping(), MessageType.Ping);

		Assert.IsTrue(stream.Length > 0, "Expected bytes written to stream");
		Assert.IsTrue(conn.LastSentUtc >= before);
	}

	[TestMethod]
	public async Task SendPingAsync_WritesToStream()
	{
		using var stream = new MemoryStream();
		using var conn = new BepConnection("X", [], stream);

		await conn.SendPingAsync();

		Assert.IsTrue(stream.Length > 0);
	}

	[TestMethod]
	public async Task SendMessageAsync_IsThreadSafe()
	{
		using var stream = new MemoryStream();
		using var conn = new BepConnection("X", [], stream);

		// Fire several concurrent sends and verify none throw
		var tasks = new Task[10];
		for ( var i = 0; i < tasks.Length; i++ )
		{
			tasks[i] = conn.SendPingAsync();
		}

		await Task.WhenAll(tasks);
		Assert.IsTrue(stream.Length > 0);
	}

	[TestMethod]
	public async Task ReadMessageAsync_UpdatesLastReceivedUtc()
	{
		// Pre-write a Ping message into a MemoryStream so BepReader has something to read
		using var stream = new MemoryStream();
		var writer = new BepWriter(stream);
		await writer.WriteMessageAsync(new Ping(), MessageType.Ping);
		stream.Position = 0;

		using var conn = new BepConnection("X", [], stream);
		var before = DateTimeOffset.UtcNow;
		var msg = await conn.ReadMessageAsync();
		var after = DateTimeOffset.UtcNow;

		Assert.AreEqual(MessageType.Ping, msg.Header.Type);
		Assert.IsTrue(conn.LastReceivedUtc >= before && conn.LastReceivedUtc <= after);
	}

	[TestMethod]
	public void Dispose_DoesNotThrow()
	{
		var stream = new MemoryStream();
		var conn = new BepConnection("X", [], stream);
		conn.Dispose();
		// double-dispose should not throw
		conn.Dispose();
	}

	[TestMethod]
	public async Task DisposeAsync_DoesNotThrow()
	{
		var stream = new MemoryStream();
		var conn = new BepConnection("X", [], stream);
		await conn.DisposeAsync();
	}

	[TestMethod]
	public async Task SendMessageAsync_CancellationToken_IsPropagated()
	{
		using var stream = new MemoryStream();
		using var conn = new BepConnection("X", [], stream);
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		var threw = false;
		try
		{
			await conn.SendMessageAsync(new Ping(), MessageType.Ping, ct: cts.Token);
		}
		catch ( OperationCanceledException )
		{
			threw = true;
		}

		Assert.IsTrue(threw, "Expected OperationCanceledException");
	}
}
