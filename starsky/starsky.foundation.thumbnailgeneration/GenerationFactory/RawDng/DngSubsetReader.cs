using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal sealed class DngRawImage
{
	public required ushort[,] Bayer { get; init; }
	public required int Width { get; init; }
	public required int Height { get; init; }
	public required int BitsPerSample { get; init; }
	public required float BlackLevel { get; init; }
	public required float WhiteLevel { get; init; }
	public required float[] AsShotNeutral { get; init; }
	public required float[,] ColorMatrix1 { get; init; }
	public required byte[] CfaPattern { get; init; }
}

internal static class DngSubsetReader
{
	private const ushort TagSubIfds = 0x014A;
	private const ushort TagImageWidth = 0x0100;
	private const ushort TagImageLength = 0x0101;
	private const ushort TagBitsPerSample = 0x0102;
	private const ushort TagCompression = 0x0103;
	private const ushort TagPhotometricInterpretation = 0x0106;
	private const ushort TagStripOffsets = 0x0111;
	private const ushort TagSamplesPerPixel = 0x0115;
	private const ushort TagRowsPerStrip = 0x0116;
	private const ushort TagStripByteCounts = 0x0117;
	private const ushort TagPredictor = 0x013D;
	private const ushort TagCfaRepeatPatternDim = 0x828D;
	private const ushort TagCfaPattern = 0x828E;
	private const ushort TagBlackLevel = 0xC61A;
	private const ushort TagWhiteLevel = 0xC61D;
	private const ushort TagAsShotNeutral = 0xC628;
	private const ushort TagColorMatrix1 = 0xC621;

	private const ushort CompressionUncompressed = 1;
	private const ushort CompressionDeflate = 8;
	private const ushort CompressionAdobeDeflate = 32946;
	private const ushort PhotometricCfa = 32803;

	public static bool TryLoad(Stream input, out DngRawImage? image, out string error)
	{
		image = null;
		error = string.Empty;

		if ( !TryParseHeader(input, out var littleEndian, out var firstIfdOffset) )
		{
			error = "Invalid TIFF/DNG header";
			return false;
		}

		var ifd0 = ReadIfd(input, firstIfdOffset, littleEndian);
		if ( ifd0 == null )
		{
			error = "Failed to read IFD0";
			return false;
		}

		var rawIfd = ResolveRawIfd(input, littleEndian, ifd0) ?? ifd0;
		if ( !TryBuildRawImage(input, littleEndian, rawIfd, out image, out error) )
		{
			return false;
		}

		return true;
	}

	private static IfdDirectory? ResolveRawIfd(Stream input, bool littleEndian, IfdDirectory ifd0)
	{
		if ( TryGetUnsignedArray(input, littleEndian, ifd0, TagSubIfds, out var subIfdOffsets) )
		{
			foreach ( var offset in subIfdOffsets )
			{
				var sub = ReadIfd(input, offset, littleEndian);
				if ( sub != null && IsRawIfd(input, littleEndian, sub) )
				{
					return sub;
				}
			}
		}

		return IsRawIfd(input, littleEndian, ifd0) ? ifd0 : null;
	}

	private static bool IsRawIfd(Stream input, bool littleEndian, IfdDirectory ifd)
	{
		if ( !TryGetUnsigned(input, littleEndian, ifd, TagPhotometricInterpretation,
			    out var photometric) )
		{
			return false;
		}

		return photometric == PhotometricCfa;
	}

