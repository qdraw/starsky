using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using starsky.foundation.connect.Protocol.Generated;

namespace starsky.foundation.connect.Protocol;

/// <summary>
/// Writes BEP v1 frames to a stream.
/// </summary>
public sealed class BepWriter
{
	private const uint HelloMagic = 0x2EA7D90Bu;

	private readonly Stream _stream;

	/// <summary>
	/// Message types whose bodies SHOULD be LZ4-compressed when the peer supports it.
	/// </summary>
	private static readonly MessageType[] CompressibleTypes =
	[
		MessageType.Index,
		MessageType.IndexUpdate,
		MessageType.Response,
	];

	public BepWriter(Stream stream)
	{
		_stream = stream;
	}

	/// <summary>
	/// Writes the pre-authentication Hello frame.
	/// </summary>
	public async Task WriteHelloAsync(Hello hello, CancellationToken ct = default)
	{
		var payload = hello.ToByteArray();

		// magic (4) + length (2) + payload
		var frame = new byte[6 + payload.Length];
		BinaryPrimitives.WriteUInt32BigEndian(frame, HelloMagic);
		BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(4), ( ushort )payload.Length);
		payload.CopyTo(frame, 6);

		await _stream.WriteAsync(frame, ct);
		await _stream.FlushAsync(ct);
	}

	/// <summary>
	/// Writes a post-authentication BEP message frame.
	/// </summary>
	/// <param name="message">The protobuf message to send.</param>
	/// <param name="type">The BEP message type identifier.</param>
	/// <param name="compress">When true and the message type supports it, apply LZ4 compression.</param>
	public async Task WriteMessageAsync(
		IMessage message,
		MessageType type,
		bool compress = false,
		CancellationToken ct = default)
	{
		var body = message.ToByteArray();
		var useCompression = compress && Array.IndexOf(CompressibleTypes, type) >= 0;

		var compression = useCompression ? MessageCompression.Lz4 : MessageCompression.None;
		var header = new Header { Type = type, Compression = compression };
		var headerBytes = header.ToByteArray();

		var bodyBytes = useCompression ? Compression.Compress(body) : body;

		// header length (2) + header + body length (4) + body
		var frame = new byte[2 + headerBytes.Length + 4 + bodyBytes.Length];
		var span = frame.AsSpan();

		BinaryPrimitives.WriteUInt16BigEndian(span, ( ushort )headerBytes.Length);
		headerBytes.CopyTo(span[2..]);
		BinaryPrimitives.WriteUInt32BigEndian(span[( 2 + headerBytes.Length )..], ( uint )bodyBytes.Length);
		bodyBytes.CopyTo(span[( 2 + headerBytes.Length + 4 )..]);

		await _stream.WriteAsync(frame, ct);
		await _stream.FlushAsync(ct);
	}
}
