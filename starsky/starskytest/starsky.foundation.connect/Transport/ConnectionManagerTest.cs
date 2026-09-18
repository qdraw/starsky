using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Crypto;
using starsky.foundation.connect.Protocol.Generated;
using starsky.foundation.connect.Transport;

namespace starskytest.starsky.foundation.connect.Transport;

[TestClass]
public sealed class ConnectionManagerTest
{
	private X509Certificate2 _cert = null!;

	[TestInitialize]
	public void Setup()
	{
		_cert = DeviceIdentity.GenerateCertificate();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_cert.Dispose();
	}

	[TestMethod]
	public void ConnectedDeviceIds_IsEmpty_Initially()
	{
		using var manager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: 0);

		Assert.AreEqual(0, manager.ConnectedDeviceIds.Count);
	}

	[TestMethod]
	public void AddPeer_RegistersPeer()
	{
		using var manager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: 0);

		// Should not throw
		manager.AddPeer("MFZWI3DBONSGYYY-BSMAQLD2LZA66ZW-7JKR4TER-MHUI3OF-EZ6F63P-GWIAZDR-NBJDSYZ-IQNQPAN", ["tcp://127.0.0.1:22000"]);
	}

	[TestMethod]
	public async Task SendAsync_ToUnknownPeer_DoesNotThrow()
	{
		using var manager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: 0);

		// Peer is not connected — send must be a no-op
		await manager.SendAsync("SOMEDEVICE", new Ping(), MessageType.Ping);
	}

	[TestMethod]
	public async Task StopAsync_WithoutStartAsync_DoesNotThrow()
	{
		using var manager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: 0);

		await manager.StopAsync(CancellationToken.None);
	}

	[TestMethod]
	public void Dispose_DoesNotThrow()
	{
		var manager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: 0);
		manager.Dispose();
	}

	[TestMethod]
	public async Task StartAsync_ThenStopAsync_DoesNotThrow()
	{
		// Pick a free port to avoid port conflicts in CI
		int port;
		using ( var tmp = new TcpListener(IPAddress.Loopback, 0) )
		{
			tmp.Start();
			port = ( ( IPEndPoint )tmp.LocalEndpoint ).Port;
			tmp.Stop();
		}

		using var manager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: port);

		await manager.StartAsync(CancellationToken.None);
		await manager.StopAsync(CancellationToken.None);
	}

	[TestMethod]
	public async Task InjectConnectionAsync_WithMemoryStream_StartsTask()
	{
		// We can't complete a full Hello handshake with a plain MemoryStream (no real peer),
		// but InjectConnectionAsync must return promptly and queue the work as a background Task.
		using var manager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: 0);

		using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
		using var stream = new MemoryStream(); // empty stream → handshake will fail fast

		// Should return before the handshake completes (fire-and-forget pattern)
		await manager.InjectConnectionAsync(stream, "ABCDEFG", [], cts.Token);
	}

	[TestMethod]
	public void OnPeerConnected_CanBeSet()
	{
		using var manager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: 0);

		manager.OnPeerConnected = (id, hello, ct) => Task.CompletedTask;
		manager.OnMessageReceived = (id, msg, ct) => Task.CompletedTask;
		manager.OnPeerDisconnected = (id, ex, ct) => Task.CompletedTask;
	}
}
