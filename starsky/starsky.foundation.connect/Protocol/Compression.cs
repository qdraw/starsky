using System;
using System.Buffers.Binary;
using K4os.Compression.LZ4;

namespace starsky.foundation.connect.Protocol;

/// <summary>
/// LZ4 encode/decode for BEP v1 message bodies.
/// Wire format: uint32 (big-endian) uncompressed length, followed by an LZ4 block.
/// </summary>
public static class Compression
{
	/// <summary>Maximum uncompressed size accepted without throwing.</summary>
	private const int MaxMessageBytes = 500_000_000;

	/// <summary>
	/// Compresses <paramref name="data"/> and prepends the 4-byte big-endian uncompressed length.
	/// </summary>
	public static byte[] Compress(ReadOnlySpan<byte> data)
	{
		if ( data.Length > MaxMessageBytes )
		{
			throw new BepProtocolException(
				$"Message body ({data.Length} bytes) exceeds the 500 MB limit.");
		}

		var maxCompressedLength = LZ4Codec.MaximumOutputSize(data.Length);
		var compressed = new byte[maxCompressedLength];

		var compressedLength = LZ4Codec.Encode(
			data, compressed.AsSpan(), LZ4Level.L00_FAST);

		var result = new byte[4 + compressedLength];
		BinaryPrimitives.WriteUInt32BigEndian(result, ( uint )data.Length);
		compressed.AsSpan(0, compressedLength).CopyTo(result.AsSpan(4));
		return result;
	}

	/// <summary>
	/// Decompresses an LZ4 block produced by <see cref="Compress"/>.
	/// Expects the 4-byte big-endian uncompressed length prefix.
	/// </summary>
	public static byte[] Decompress(ReadOnlySpan<byte> compressedWithPrefix)
	{
		if ( compressedWithPrefix.Length < 4 )
		{
			throw new BepProtocolException("LZ4 data is too short to contain the length prefix.");
		}

		var rawLength = BinaryPrimitives.ReadUInt32BigEndian(compressedWithPrefix);

		if ( rawLength > MaxMessageBytes )
		{
			throw new BepProtocolException(
				$"Declared uncompressed length ({rawLength} bytes) exceeds the 500 MB limit.");
		}

		var uncompressedLength = ( int )rawLength;
		var output = new byte[uncompressedLength];
		var decoded = LZ4Codec.Decode(
			compressedWithPrefix[4..], output.AsSpan());

		if ( decoded != uncompressedLength )
		{
			throw new BepProtocolException(
				$"LZ4 decompressed {decoded} bytes but expected {uncompressedLength}.");
		}

		return output;
	}
}
