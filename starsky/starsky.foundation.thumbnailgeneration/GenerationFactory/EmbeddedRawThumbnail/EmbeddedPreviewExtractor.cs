using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Production-style embedded RAW preview extractor.
///     - Very fast preview extraction (~1–3 ms typical)
///     - Minimal allocations
///     - Streaming copy
///     - Recursive TIFF IFD traversal
///     Supports RAW formats based on TIFF: DNG, CR2, NEF, ARW
/// </summary>
public class EmbeddedPreviewExtractor(IWebLogger logger)
{
	private const ushort TagJpegOffset = 0x0201;
	private const ushort TagJpegLength = 0x0202;
	private const ushort TagStripOffsets = 0x0111;
	private const ushort TagStripByteCounts = 0x0117;
	private const ushort TagSubIfds = 0x014A;
	private const ushort TagImageWidth = 0x0100;
	private const ushort TagImageHeight = 0x0101;

	private const ushort TiffMagicLe = 0x002A;

	private const int MaxIfdDepth = 6;
	private const int MaxPreviews = 32;
	private const int MinJpegBytes = 4 * 1024; // ignore tiny thumbnails
	private const uint MinDimension = 1024;

	// Prefer a preview of at least this width; fall back to largest otherwise.
	private const uint PreferredWidth = 2048;

	// Sanity cap on IFD entry count; prevents pathological/malformed RAW files
	// from causing huge allocations or hangs. Real RAW files rarely exceed 1000 entries per IFD.
	private const int MaxIfdEntryCount = 8192;

	/// <summary>
	///     Tries to extract embedded JPEG previews from a RAW file.
	/// </summary>
	/// <param name="rawPath">Path to the RAW file</param>
	/// <param name="outputLarge">Output path for the large preview (or null)</param>
	/// <param name="outputMedium">Output path for the medium preview (or null)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	public bool TryExtract(string rawPath, string? outputLarge, string? outputMedium)
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
	public bool TryExtract(Stream input, string? outputLarge, string? outputMedium)
	{
		if ( !TryParseTiffHeader(input, out var littleEndian, out var firstIfd) )
		{
			return false;
		}

		var previews = new List<PreviewCandidate>(MaxPreviews);
		var visited = new HashSet<uint>();

		ParseIfdRecursive(input, firstIfd, littleEndian, previews, visited, 0);

		if ( previews.Count == 0 )
		{
			return false;
		}

		// Sort descending by width (unknown width sorts last), then by byte length.
		previews.Sort((a, b) =>
		{
			var cmp = b.Width.CompareTo(a.Width);
			return cmp != 0 ? cmp : b.Length.CompareTo(a.Length);
		});

		var large = SelectAtLeast(previews, PreferredWidth);
		var medium = SelectAtLeast(previews, MinDimension);

		// Fall back: if no large, use the biggest we have.
		large ??= previews[0];
		medium ??= previews[0];

		var ok = true;

		if ( outputLarge is not null )
		{
			ok &= WritePreview(input, large.Value, outputLarge);
		}

		if ( outputMedium is not null && medium.Value.Offset != large.Value.Offset )
		{
			ok &= WritePreview(input, medium.Value, outputMedium);
		}

		return ok;
	}

	private static bool TryParseTiffHeader(Stream s, out bool littleEndian, out uint firstIfd)
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

		var magic = ReadUInt16(header.Slice(2), littleEndian);
		if ( magic != TiffMagicLe )
		{
			return false;
		}