	private static bool TryBuildRawImage(Stream input, bool littleEndian, IfdDirectory ifd,
		out DngRawImage? image, out string error)
	{
		image = null;
		error = string.Empty;

		if ( !TryGetUnsigned(input, littleEndian, ifd, TagImageWidth, out var widthU) ||
		     !TryGetUnsigned(input, littleEndian, ifd, TagImageLength, out var heightU) ||
		     !TryGetUnsigned(input, littleEndian, ifd, TagBitsPerSample, out var bitsPerSampleU) )
		{
			error = "Missing width/height/bits metadata";
			return false;
		}

		if ( !TryGetUnsigned(input, littleEndian, ifd, TagCompression, out var compression) )
		{
			error = "Only uncompressed DNG is supported in the subset reader";
			return false;
		}

		if ( compression is not ( CompressionUncompressed or CompressionDeflate or CompressionAdobeDeflate ) )
		{
			error = "Only uncompressed DNG is supported in the subset reader";
			return false;
		}

		if ( !TryGetUnsignedArray(input, littleEndian, ifd, TagStripOffsets, out var stripOffsets) ||
		     !TryGetUnsignedArray(input, littleEndian, ifd, TagStripByteCounts,
			     out var stripByteCounts) ||
		     stripOffsets.Length == 0 || stripByteCounts.Length == 0 )
		{
			error = "Missing strip data pointers";
			return false;
		}

		if ( stripOffsets.Length != stripByteCounts.Length )
		{
			error = "Strip offsets/counts mismatch";
			return false;
		}

		var width = ( int ) widthU;
		var height = ( int ) heightU;
		var bitsPerSample = ( int ) bitsPerSampleU;
		var rowsPerStrip = TryGetUnsigned(input, littleEndian, ifd, TagRowsPerStrip, out var rowsU)
			? ( int ) rowsU
			: height;
		var predictor = TryGetUnsigned(input, littleEndian, ifd, TagPredictor, out var predictorU)
			? ( int ) predictorU
			: 1;

		if ( bitsPerSample is not ( 8 or 12 or 14 or 16 ) )
		{
			error = $"Unsupported bits per sample: {bitsPerSample}";
			return false;
		}

		if ( predictor != 1 )
		{
			error = $"Unsupported predictor: {predictor}";
			return false;
		}

		var bayer = new ushort[height, width];
		var pixelCount = width * height;
		if ( !TryReadPixels(input, littleEndian, stripOffsets, stripByteCounts, bitsPerSample,
			    width, height, rowsPerStrip, ( ushort ) compression, bayer) )
		{
			error = "Failed to decode strip payload";
			return false;
		}

		var blackLevel = TryGetFloat(input, littleEndian, ifd, TagBlackLevel, out var black)
			? black
			: 0f;
		var whiteLevel = TryGetFloat(input, littleEndian, ifd, TagWhiteLevel, out var white)
			? white
			: ( ( 1 << Math.Min(16, bitsPerSample) ) - 1 );
		var asShotNeutral = TryGetFloatArray(input, littleEndian, ifd, TagAsShotNeutral,
			out var neutral) && neutral.Length >= 3
			? neutral.Take(3).ToArray()
			: [1f, 1f, 1f];
		var colorMatrix = TryGetFloatArray(input, littleEndian, ifd, TagColorMatrix1,
			out var matrixValues) && matrixValues.Length >= 9
			? To3x3(matrixValues)
			: Identity3x3();
		var cfaPattern = TryGetByteArray(input, littleEndian, ifd, TagCfaPattern, out var cfa) &&
		                 cfa.Length >= 4
			? cfa.Take(4).ToArray()
			: new byte[] { 0, 1, 1, 2 }; // fallback RGGB

		image = new DngRawImage
		{
			Bayer = bayer,
			Width = width,
			Height = height,
			BitsPerSample = bitsPerSample,
			BlackLevel = blackLevel,
			WhiteLevel = whiteLevel,
			AsShotNeutral = asShotNeutral,
			ColorMatrix1 = colorMatrix,
			CfaPattern = cfaPattern
		};

		return true;
	}

	private static bool TryReadPixels(Stream input, bool littleEndian, IReadOnlyList<uint> offsets,
		IReadOnlyList<uint> counts, int bitsPerSample, int width, int height, int rowsPerStrip,
		ushort compression, ushort[,] bayer)
	{
		if ( width <= 0 || height <= 0 || rowsPerStrip <= 0 )
		{
			return false;
		}

		var rowCursor = 0;
		for ( var i = 0; i < offsets.Count && rowCursor < height; i++ )
		{
			var count = ( int ) counts[i];
			if ( count <= 0 )
			{
				return false;
			}

			var encoded = new byte[count];
			if ( !TrySeekAndRead(input, offsets[i], encoded, 0, count) )
			{
				return false;
			}

			var payload = compression switch
			{
				CompressionUncompressed => encoded,
				CompressionDeflate or CompressionAdobeDeflate => Inflate(encoded),
				_ => null
			};

			if ( payload == null )
			{
				return false;
			}

			var rowsInStrip = Math.Min(rowsPerStrip, height - rowCursor);
			var stripPixelCount = checked(rowsInStrip * width);
			var decoded = DecodePixels(payload, littleEndian, bitsPerSample, stripPixelCount);
			if ( decoded == null )
			{
				return false;
			}

			for ( var p = 0; p < decoded.Length; p++ )
			{
				var y = rowCursor + p / width;
				var x = p % width;
				bayer[y, x] = decoded[p];
			}

			rowCursor += rowsInStrip;
		}

		return rowCursor == height;
	}

