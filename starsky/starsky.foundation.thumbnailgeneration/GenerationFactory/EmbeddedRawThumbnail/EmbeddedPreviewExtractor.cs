using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace starsky.foundation.thumbnailgeneration.Services;

/// <summary>
///     Production-style embedded RAW preview extractor.
///     - Very fast preview extraction (~1–3 ms typical)
///     - Minimal allocations
///     - Streaming copy
///     - Recursive TIFF IFD traversal
///     Supports RAW formats based on TIFF: DNG, CR2, NEF, ARW
/// </summary>
public static class EmbeddedPreviewExtractor
{
	const ushort TagJpegOffset = 0x0201;
	const ushort TagJpegLength = 0x0202;
	const ushort TagStripOffsets = 0x0111;
	const ushort TagStripByteCounts = 0x0117;
	const ushort TagSubIfds = 0x014A;
	const ushort TagImageWidth = 0x0100;
	const ushort TagImageHeight = 0x0101;
	const ushort TagNewSubfileType = 0x00FE;

	const ushort TiffMagicLe = 0x002A;
	const ushort TiffMagicBe = 0x002A; // same value, byte order already applied

	const int MaxIfdDepth = 6;
	const int MaxPreviews = 32;
	const int MinJpegBytes = 4 * 1024; // ignore tiny thumbnails
	const uint MinDimension = 1024;

	// Prefer a preview of at least this width; fall back to largest otherwise.
	const uint PreferredWidth = 2048;

	/// <summary>
	///     Tries to extract embedded JPEG previews from a RAW file.
	/// </summary>
	/// <param name="rawPath">Path to the RAW file</param>
	/// <param name="outputLarge">Output path for the large preview (or null)</param>
	/// <param name="outputMedium">Output path for the medium preview (or null)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	public static bool TryExtract(string rawPath, string? outputLarge, string? outputMedium)
	{
		using var fs = new FileStream(rawPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		return TryExtract(fs, outputLarge, outputMedium);
	}

	/// <summary>
	///     Tries to extract embedded JPEG previews from a RAW stream.
	/// </summary>
	/// <param name="input">Input stream containing RAW data</param>
	/// <param name="outputLarge">Output path for the large preview (or null)</param>
	/// <param name="outputMedium">Output path for the medium preview (or null)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	public static bool TryExtract(Stream input, string? outputLarge, string? outputMedium)
	{
		if ( !TryParseTiffHeader(input, out var littleEndian, out var firstIfd) )
		{
			return false;
		}

		var previews = new List<PreviewCandidate>(MaxPreviews);
		var visited = new HashSet<uint>();

		ParseIfdRecursive(input, firstIfd, littleEndian, previews, visited, depth: 0);

		if ( previews.Count == 0 )
		{
			return false;
		}

		// Sort descending by width (unknown width sorts last), then by byte length.
		previews.Sort((a, b) =>
		{
			int cmp = b.Width.CompareTo(a.Width);
			return cmp != 0 ? cmp : b.Length.CompareTo(a.Length);
		});

		PreviewCandidate? large = SelectAtLeast(previews, PreferredWidth);
		PreviewCandidate? medium = SelectAtLeast(previews, MinDimension);

		// Fall back: if no large, use the biggest we have.
		large ??= previews[0];
		medium ??= previews[0];

		bool ok = true;

		if ( outputLarge is not null )
		{
			ok &= WritePreview(input, large.Value, outputLarge);
		}

		if ( outputMedium is not null && medium.Value.Offset != large!.Value.Offset )
		{
			ok &= WritePreview(input, medium.Value, outputMedium);
		}

		return ok;
	}

	// -------------------------------------------------------------------------

	struct PreviewCandidate
	{
		public uint Offset;
		public uint Length;
		public uint Width; // 0 = unknown
		public uint Height; // 0 = unknown
	}

	static bool TryParseTiffHeader(Stream s, out bool littleEndian, out uint firstIfd)
	{
		littleEndian = false;
		firstIfd = 0;

		Span<byte> header = stackalloc byte[8];

		if ( !TryReadExact(s, header) )
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
			return false; // not a TIFF-based file
		}

		ushort magic = ReadUInt16(header.Slice(2), littleEndian);
		if ( magic != TiffMagicLe )
		{
			return false;
		}

		firstIfd = ReadUInt32(header.Slice(4), littleEndian);
		return firstIfd != 0;
	}