		firstIfd = ReadUInt32(header.Slice(4), littleEndian);
		return firstIfd != 0;
	}

	private void ParseIfdRecursive(Stream input, uint offset,
		bool littleEndian, List<PreviewCandidate> previews, HashSet<uint> visited, int depth)
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

		var entryCount = ReadUInt16(countBuf, littleEndian);

		switch ( entryCount )
		{
			// Sanity guard: avoid absurd entry counts that may cause OOM or hangs
			case 0:
				return;
			case > MaxIfdEntryCount:
			{
				var streamPos = input.Position;
				var streamLen = input.Length;
				logger.LogInformation(
					$"[EmbeddedPreviewExtractor] IFD at offset {offset} skipped: entryCount {entryCount} exceeds cap {MaxIfdEntryCount} (stream pos: {streamPos}, len: {streamLen})");

				// Skip this IFD entirely: can't safely read the entries, so we skip to the next IFD pointer
				// The next IFD pointer should be at: current position + (entryCount * 12) + 4
				// But since we can't trust this file, just return and don't recurse
				return;
			}
		}

		// Read all IFD entries into a local buffer to avoid many small seeks.
		var entryBytes = entryCount * 12;
		var entryBuf = ArrayPool<byte>.Shared.Rent(entryBytes);

		try
		{
			if ( !TryReadExact(input, entryBuf, 0, entryBytes) )
			{
				return;
			}

			ParseIfdEntries(input, entryBuf.AsSpan(0, entryBytes), littleEndian, previews, visited,
				depth);
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

		var nextIfd = ReadUInt32(nextBuf, littleEndian);
		if ( nextIfd != 0 )
		{
			ParseIfdRecursive(input, nextIfd, littleEndian, previews, visited, depth + 1);
		}
	}

	private void ParseIfdEntries(
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
		var stripMulti = false;

		var count = entries.Length / 12;

		for ( var i = 0; i < count; i++ )
		{
			var e = entries.Slice(i * 12, 12);

			var tag = ReadUInt16(e, littleEndian);
			var type = ReadUInt16(e.Slice(2), littleEndian);
			var n = ReadUInt32(e.Slice(4), littleEndian);
			var value = ReadUInt32(e.Slice(8), littleEndian);

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
	private static uint ReadIndirectUInt32(Stream s, uint offset, ushort type, bool little)
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
	private static uint SumIndirectUInt32(Stream s, uint offset, ushort type, uint n, bool little)
	{
		if ( !TrySeek(s, offset) )
		{
			return 0;
		}

		uint sum = 0;
		Span<byte> b = stackalloc byte[4];
		Span<byte> s2 = stackalloc byte[2];

		for ( uint i = 0; i < n; i++ )
		{
			if ( type == 3 )
			{
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
	private static void ReadIndirectOffsets(Stream s, uint offset, ushort type, uint n, bool little,
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

	private static void AddCandidate(
		List<PreviewCandidate> previews,
		uint offset, uint length,
		uint width, uint height)
	{
		var candidate = new PreviewCandidate
		{
			Offset = offset, Length = length, Width = width, Height = height
		};

		if ( previews.Count >= MaxPreviews )
		{
			var worstIndex = 0;
			for ( var i = 1; i < previews.Count; i++ )
			{
				if ( IsBetterCandidate(previews[worstIndex], previews[i]) )
				{
					worstIndex = i;
				}
			}

			if ( IsBetterCandidate(candidate, previews[worstIndex]) )
			{
				previews[worstIndex] = candidate;
			}

			return;
		}

		previews.Add(candidate);
	}

	private static bool IsBetterCandidate(PreviewCandidate left, PreviewCandidate right)
	{
		if ( left.Width != right.Width )
		{
			return left.Width > right.Width;
		}

		return left.Length > right.Length;
	}

	private static PreviewCandidate? SelectAtLeast(List<PreviewCandidate> previews, uint minWidth)
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

	private static bool WritePreview(Stream input, PreviewCandidate preview, string outputPath)
	{
		if ( !TrySeek(input, preview.Offset) )
		{
			return false;
		}

		using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

		long remaining = preview.Length;
		var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);

		try
		{
			while ( remaining > 0 )
			{
				var read = input.Read(buffer, 0, ( int ) Math.Min(buffer.Length, remaining));

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
	private static uint ReadScalar(ushort type, uint raw)
	{
		return type == 3 ? raw & 0xFFFF : raw;
		// SHORT is stored in the low 16 bits
	}

	private static bool TrySeek(Stream s, uint offset)
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

	private static bool TryReadExact(Stream s, Span<byte> buffer)
	{
		var total = 0;

		while ( total < buffer.Length )
		{
			var read = s.Read(buffer.Slice(total));

			if ( read == 0 )
			{
				return false; // EOF before buffer filled
			}

			total += read;
		}

		return true;
	}

	// Timeout-enabled read for rented byte[] buffers
	private bool TryReadExact(Stream s, byte[] buffer, int offset, int count,
		int timeoutMs = 5000)
	{
		if ( count == 0 )
		{
			return true;
		}

		using var cts = new CancellationTokenSource(timeoutMs);
		var total = 0;

		try
		{
			while ( total < count )
			{
				var task = s.ReadAsync(buffer, offset + total, count - total, cts.Token);
				var read = task.GetAwaiter().GetResult();

				if ( read == 0 )
				{
					logger.LogInformation(
						$"[EmbeddedPreviewExtractor] TryReadExact: EOF after reading {total} of {count} bytes");
					return false;
				}

				total += read;
			}

			return true;
		}
		catch ( OperationCanceledException )
		{
			logger.LogError(
				$"[EmbeddedPreviewExtractor] TryReadExact: timeout after {timeoutMs}ms when reading {count} bytes");
			return false;
		}
		catch ( Exception ex )
		{
			logger.LogError($"[EmbeddedPreviewExtractor] TryReadExact: exception: {ex.Message}");
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ushort ReadUInt16(ReadOnlySpan<byte> b, bool little)
	{
		return little
			? ( ushort ) ( b[0] | ( b[1] << 8 ) )
			: ( ushort ) ( b[1] | ( b[0] << 8 ) );
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ReadUInt32(ReadOnlySpan<byte> b, bool little)
	{
		return little
			? ( uint ) ( b[0] | ( b[1] << 8 ) | ( b[2] << 16 ) | ( b[3] << 24 ) )
			: ( uint ) ( b[3] | ( b[2] << 8 ) | ( b[1] << 16 ) | ( b[0] << 24 ) );
	}

	// -------------------------------------------------------------------------

	private struct PreviewCandidate
	{
		public uint Offset;
		public uint Length;
		public uint Width; // 0 = unknown
		public uint Height; // 0 = unknown
	}
}
