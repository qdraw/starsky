using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace starsky.foundation.connect.Relay;

/// <summary>
/// Client for the Syncthing relay protocol v1.
/// Wire format: 4-byte magic (0x9E79BC40) + int32 type + int32 length + XDR payload.
/// ALPN: "bep-relay". Max payload: 1024 bytes.
/// </summary>
public sealed class RelayClient : IDisposable
{
	private const uint RelayMagic = 0x9E79BC40u;
	private const int MaxPayload = 1024;
	private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(2);

	// Message type identifiers
	private const int TypePing = 0;
	private const int TypePong = 1;
	private const int TypeJoinRelayRequest = 2;
	private const int TypeJoinSessionRequest = 3;
	private const int TypeResponse = 4;
	private const int TypeConnectRequest = 5;
	private const int TypeSessionInvitation = 6;
	private const int TypeRelayFull = 7;

	private const int ResponseSuccess = 0;

	private readonly Uri _relayUri;
	private readonly X509Certificate2 _localCert;
	private readonly ILogger<RelayClient> _logger;

	/// <summary>
	/// Raised when the relay server sends a SessionInvitation.
	/// The tuple is (peerDeviceId, key, address, port, isServer).
	/// </summary>
	public Func<byte[], byte[], string, int, bool, Task>? OnSessionInvitation { get; set; }

	public RelayClient(Uri relayUri, X509Certificate2 localCert, ILogger<RelayClient> logger)
	{
		_relayUri = relayUri;
		_localCert = localCert;
		_logger = logger;
	}

	/// <summary>
	/// Connects to the relay server, joins as a client, and starts the message loop.
	/// Returns when the connection is closed or <paramref name="ct"/> is cancelled.
	/// </summary>
	public async Task RunAsync(CancellationToken ct)
	{
		var host = _relayUri.Host;
		var port = _relayUri.Port > 0 ? _relayUri.Port : 22067;
		var token = System.Web.HttpUtility.ParseQueryString(_relayUri.Query)["token"] ?? string.Empty;

		using var tcp = new TcpClient();
		await tcp.ConnectAsync(host, port, ct);

		await using var ssl = new SslStream(tcp.GetStream(), leaveInnerStreamOpen: false);
		var options = new SslClientAuthenticationOptions
		{
			TargetHost = host,
			ApplicationProtocols = [new SslApplicationProtocol("bep-relay")],
			ClientCertificates = new X509CertificateCollection { _localCert },
			EnabledSslProtocols = SslProtocols.Tls13,
			CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
			RemoteCertificateValidationCallback = (_, _, _, _) => true,
		};
		await ssl.AuthenticateAsClientAsync(options, ct);

		// Send JoinRelayRequest
		await SendMessageAsync(ssl, TypeJoinRelayRequest, XdrWriteString(token), ct);

		// Await Response
		var (msgType, payload) = await ReadMessageAsync(ssl, ct);
		if ( msgType == TypeRelayFull )
		{
			_logger.LogWarning("Relay server is full.");
			return;
		}

		if ( msgType != TypeResponse )
		{
			_logger.LogWarning("Unexpected relay message type {Type} after JoinRelayRequest.", msgType);
			return;
		}

		var (code, _) = XdrReadResponse(payload);
		if ( code != ResponseSuccess )
		{
			_logger.LogWarning("Relay join rejected with code {Code}.", code);
			return;
		}

		_logger.LogInformation("Joined relay {Relay}.", _relayUri);

		// Message loop with idle timeout
		while ( !ct.IsCancellationRequested )
		{
			using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			timeoutCts.CancelAfter(IdleTimeout);

			try
			{
				var (type, body) = await ReadMessageAsync(ssl, timeoutCts.Token);
				await DispatchAsync(type, body, ct);
			}
			catch ( OperationCanceledException ) when ( !ct.IsCancellationRequested )
			{
				_logger.LogInformation("Relay idle timeout, disconnecting.");
				break;
			}
		}
	}

	private async Task DispatchAsync(int type, byte[] payload, CancellationToken ct)
	{
		switch ( type )
		{
			case TypePing:
				// Relay uses one-way ping; no pong required
				break;

			case TypeSessionInvitation when OnSessionInvitation is not null:
			{
				var (deviceId, key, address, port, isServer) = XdrReadSessionInvitation(payload);
				await OnSessionInvitation(deviceId, key, address, port, isServer);
				break;
			}

			default:
				_logger.LogDebug("Unhandled relay message type {Type}.", type);
				break;
		}
	}

	// ── XDR helpers ──────────────────────────────────────────────────────────

	private static async Task SendMessageAsync(Stream stream, int type, byte[] payload, CancellationToken ct)
	{
		var frame = new byte[12 + payload.Length];
		BinaryPrimitives.WriteUInt32BigEndian(frame.AsSpan(0), RelayMagic);
		BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(4), type);
		BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(8), payload.Length);
		payload.CopyTo(frame, 12);
		await stream.WriteAsync(frame, ct);
		await stream.FlushAsync(ct);
	}

	private static async Task<(int type, byte[] payload)> ReadMessageAsync(Stream stream, CancellationToken ct)
	{
		var header = new byte[12];
		await stream.ReadExactlyAsync(header, ct);

		var magic = BinaryPrimitives.ReadUInt32BigEndian(header);
		if ( magic != RelayMagic )
		{
			throw new InvalidOperationException($"Invalid relay magic: 0x{magic:X8}");
		}

		var type = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(4));
		var length = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(8));

		if ( length < 0 || length > MaxPayload )
		{
			throw new InvalidOperationException($"Relay payload length {length} out of range.");
		}

		var payload = new byte[length];
		if ( length > 0 )
		{
			await stream.ReadExactlyAsync(payload, ct);
		}

		return (type, payload);
	}

	/// <summary>Encodes a string as XDR: 4-byte big-endian length + UTF-8 bytes + padding.</summary>
	private static byte[] XdrWriteString(string value)
	{
		var bytes = Encoding.UTF8.GetBytes(value);
		var padded = (bytes.Length + 3) & ~3;
		var result = new byte[4 + padded];
		BinaryPrimitives.WriteInt32BigEndian(result, bytes.Length);
		bytes.CopyTo(result, 4);
		return result;
	}

	private static (int code, string message) XdrReadResponse(byte[] payload)
	{
		if ( payload.Length < 8 ) return (ResponseSuccess, string.Empty);
		var offset = 0;
		var code = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(offset)); offset += 4;
		var msgLen = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(offset)); offset += 4;
		var msg = msgLen > 0 && offset + msgLen <= payload.Length
			? Encoding.UTF8.GetString(payload, offset, msgLen)
			: string.Empty;
		return (code, msg);
	}

	private static (byte[] deviceId, byte[] key, string address, int port, bool isServer)
		XdrReadSessionInvitation(byte[] payload)
	{
		var offset = 0;

		byte[] ReadBytes()
		{
			var len = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(offset)); offset += 4;
			var padded = (len + 3) & ~3;
			var data = payload[offset..(offset + len)]; offset += padded;
			return data;
		}

		var deviceId = ReadBytes();
		var key = ReadBytes();
		var addrBytes = ReadBytes();
		var address = Encoding.UTF8.GetString(addrBytes);
		var port = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(offset)); offset += 4;
		var isServer = BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(offset)) != 0;

		return (deviceId, key, address, port, isServer);
	}

	public void Dispose() { }
}