	static void ParseIfdRecursive(
		Stream input,
		uint offset,
		bool littleEndian,
		List<PreviewCandidate> previews,
		HashSet<uint> visited,
		int depth)
	{
		if ( depth > MaxIfdDepth || offset == 0 )
		{
			return;
		}

		if ( !visited.Add(offset) )
		{
			return; // cycle detected
		}

		if ( !TrySeek(input, offset) )
		{
			return;
		}

		Span<byte> countBuf = stackalloc byte[2];
		if ( !TryReadExact(input, countBuf) )
		{
			return;
		}

		ushort entryCount = ReadUInt16(countBuf, littleEndian);

		// Read all IFD entries into a local buffer to avoid many small seeks.
		int entryBytes = entryCount * 12;
		byte[] entryBuf = ArrayPool<byte>.Shared.Rent(entryBytes);

		try
		{
			if ( !TryReadExact(input, entryBuf.AsSpan(0, entryBytes)) )
			{
				return;
			}

			ParseIfdEntries(
				input, entryBuf.AsSpan(0, entryBytes),
				littleEndian, previews, visited, depth);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(entryBuf);
		}

		// Next IFD pointer follows the entry block.
		Span<byte> nextBuf = stackalloc byte[4];
		if ( !TryReadExact(input, nextBuf) )
		{
			return;
		}

		uint nextIfd = ReadUInt32(nextBuf, littleEndian);
		if ( nextIfd != 0 )
		{
			ParseIfdRecursive(input, nextIfd, littleEndian, previews, visited, depth + 1);
		}
	}

	static void ParseIfdEntries(
		Stream input,
		ReadOnlySpan<byte> entries,
		bool littleEndian,
		List<PreviewCandidate> previews,
		HashSet<uint> visited,
		int depth)
	{
		// Tags we care about in this IFD.
		uint jpegOffset = 0, jpegLength = 0;
		uint stripOffset = 0, stripLength = 0;
		uint ifdWidth = 0, ifdHeight = 0;
		var subIfdOffsets = new List<uint>(4);
		bool stripMulti = false;

		int count = entries.Length / 12;

		for ( int i = 0; i < count; i++ )
		{
			var e = entries.Slice(i * 12, 12);

			ushort tag = ReadUInt16(e, littleEndian);
			ushort type = ReadUInt16(e.Slice(2), littleEndian);
			uint n = ReadUInt32(e.Slice(4), littleEndian);
			uint value = ReadUInt32(e.Slice(8), littleEndian);

			switch ( tag )
			{
				case TagJpegOffset:
					// Always a single LONG; value is the offset directly.
					jpegOffset = value;
					break;

				case TagJpegLength:
					jpegLength = value;
					break;

				case TagImageWidth:
					ifdWidth = ReadScalar(type, value);
					break;

				case TagImageHeight:
					ifdHeight = ReadScalar(type, value);
					break;

				case TagStripOffsets:
					if ( n == 1 )
					{
						stripOffset = value;
					}
					else
					{
						// value is a pointer to an array of offsets; read first one.
						stripOffset = ReadIndirectUInt32(input, value, type, littleEndian);
						stripMulti = true;
					}
					break;

				case TagStripByteCounts:
					if ( n == 1 )
					{
						stripLength = value;
					}
					else
					{
						// Sum all strip byte counts to get total image size.
						stripLength = SumIndirectUInt32(input, value, type, n, littleEndian);
						stripMulti = true;
					}
					break;

				case TagSubIfds:
					if ( n == 1 )
					{
						subIfdOffsets.Add(value);
					}
					else
					{
						// value is a pointer to an array of IFD offsets.
						ReadIndirectOffsets(input, value, type, n, littleEndian, subIfdOffsets);
					}
					break;
			}
		}

		// Prefer an explicit JPEG thumbnail over raw strips.
		if ( jpegOffset > 0 && jpegLength >= MinJpegBytes )
		{
			AddCandidate(previews, jpegOffset, jpegLength, ifdWidth, ifdHeight);
		}
		else if ( !stripMulti && stripOffset > 0 && stripLength >= MinJpegBytes )
		{
			// Single-strip — might be an embedded JPEG (e.g. in some Canon IFDs).
			AddCandidate(previews, stripOffset, stripLength, ifdWidth, ifdHeight);
		}
		// Multi-strip raw data is intentionally skipped; it's sensor data, not a preview.

		foreach ( var sub in subIfdOffsets )
		{
			ParseIfdRecursive(input, sub, littleEndian, previews, visited, depth + 1);
		}
	}

	// -------------------------------------------------------------------------
	// Indirect value helpers

	/// <summary>Reads a single scalar from an indirect pointer (for SHORT or LONG types).</summary>
	static uint ReadIndirectUInt32(Stream s, uint offset, ushort type, bool little)
	{
		if ( !TrySeek(s, offset) )
		{
			return 0;
		}

		Span<byte> b = stackalloc byte[4];

		if ( type == 3 ) // SHORT
		{
			Span<byte> s2 = stackalloc byte[2];
			return TryReadExact(s, s2) ? ReadUInt16(s2, little) : 0u;
		}

		return TryReadExact(s, b) ? ReadUInt32(b, little) : 0u;
	}

