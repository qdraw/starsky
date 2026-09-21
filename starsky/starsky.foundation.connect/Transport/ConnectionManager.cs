using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using starsky.foundation.connect.Crypto;
using starsky.foundation.connect.Protocol;
using starsky.foundation.connect.Protocol.Generated;

namespace starsky.foundation.connect.Transport;

/// <summary>
/// Manages the lifecycle of BEP connections to multiple peers.
/// Handles outbound dialing (with exponential reconnect backoff), inbound acceptance,
/// and the per-connection keepalive / receive-timeout timers.
/// </summary>
public sealed class ConnectionManager : IHostedService, IDisposable
{
	private static readonly TimeSpan PingSendInterval = TimeSpan.FromSeconds(90);
	private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(300);
	private static readonly TimeSpan PingCheckInterval = TimeSpan.FromSeconds(45);
	private static readonly TimeSpan MinReconnectDelay = TimeSpan.FromSeconds(5);
	private static readonly TimeSpan MaxReconnectDelay = TimeSpan.FromMinutes(30);

	private readonly X509Certificate2 _localCert;
	private readonly ILogger<ConnectionManager> _logger;
	private readonly ConcurrentDictionary<string, BepConnection> _connections = new();
	private readonly ConcurrentDictionary<string, string[]> _outboundPeers = new();
	private readonly CancellationTokenSource _cts = new();
	private TlsListener? _listener;

	/// <summary>
	/// Raised when a peer connects and sends its Hello. Arguments: (deviceId, hello).
	/// </summary>
	public Func<string, Hello, CancellationToken, Task>? OnPeerConnected { get; set; }

	/// <summary>
	/// Raised when a message arrives from a peer. Arguments: (deviceId, message).
	/// </summary>
	public Func<string, BepMessage, CancellationToken, Task>? OnMessageReceived { get; set; }

	/// <summary>
	/// Raised when a peer disconnects. Arguments: (deviceId, reason).
	/// </summary>
	public Func<string, Exception?, CancellationToken, Task>? OnPeerDisconnected { get; set; }

	private readonly int _listenPort;

	public ConnectionManager(
		X509Certificate2 localCert,
		ILogger<ConnectionManager> logger,
		int listenPort = 22000)
	{
		_localCert = localCert;
		_logger = logger;
		_listenPort = listenPort;
	}

	/// <summary>All currently connected device IDs (normalized, no hyphens).</summary>
	public IReadOnlyCollection<string> ConnectedDeviceIds => _connections.Keys.ToArray();

	/// <summary>
	/// Registers an outbound peer to connect to.
	/// The manager dials on startup and reconnects with exponential backoff after disconnection.
	/// </summary>
	public void AddPeer(string deviceId, string[] addresses)
	{
		var normalized = DeviceIdentity.NormalizeDeviceId(deviceId);
		_outboundPeers[normalized] = addresses;
	}

	/// <summary>Sends a BEP message to a connected peer. No-ops if peer is not connected.</summary>
	public async Task SendAsync(
		string deviceId,
		IMessage message,
		MessageType type,
		bool compress = false,
		CancellationToken ct = default)
	{
		var normalized = DeviceIdentity.NormalizeDeviceId(deviceId);
		if ( _connections.TryGetValue(normalized, out var conn) )
		{
			await conn.SendMessageAsync(message, type, compress, ct);
		}
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		var allowedIds = new HashSet<string>(_outboundPeers.Keys, StringComparer.OrdinalIgnoreCase);
		_listener = new TlsListener(_localCert, allowedIds,
			Microsoft.Extensions.Logging.Abstractions.NullLogger<TlsListener>.Instance, _listenPort);
		_listener.Start();

		_ = Task.Run(() => AcceptLoopAsync(_cts.Token), _cts.Token);
		_ = Task.Run(() => KeepaliveLoopAsync(_cts.Token), _cts.Token);

		foreach ( var (deviceId, addresses) in _outboundPeers )
		{
			_ = Task.Run(() => OutboundLoopAsync(deviceId, addresses, _cts.Token), _cts.Token);
		}

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_cts.Cancel();
		_listener?.Stop();
		return Task.CompletedTask;
	}

	// ── Internal loops ────────────────────────────────────────────────────────

