using System;
using System.Collections.Generic;

namespace starsky.benchmarks.rawdng;

internal static class MinimalDngFactory
{
	private const uint IfdOffset = 8;
	private const ushort CfaPhotometric = 32803;
	private const uint AsShotNeutralOffset = 192;
	private const uint BlackLevelOffset = 216;
	private const uint WhiteLevelOffset = 248;

	public static byte[] CreateUncompressedCfaDng(int width, int height, ushort bitsPerSample,
		bool packed = false)
	{
		if ( width <= 0 || height <= 0 )
		{
			throw new ArgumentOutOfRangeException(nameof(width));
		}

		var pixelCount = checked(width * height);
		var rawPayload = CreateRawPayload(bitsPerSample, pixelCount, packed);
		var rawDataOffset = 320u;
		var bytes = new byte[rawDataOffset + rawPayload.Length];

		WriteHeader(bytes);
		WriteIfd(bytes, width, height, bitsPerSample, rawDataOffset, rawPayload.Length);
		WriteMetadataArrays(bytes);
		Buffer.BlockCopy(rawPayload, 0, bytes, ( int ) rawDataOffset, rawPayload.Length);
		return bytes;
	}

	private static byte[] CreateRawPayload(ushort bitsPerSample, int pixelCount, bool packed)
	{
		return bitsPerSample switch
		{
			8 => Create8Bit(pixelCount),
			16 => Create16Bit(pixelCount),
			10 or 12 or 14 when packed => PackLittleEndian(bitsPerSample, pixelCount),
			10 or 12 or 14 => CreateWordStored(bitsPerSample, pixelCount),
			_ => throw new NotSupportedException($"Unsupported bits per sample for benchmark data: {bitsPerSample}")
		};
	}

	private static byte[] Create8Bit(int pixelCount)
	{
		var bytes = new byte[pixelCount];
		for ( var i = 0; i < bytes.Length; i++ )
		{
			bytes[i] = ( byte ) ( i & 0xFF );
		}

		return bytes;
	}

	private static byte[] Create16Bit(int pixelCount)
	{
		var bytes = new byte[pixelCount * 2];
		for ( var i = 0; i < pixelCount; i++ )
		{
			var value = ( ushort ) ( i & 0x3FFF );
			var index = i * 2;
			bytes[index] = ( byte ) ( value & 0xFF );
			bytes[index + 1] = ( byte ) ( value >> 8 );
		}

		return bytes;
	}

	private static byte[] CreateWordStored(ushort bitsPerSample, int pixelCount)
	{
		var bytes = new byte[pixelCount * 2];
		var mask = ( 1 << bitsPerSample ) - 1;
		for ( var i = 0; i < pixelCount; i++ )
		{
			var value = ( ushort ) ( i & mask );
			var index = i * 2;
			bytes[index] = ( byte ) ( value & 0xFF );
			bytes[index + 1] = ( byte ) ( value >> 8 );
		}

		return bytes;
	}

	private static byte[] PackLittleEndian(ushort bitsPerSample, int pixelCount)
	{
		var totalBits = pixelCount * bitsPerSample;
		var packed = new byte[( totalBits + 7 ) / 8];
		var mask = ( 1 << bitsPerSample ) - 1;
		var bitIndex = 0;
		for ( var i = 0; i < pixelCount; i++ )
		{
			var value = i & mask;
			for ( var b = 0; b < bitsPerSample; b++ )
			{
				if ( ( ( value >> b ) & 1 ) == 0 )
				{
					continue;
				}

				var absoluteBit = bitIndex + b;
				packed[absoluteBit / 8] |= ( byte ) ( 1 << ( absoluteBit & 7 ) );
			}

			bitIndex += bitsPerSample;
		}

		return packed;
	}

	private static void WriteHeader(byte[] data)
	{
		data[0] = ( byte ) 'I';
		data[1] = ( byte ) 'I';
		WriteU16(data, 2, 42);
		WriteU32(data, 4, IfdOffset);
	}

	private static void WriteIfd(byte[] data, int width, int height, ushort bitsPerSample,
		uint rawDataOffset, int stripByteCount)
	{
		var entries = new List<(ushort Tag, ushort Type, uint Count, uint Value)>
		{
			(0x0100, 4, 1, ( uint ) width),
			(0x0101, 4, 1, ( uint ) height),
			(0x0102, 3, 1, bitsPerSample),
			(0x0103, 3, 1, 1),
			(0x0106, 3, 1, CfaPhotometric),
			(0x0111, 4, 1, rawDataOffset),
			(0x0115, 3, 1, 1),
			(0x0116, 4, 1, ( uint ) height),
			(0x0117, 4, 1, ( uint ) stripByteCount),
			(0x828D, 3, 2, 0x00020002),
			(0x828E, 1, 4, 0x02010100),
			(0xC61A, 5, 4, BlackLevelOffset),
			(0xC61D, 5, 4, WhiteLevelOffset),
			(0xC628, 5, 3, AsShotNeutralOffset)
		};

		WriteU16(data, ( int ) IfdOffset, ( ushort ) entries.Count);
		var entryOffset = ( int ) IfdOffset + 2;
		for ( var i = 0; i < entries.Count; i++ )
		{
			var e = entries[i];
			WriteIfdEntry(data, entryOffset + i * 12, e.Tag, e.Type, e.Count, e.Value);
		}

		WriteU32(data, entryOffset + entries.Count * 12, 0);
	}

	private static void WriteMetadataArrays(byte[] data)
	{
		WriteRational(data, ( int ) AsShotNeutralOffset + 0, 2, 1);
		WriteRational(data, ( int ) AsShotNeutralOffset + 8, 1, 1);
		WriteRational(data, ( int ) AsShotNeutralOffset + 16, 2, 1);

		for ( var i = 0; i < 4; i++ )
		{
			WriteRational(data, ( int ) BlackLevelOffset + i * 8, 64, 1);
			WriteRational(data, ( int ) WhiteLevelOffset + i * 8, 16383, 1);
		}
	}

	private static void WriteIfdEntry(byte[] data, int offset, ushort tag, ushort type,
		uint count, uint value)
	{
		WriteU16(data, offset, tag);
		WriteU16(data, offset + 2, type);
		WriteU32(data, offset + 4, count);
		WriteU32(data, offset + 8, value);
	}

	private static void WriteU16(byte[] data, int offset, ushort value)
	{
		data[offset] = ( byte ) ( value & 0xFF );
		data[offset + 1] = ( byte ) ( value >> 8 );
	}

	private static void WriteU32(byte[] data, int offset, uint value)
	{
		data[offset] = ( byte ) ( value & 0xFF );
		data[offset + 1] = ( byte ) ( ( value >> 8 ) & 0xFF );
		data[offset + 2] = ( byte ) ( ( value >> 16 ) & 0xFF );
		data[offset + 3] = ( byte ) ( ( value >> 24 ) & 0xFF );
	}

	private static void WriteRational(byte[] data, int offset, uint numerator, uint denominator)
	{
		WriteU32(data, offset, numerator);
		WriteU32(data, offset + 4, denominator);
	}
}