	private static byte[]? Inflate(byte[] compressed)
	{
		try
		{
			using var input = new MemoryStream(compressed);
			using var zlib = new ZLibStream(input, CompressionMode.Decompress, leaveOpen: false);
			using var output = new MemoryStream();
			zlib.CopyTo(output);
			return output.ToArray();
		}
		catch
		{
			try
			{
				using var input = new MemoryStream(compressed);
				using var deflate = new DeflateStream(input, CompressionMode.Decompress,
					leaveOpen: false);
				using var output = new MemoryStream();
				deflate.CopyTo(output);
				return output.ToArray();
			}
			catch
			{
				return null;
			}
		}
	}

	private static ushort[]? DecodePixels(byte[] payload, bool littleEndian, int bitsPerSample,
		int pixelCount)
	{
		if ( bitsPerSample == 8 )
		{
			return Decode8(payload, pixelCount);
		}

		if ( bitsPerSample == 16 )
		{
			return Decode16(payload, littleEndian, pixelCount);
		}

		if ( payload.Length >= pixelCount * 2 )
		{
			return DecodeWordStored(payload, littleEndian, bitsPerSample, pixelCount);
		}

		return littleEndian
			? DecodePackedLittleEndian(payload, bitsPerSample, pixelCount)
			: DecodePackedBigEndian(payload, bitsPerSample, pixelCount);
	}

	private static ushort[]? Decode8(byte[] payload, int pixelCount)
	{
		if ( payload.Length < pixelCount )
		{
			return null;
		}

		var result = new ushort[pixelCount];
		for ( var i = 0; i < pixelCount; i++ )
		{
			result[i] = payload[i];
		}

		return result;
	}

	private static ushort[]? Decode16(byte[] payload, bool littleEndian, int pixelCount)
	{
		if ( payload.Length < pixelCount * 2 )
		{
			return null;
		}

		var result = new ushort[pixelCount];
		for ( var i = 0; i < pixelCount; i++ )
		{
			var span = payload.AsSpan(i * 2, 2);
			result[i] = littleEndian
				? BinaryPrimitives.ReadUInt16LittleEndian(span)
				: BinaryPrimitives.ReadUInt16BigEndian(span);
		}

		return result;
	}

	private static ushort[]? DecodeWordStored(byte[] payload, bool littleEndian, int bitsPerSample,
		int pixelCount)
	{
		var words = Decode16(payload, littleEndian, pixelCount);
		if ( words == null )
		{
			return null;
		}

		var mask = ( 1u << bitsPerSample ) - 1u;
		for ( var i = 0; i < words.Length; i++ )
		{
			words[i] = ( ushort ) ( words[i] & mask );
		}

		return words;
	}

	private static ushort[]? DecodePackedLittleEndian(byte[] payload, int bitsPerSample,
		int pixelCount)
	{
		var requiredBytes = ( pixelCount * bitsPerSample + 7 ) / 8;
		if ( payload.Length < requiredBytes )
		{
			return null;
		}

		var result = new ushort[pixelCount];
		var bitIndex = 0;
		for ( var i = 0; i < pixelCount; i++ )
		{
			var byteIndex = bitIndex >> 3;
			var bitOffset = bitIndex & 7;
			ulong scratch = 0;
			for ( var b = 0; b < 4 && byteIndex + b < payload.Length; b++ )
			{
				scratch |= ( ulong ) payload[byteIndex + b] << ( b * 8 );
			}

			result[i] = ( ushort ) ( ( scratch >> bitOffset ) & ( ( 1u << bitsPerSample ) - 1u ) );
			bitIndex += bitsPerSample;
		}

		return result;
	}

	private static ushort[]? DecodePackedBigEndian(byte[] payload, int bitsPerSample,
		int pixelCount)
	{
		var requiredBytes = ( pixelCount * bitsPerSample + 7 ) / 8;
		if ( payload.Length < requiredBytes )
		{
			return null;
		}

		var result = new ushort[pixelCount];
		var bitCursor = 0;
		for ( var i = 0; i < pixelCount; i++ )
		{
			uint value = 0;
			for ( var b = 0; b < bitsPerSample; b++ )
			{
				var absoluteBit = bitCursor + b;
				var byteIndex = absoluteBit >> 3;
				var bitInByte = 7 - ( absoluteBit & 7 );
				var bit = ( payload[byteIndex] >> bitInByte ) & 1;
				value = ( value << 1 ) | ( uint ) bit;
			}

			result[i] = ( ushort ) value;
			bitCursor += bitsPerSample;
		}

		return result;
	}

