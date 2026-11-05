using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using starsky.foundation.connect.Protocol.Generated;

namespace starsky.foundation.connect.Protocol;

/// <summary>
/// A single authenticated BEP v1 connection to a remote peer.
/// Wraps the TLS stream with a reader, writer, and last-activity timestamps.
/// </summary>
public sealed class BepConnection : IDisposable, IAsyncDisposable
{
	private readonly Stream _stream;
	private readonly BepReader _reader;
	private readonly BepWriter _writer;
	private readonly SemaphoreSlim _writeLock = new(1, 1);
	private DateTimeOffset _lastSent = DateTimeOffset.UtcNow;
	private DateTimeOffset _lastReceived = DateTimeOffset.UtcNow;

	public string DeviceId { get; }
	public byte[] DeviceIdBytes { get; }

	public DateTimeOffset LastSentUtc => _lastSent;
	public DateTimeOffset LastReceivedUtc => _lastReceived;

	public BepConnection(string deviceId, byte[] deviceIdBytes, Stream stream)
	{
		DeviceId = deviceId;
		DeviceIdBytes = deviceIdBytes;
		_stream = stream;
		_reader = new BepReader(stream);
		_writer = new BepWriter(stream);
	}

	/// <summary>Reads one post-authentication frame, updating LastReceivedUtc.</summary>
	public async Task<BepMessage> ReadMessageAsync(CancellationToken ct = default)
	{
		var msg = await _reader.ReadMessageAsync(ct);
		_lastReceived = DateTimeOffset.UtcNow;
		return msg;
	}

	/// <summary>Sends a post-authentication frame, updating LastSentUtc. Thread-safe.</summary>
	public async Task SendMessageAsync(
		IMessage message,
		MessageType type,
		bool compress = false,
		CancellationToken ct = default)
	{
		await _writeLock.WaitAsync(ct);
		try
		{
			await _writer.WriteMessageAsync(message, type, compress, ct);
			_lastSent = DateTimeOffset.UtcNow;
		}
		finally
		{
			_writeLock.Release();
		}
	}

	/// <summary>Sends a Ping frame, updating LastSentUtc. Thread-safe.</summary>
	public Task SendPingAsync(CancellationToken ct = default) =>
		SendMessageAsync(new Ping(), MessageType.Ping, compress: false, ct);

	public void Dispose()
	{
		_writeLock.Dispose();
		_stream.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		_writeLock.Dispose();
		await _stream.DisposeAsync();
	}
}