	private async Task AcceptLoopAsync(CancellationToken ct)
	{
		while ( !ct.IsCancellationRequested )
		{
			try
			{
				var ssl = await _listener!.AcceptAsync(ct);
				if ( ssl is null ) continue;
				_ = Task.Run(() => HandshakeAndReadLoopAsync(ssl, ct: ct), ct);
			}
			catch ( OperationCanceledException ) { break; }
			catch ( Exception ex )
			{
				_logger.LogWarning(ex, "Accept loop error.");
				await Task.Delay(1000, ct).ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	/// Injects a pre-established BEP stream (e.g. from openssl s_client on macOS) as if it
	/// were a normally dialed connection. Performs the Hello exchange, registers the connection,
	/// fires <see cref="OnPeerConnected"/>, and starts the message read loop.
	/// </summary>
	public Task InjectConnectionAsync(
		Stream bepStream,
		string peerDeviceId,
		byte[] peerDeviceIdBytes,
		CancellationToken ct = default)
	{
		var normalized = DeviceIdentity.NormalizeDeviceId(peerDeviceId);
		_ = Task.Run(
			() => HandshakeAndReadLoopAsync(bepStream, knownDeviceId: normalized, knownDeviceIdBytes: peerDeviceIdBytes, ct: ct),
			ct);
		return Task.CompletedTask;
	}

	private async Task OutboundLoopAsync(string deviceId, string[] addresses, CancellationToken ct)
	{
		var delay = MinReconnectDelay;
		while ( !ct.IsCancellationRequested )
		{
			if ( _connections.ContainsKey(deviceId) )
			{
				await Task.Delay(PingCheckInterval, ct).ConfigureAwait(false);
				continue;
			}

			foreach ( var address in addresses )
			{
				if ( ct.IsCancellationRequested ) return;
				try
				{
					var (host, port) = ParseAddress(address);
					var allowedIds = new HashSet<string> { deviceId };
					var dialer = new TlsDialer(_localCert, allowedIds);
					var ssl = await dialer.ConnectAsync(host, port, ct);
					_ = Task.Run(() => HandshakeAndReadLoopAsync(ssl, knownDeviceId: deviceId, ct: ct), ct);
					delay = MinReconnectDelay;
					break;
				}
				catch ( Exception ex ) when ( ex is not OperationCanceledException )
				{
					_logger.LogDebug(ex, "Failed to connect to {Address}.", address);
				}
			}

			await Task.Delay(delay, ct).ConfigureAwait(false);
			delay = delay * 2 < MaxReconnectDelay ? delay * 2 : MaxReconnectDelay;
		}
	}

	private async Task HandshakeAndReadLoopAsync(
		Stream stream,
		CancellationToken ct,
		string? knownDeviceId = null,
		byte[]? knownDeviceIdBytes = null)
	{
		var writer = new BepWriter(stream);
		var reader = new BepReader(stream);
		string? deviceId = null;

		try
		{
			// Exchange Hello
			var hello = new Hello
			{
				DeviceName = Environment.MachineName,
				ClientName = "starsky-connect",
				ClientVersion = "v0.9.5",
			};
			await writer.WriteHelloAsync(hello, ct);
			var remoteHello = await reader.ReadHelloAsync(ct);

			byte[] deviceIdBytes;
			if ( knownDeviceId is not null && knownDeviceIdBytes is not null )
			{
				// Pre-authenticated stream (e.g. openssl s_client): device ID was derived by caller
				deviceId = knownDeviceId;
				deviceIdBytes = knownDeviceIdBytes;
			}
			else
			{
				// SslStream: derive device ID from the peer's TLS certificate
				var ssl = (SslStream)stream;
				var peerCert = ssl.RemoteCertificate as X509Certificate2
				               ?? new X509Certificate2(ssl.RemoteCertificate!);
				deviceId = DeviceIdentity.NormalizeDeviceId(DeviceIdentity.DeriveDeviceId(peerCert));
				deviceIdBytes = DeviceIdentity.DeriveDeviceIdBytes(peerCert);
			}

			var conn = new BepConnection(deviceId, deviceIdBytes, stream);
			_connections[deviceId] = conn;

			_logger.LogInformation("Connected to {DeviceId} ({Name}).", deviceId, remoteHello.DeviceName);

			if ( OnPeerConnected is not null )
			{
				await OnPeerConnected(deviceId, remoteHello, ct);
			}

			// Read loop
			while ( !ct.IsCancellationRequested )
			{
				var msg = await conn.ReadMessageAsync(ct);
				if ( OnMessageReceived is not null )
				{
					await OnMessageReceived(deviceId, msg, ct);
				}
			}
		}
		catch ( OperationCanceledException ) { }
		catch ( EndOfStreamException ex )
		{
			_logger.LogInformation(ex, "Peer closed connection.");
			await FireDisconnectedAsync(deviceId ?? knownDeviceId, ex, ct);
		}
		catch ( Exception ex )
		{
			_logger.LogWarning(ex, "Connection error.");
			await FireDisconnectedAsync(deviceId ?? knownDeviceId, ex, ct);
		}
		finally
		{
			if ( deviceId is not null )
			{
				_connections.TryRemove(deviceId, out _);
			}

			try { await stream.DisposeAsync(); } catch { /* best effort */ }
		}
	}

	private async Task KeepaliveLoopAsync(CancellationToken ct)
	{
		while ( !ct.IsCancellationRequested )
		{
			try
			{
				await Task.Delay(PingCheckInterval, ct);
			}
			catch ( OperationCanceledException ) { break; }

			var now = DateTimeOffset.UtcNow;
			foreach ( var (deviceId, conn) in _connections )
			{
				if ( now - conn.LastReceivedUtc > ReceiveTimeout )
				{
					_logger.LogWarning("Receive timeout for {DeviceId}, closing.", deviceId);
					_connections.TryRemove(deviceId, out _);
					await conn.DisposeAsync();
					continue;
				}

				if ( now - conn.LastSentUtc > PingSendInterval )
				{
					try
					{
						await conn.SendPingAsync(ct);
					}
					catch ( Exception ex )
					{
						_logger.LogWarning(ex, "Ping failed for {DeviceId}.", deviceId);
					}
				}
			}
		}
	}

	private async Task FireDisconnectedAsync(string? deviceId, Exception? ex, CancellationToken ct)
	{
		if ( deviceId is not null && OnPeerDisconnected is not null )
		{
			await OnPeerDisconnected(deviceId, ex, ct);
		}
	}

	private static (string host, int port) ParseAddress(string address)
	{
		// Expects "tcp://host:port" or "host:port"
		var uri = address.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase)
			? new Uri(address)
			: new Uri("tcp://" + address);
		return (uri.Host, uri.Port > 0 ? uri.Port : 22000);
	}

	public void Dispose()
	{
		_cts.Cancel();
		_cts.Dispose();
		_listener?.Dispose();
		foreach ( var conn in _connections.Values )
		{
			conn.Dispose();
		}
	}
}