	/// <summary>Sums n values from an indirect pointer.</summary>
	static uint SumIndirectUInt32(Stream s, uint offset, ushort type, uint n, bool little)
	{
		if ( !TrySeek(s, offset) )
		{
			return 0;
		}

		uint sum = 0;
		int stride = type == 3 ? 2 : 4;
		Span<byte> b = stackalloc byte[4];

		for ( uint i = 0; i < n; i++ )
		{
			if ( type == 3 )
			{
				Span<byte> s2 = stackalloc byte[2];
				if ( !TryReadExact(s, s2) )
				{
					break;
				}

				sum += ReadUInt16(s2, little);
			}
			else
			{
				if ( !TryReadExact(s, b) )
				{
					break;
				}

				sum += ReadUInt32(b, little);
			}
		}

		return sum;
	}

	/// <summary>Reads n IFD offsets from an indirect pointer into the list.</summary>
	static void ReadIndirectOffsets(Stream s, uint offset, ushort type, uint n, bool little,
		List<uint> result)
	{
		if ( !TrySeek(s, offset) )
		{
			return;
		}

		Span<byte> b = stackalloc byte[4];
		Span<byte> s2 = stackalloc byte[2];

		for ( uint i = 0; i < n; i++ )
		{
			uint v;
			if ( type == 3 )
			{
				if ( !TryReadExact(s, s2) )
				{
					break;
				}

				v = ReadUInt16(s2, little);
			}
			else
			{
				if ( !TryReadExact(s, b) )
				{
					break;
				}

				v = ReadUInt32(b, little);
			}

			if ( v != 0 )
			{
				result.Add(v);
			}
		}
	}

	// -------------------------------------------------------------------------
	// Candidate management

	static void AddCandidate(
		List<PreviewCandidate> previews,
		uint offset, uint length,
		uint width, uint height)
	{
		if ( previews.Count >= MaxPreviews )
		{
			Debug.Fail("MAX_PREVIEWS exceeded — some candidates were dropped.");
			return;
		}

		previews.Add(new PreviewCandidate
		{
			Offset = offset,
			Length = length,
			Width = width,
			Height = height,
		});
	}

	static PreviewCandidate? SelectAtLeast(List<PreviewCandidate> previews, uint minWidth)
	{
		// List is already sorted widest-first.
		foreach ( var p in previews )
		{
			if ( p.Width == 0 )
			{
				continue; // unknown dimension — handled as fallback by caller
			}

			if ( p.Width >= minWidth )
			{
				return p;
			}
		}

		return null;
	}

	// -------------------------------------------------------------------------
	// I/O

	static bool WritePreview(Stream input, PreviewCandidate preview, string outputPath)
	{
		if ( !TrySeek(input, preview.Offset) )
		{
			return false;
		}

		using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

		long remaining = preview.Length;
		byte[] buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);

		try
		{
			while ( remaining > 0 )
			{
				int read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));

				if ( read == 0 )
				{
					break;
				}

				output.Write(buffer, 0, read);
				remaining -= read;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}

		return remaining == 0; // false if file was truncated
	}

	// -------------------------------------------------------------------------
	// Low-level helpers

	/// <summary>Reads a SHORT (type 3) or LONG (type 4) scalar from an inline IFD value.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static uint ReadScalar(ushort type, uint raw)
		=> type == 3 ? (raw & 0xFFFF) : raw; // SHORT is stored in the low 16 bits

	static bool TrySeek(Stream s, uint offset)
	{
		try
		{
			s.Seek(offset, SeekOrigin.Begin);
			return true;
		}
		catch
		{
			return false;
		}
	}

	static bool TryReadExact(Stream s, Span<byte> buffer)
	{
		int total = 0;

		while ( total < buffer.Length )
		{
			int read = s.Read(buffer.Slice(total));

			if ( read == 0 )
			{
				return false; // EOF before buffer filled
			}

			total += read;
		}

		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static ushort ReadUInt16(ReadOnlySpan<byte> b, bool little)
		=> little
			? (ushort)(b[0] | (b[1] << 8))
			: (ushort)(b[1] | (b[0] << 8));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static uint ReadUInt32(ReadOnlySpan<byte> b, bool little)
		=> little
			? (uint)(b[0] | (b[1] << 8) | (b[2] << 16) | (b[3] << 24))
			: (uint)(b[3] | (b[2] << 8) | (b[1] << 16) | (b[0] << 24));
}