	private static float[,] To3x3(IReadOnlyList<float> v)
	{
		return new[,]
		{
			{ v[0], v[1], v[2] },
			{ v[3], v[4], v[5] },
			{ v[6], v[7], v[8] }
		};
	}

	private static float[,] Identity3x3()
	{
		return new[,]
		{
			{ 1f, 0f, 0f },
			{ 0f, 1f, 0f },
			{ 0f, 0f, 1f }
		};
	}

	private static bool TryParseHeader(Stream input, out bool littleEndian, out uint firstIfd)
	{
		littleEndian = true;
		firstIfd = 0;
		Span<byte> header = stackalloc byte[8];
		if ( input.Read(header) < 8 )
		{
			return false;
		}

		if ( header[0] == 'I' && header[1] == 'I' )
		{
			littleEndian = true;
		}
		else if ( header[0] == 'M' && header[1] == 'M' )
		{
			littleEndian = false;
		}
		else
		{
			return false;
		}

		var magic = ReadUInt16(header[2..], littleEndian);
		if ( magic != 42 )
		{
			return false;
		}

		firstIfd = ReadUInt32(header[4..], littleEndian);
		return firstIfd > 0 && firstIfd < input.Length;
	}

	private static IfdDirectory? ReadIfd(Stream input, uint offset, bool littleEndian)
	{
		if ( !TrySeek(input, offset) )
		{
			return null;
		}

		Span<byte> countBuf = stackalloc byte[2];
		if ( input.Read(countBuf) < 2 )
		{
			return null;
		}

		var count = ReadUInt16(countBuf, littleEndian);
		if ( count == 0 || count > 4096 )
		{
			return null;
		}

		var entries = new Dictionary<ushort, IfdEntry>();
		for ( var i = 0; i < count; i++ )
		{
			Span<byte> e = stackalloc byte[12];
			if ( input.Read(e) < 12 )
			{
				return null;
			}

			var tag = ReadUInt16(e, littleEndian);
			entries[tag] = new IfdEntry
			{
				Type = ReadUInt16(e[2..], littleEndian),
				Count = ReadUInt32(e[4..], littleEndian),
				ValueOrOffset = ReadUInt32(e[8..], littleEndian)
			};
		}

		return new IfdDirectory(entries);
	}

	private static bool TryGetUnsigned(Stream input, bool littleEndian, IfdDirectory ifd,
		ushort tag, out uint value)
	{
		value = 0;
		if ( !ifd.Entries.TryGetValue(tag, out var entry) )
		{
			return false;
		}

		var values = ReadUnsignedValues(input, littleEndian, entry, 1);
		if ( values.Length == 0 )
		{
			return false;
		}

		value = values[0];
		return true;
	}

	private static bool TryGetUnsignedArray(Stream input, bool littleEndian, IfdDirectory ifd,
		ushort tag, out uint[] values)
	{
		values = [];
		if ( !ifd.Entries.TryGetValue(tag, out var entry) )
		{
			return false;
		}

		values = ReadUnsignedValues(input, littleEndian, entry, ( int ) entry.Count);
		return values.Length > 0;
	}

	private static bool TryGetByteArray(Stream input, bool littleEndian, IfdDirectory ifd,
		ushort tag, out byte[] values)
	{
		values = [];
		if ( !ifd.Entries.TryGetValue(tag, out var entry) || entry.Type != 1 )
		{
			return false;
		}

		values = ReadBytes(input, littleEndian, entry);
		return values.Length > 0;
	}

	private static bool TryGetFloat(Stream input, bool littleEndian, IfdDirectory ifd,
		ushort tag, out float value)
	{
		value = 0;
		if ( !TryGetFloatArray(input, littleEndian, ifd, tag, out var values) || values.Length == 0 )
		{
			return false;
		}

		value = values[0];
		return true;
	}

	private static bool TryGetFloatArray(Stream input, bool littleEndian, IfdDirectory ifd,
		ushort tag, out float[] values)
	{
		values = [];
		if ( !ifd.Entries.TryGetValue(tag, out var entry) )
		{
			return false;
		}

		values = entry.Type switch
		{
			3 or 4 => ReadUnsignedValues(input, littleEndian, entry, ( int ) entry.Count)
				.Select(v => ( float ) v).ToArray(),
			5 => ReadRationalValues(input, littleEndian, entry, signed: false),
			10 => ReadRationalValues(input, littleEndian, entry, signed: true),
			_ => []
		};
		return values.Length > 0;
	}

