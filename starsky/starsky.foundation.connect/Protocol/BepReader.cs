using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using starsky.foundation.connect.Protocol.Generated;

namespace starsky.foundation.connect.Protocol;

/// <summary>
/// Reads BEP v1 frames from a stream.
/// </summary>
public sealed class BepReader
{
	private const uint HelloMagic = 0x2EA7D90Bu;
	private const int MaxMessageBytes = 500_000_000;

	private readonly Stream _stream;

	public BepReader(Stream stream)
	{
		_stream = stream;
	}

	/// <summary>
	/// Reads and parses the pre-authentication Hello frame.
	/// Must be called before any post-auth messages.
	/// </summary>
	public async Task<Hello> ReadHelloAsync(CancellationToken ct = default)
	{
		// 4-byte magic
		var magicBytes = new byte[4];
		await _stream.ReadExactlyAsync(magicBytes, ct);
		var magic = BinaryPrimitives.ReadUInt32BigEndian(magicBytes);
		if ( magic != HelloMagic )
		{
			throw new BepProtocolException(
				$"Invalid Hello magic: expected 0x{HelloMagic:X8}, got 0x{magic:X8}.");
		}

		// 2-byte length
		var lengthBytes = new byte[2];
		await _stream.ReadExactlyAsync(lengthBytes, ct);
		var length = BinaryPrimitives.ReadUInt16BigEndian(lengthBytes);

		var payload = new byte[length];
		await _stream.ReadExactlyAsync(payload, ct);

		return Hello.Parser.ParseFrom(payload);
	}

	/// <summary>
	/// Reads one post-authentication BEP message frame.
	/// Returns a <see cref="BepMessage"/> containing the parsed header and raw (decompressed) body.
	/// </summary>
	public async Task<BepMessage> ReadMessageAsync(CancellationToken ct = default)
	{
		// 2-byte header length
		var hdrLenBytes = new byte[2];
		await _stream.ReadExactlyAsync(hdrLenBytes, ct);
		var hdrLen = BinaryPrimitives.ReadUInt16BigEndian(hdrLenBytes);

		var headerBytes = new byte[hdrLen];
		await _stream.ReadExactlyAsync(headerBytes, ct);
		var header = Header.Parser.ParseFrom(headerBytes);

		// 4-byte body length
		var bodyLenBytes = new byte[4];
		await _stream.ReadExactlyAsync(bodyLenBytes, ct);
		var bodyLen = BinaryPrimitives.ReadUInt32BigEndian(bodyLenBytes);

		if ( bodyLen > MaxMessageBytes )
		{
			throw new BepProtocolException(
				$"Message body length {bodyLen} exceeds the 500 MB limit.");
		}

		var body = new byte[bodyLen];
		await _stream.ReadExactlyAsync(body, ct);

		var messageBody = header.Compression == MessageCompression.Lz4
			? Compression.Decompress(body)
			: body;

		return new BepMessage(header, messageBody);
	}
}

/// <summary>
/// A parsed BEP post-authentication message frame (header + raw protobuf body bytes).
/// </summary>
public sealed class BepMessage(Header header, byte[] body)
{
	public Header Header { get; } = header;

	/// <summary>Raw protobuf bytes of the message body (already decompressed if needed).</summary>
	public byte[] Body { get; } = body;

	public T ParseBody<T>(MessageParser<T> parser) where T : IMessage<T>
	{
		return parser.ParseFrom(Body);
	}
}