	private static uint[] ReadUnsignedValues(Stream input, bool littleEndian, IfdEntry entry,
		int wanted)
	{
		if ( entry.Type is not ( 3 or 4 ) || entry.Count == 0 )
		{
			return [];
		}

		var typeSize = entry.Type == 3 ? 2 : 4;
		var totalSize = checked(( int ) entry.Count * typeSize);
		var data = ReadRawValueBytes(input, littleEndian, entry, totalSize);
		if ( data.Length < totalSize )
		{
			return [];
		}

		var count = Math.Min(( int ) entry.Count, wanted);
		var result = new uint[count];
		for ( var i = 0; i < count; i++ )
		{
			result[i] = entry.Type == 3
				? ReadUInt16(data.AsSpan(i * 2, 2), littleEndian)
				: ReadUInt32(data.AsSpan(i * 4, 4), littleEndian);
		}

		return result;
	}

	private static float[] ReadRationalValues(Stream input, bool littleEndian, IfdEntry entry,
		bool signed)
	{
		if ( entry.Count == 0 )
		{
			return [];
		}

		var total = checked(( int ) entry.Count * 8);
		if ( !TrySeek(input, entry.ValueOrOffset) )
		{
			return [];
		}

		var buf = new byte[total];
		if ( input.Read(buf, 0, total) < total )
		{
			return [];
		}

		var values = new float[entry.Count];
		for ( var i = 0; i < entry.Count; i++ )
		{
			var pos = i * 8;
			if ( signed )
			{
				var n = ( int ) ReadUInt32(buf.AsSpan(pos, 4), littleEndian);
				var d = ( int ) ReadUInt32(buf.AsSpan(pos + 4, 4), littleEndian);
				values[i] = d == 0 ? 0 : ( float ) n / d;
			}
			else
			{
				var n = ReadUInt32(buf.AsSpan(pos, 4), littleEndian);
				var d = ReadUInt32(buf.AsSpan(pos + 4, 4), littleEndian);
				values[i] = d == 0 ? 0 : ( float ) n / d;
			}
		}

		return values;
	}

	private static byte[] ReadBytes(Stream input, bool littleEndian, IfdEntry entry)
	{
		var total = ( int ) entry.Count;
		if ( total <= 0 )
		{
			return [];
		}

		return ReadRawValueBytes(input, littleEndian, entry, total);
	}

	private static byte[] ReadRawValueBytes(Stream input, bool littleEndian, IfdEntry entry,
		int totalSize)
	{
		if ( totalSize <= 4 )
		{
			Span<byte> inline = stackalloc byte[4];
			if ( littleEndian )
			{
				BinaryPrimitives.WriteUInt32LittleEndian(inline, entry.ValueOrOffset);
			}
			else
			{
				BinaryPrimitives.WriteUInt32BigEndian(inline, entry.ValueOrOffset);
			}

			return inline[..totalSize].ToArray();
		}

		if ( !TrySeek(input, entry.ValueOrOffset) )
		{
			return [];
		}

		var bytes = new byte[totalSize];
		return input.Read(bytes, 0, totalSize) >= totalSize ? bytes : [];
	}

	private static bool TrySeek(Stream input, long offset)
	{
		if ( !input.CanSeek || offset < 0 || offset >= input.Length )
		{
			return false;
		}

		input.Seek(offset, SeekOrigin.Begin);
		return true;
	}

	private static bool TrySeekAndRead(Stream input, long offset, byte[] buffer,
		int writeOffset, int count)
	{
		if ( !TrySeek(input, offset) )
		{
			return false;
		}

		return input.Read(buffer, writeOffset, count) >= count;
	}

	private static ushort ReadUInt16(ReadOnlySpan<byte> bytes, bool littleEndian)
	{
		return littleEndian
			? BinaryPrimitives.ReadUInt16LittleEndian(bytes)
			: BinaryPrimitives.ReadUInt16BigEndian(bytes);
	}

	private static uint ReadUInt32(ReadOnlySpan<byte> bytes, bool littleEndian)
	{
		return littleEndian
			? BinaryPrimitives.ReadUInt32LittleEndian(bytes)
			: BinaryPrimitives.ReadUInt32BigEndian(bytes);
	}

	private sealed class IfdDirectory(Dictionary<ushort, IfdEntry> entries)
	{
		public Dictionary<ushort, IfdEntry> Entries { get; } = entries;
	}

	private sealed class IfdEntry
	{
		public ushort Type { get; init; }
		public uint Count { get; init; }
		public uint ValueOrOffset { get; init; }
	}
}

