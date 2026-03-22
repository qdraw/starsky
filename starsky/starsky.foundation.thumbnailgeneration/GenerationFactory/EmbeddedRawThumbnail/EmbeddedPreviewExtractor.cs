using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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

	// Maximum allowed preview size: 50 MB (prevents extracting entire file by mistake)
	private const long MaxPreviewBytes = 50 * 1024 * 1024;

	// Prefer a preview of at least this width; fall back to largest otherwise.
	private const uint PreferredWidth = 2048;

	// Sanity cap on IFD entry count; prevents pathological/malformed RAW files
	// from causing huge allocations or hangs. Real RAW files rarely exceed 1000 entries per IFD.
	// Updated: Many modern RAW files (Sony, Nikon high-res) have 8K+ entries legitimately.
	// Increased to 32768 (32K entries) to process high-res preview IFDs.
	// At 12 bytes/entry, this is only 768 KB allocation—acceptable for preview extraction.
	// Further updated: entry counts up to 60K observed in real files. Increased to 65535 (ushort.MaxValue).
	// At 12 bytes/entry, worst case = 786 KB allocation. Added size-based check below.

	// Secondary sanity check: skip IFDs that would consume >50% of file size.
	// Real image data IFDs should never take half the file.
	private const double MaxIfdSizeRatio = 0.5;

	/// <summary>
	///     Tries to extract embedded JPEG previews from a RAW file.
	/// </summary>
	/// <param name="rawPath">Path to the RAW file</param>
	/// <param name="outputLarge">Output path for the large preview (or null)</param>
	/// <param name="outputMedium">Output path for the medium preview (or null)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	public async Task<bool> TryExtract(string rawPath, string? outputLarge, string? outputMedium)
	{
		using var fs = new FileStream(rawPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		return await TryExtract(fs, outputLarge, outputMedium);
	}

	/// <summary>
	///     Tries to extract embedded JPEG previews from a RAW stream.
	/// </summary>
	/// <param name="input">Input stream containing RAW data</param>
	/// <param name="outputLarge">Output path for the large preview (or null)</param>
	/// <param name="outputMedium">Output path for the medium preview (or null)</param>
	/// <returns>true if at least one preview was successfully extracted</returns>
	public Task<bool> TryExtract(Stream input, string? outputLarge, string? outputMedium)
	{
		if ( !TryParseTiffHeader(input, out var littleEndian, out var firstIfd) )
		{
			return Task.FromResult(false);
		}

		var previews = new List<PreviewCandidate>(MaxPreviews);
		var visited = new HashSet<uint>();

		ParseIfdRecursive(input, firstIfd, littleEndian, previews, visited, 0,
			new IfdPathContext(false, 0, 0));

		if ( previews.Count == 0 )
		{
			return Task.FromResult(false);
		}

		// Sort by source priority first, then dimensions, then byte length.
		previews.Sort((a, b) => CompareCandidates(a, b));

		var ok = true;
		uint? selectedLargeOffset = null;

		if ( outputLarge is not null )
		{
			ok &= TryWriteBestPreview(input, previews, outputLarge, PreferredWidth, null,
				out selectedLargeOffset);
		}

		if ( outputMedium is not null )
		{
			var mediumWritten = TryWriteBestPreview(input, previews, outputMedium, MinDimension,
				selectedLargeOffset, out _);
			if ( !mediumWritten && selectedLargeOffset.HasValue && outputLarge is not null &&
			     File.Exists(outputLarge) )
			{
				File.Copy(outputLarge, outputMedium, true);
				mediumWritten = true;
			}

			ok &= mediumWritten;
		}

		return Task.FromResult(ok);
	}

	private bool TryWriteBestPreview(Stream input, List<PreviewCandidate> previews,
		string outputPath, uint minWidth, uint? skipOffset, out uint? selectedOffset)
	{
		selectedOffset = null;
		var hasNonTiny = previews.Exists(p => p.Length >= MinJpegBytes);

		if ( TryWriteBestPreviewPass(input, previews, outputPath, minWidth, skipOffset,
				hasNonTiny ? (uint)MinJpegBytes : 0u, out selectedOffset) )
		{
			return true;
		}

		// Fallback: allow tiny candidates when no viable larger preview exists.
		return hasNonTiny && TryWriteBestPreviewPass(input, previews, outputPath, minWidth,
			skipOffset, 0, out selectedOffset);
	}

	private bool TryWriteBestPreviewPass(Stream input, List<PreviewCandidate> previews,
		string outputPath, uint minWidth, uint? skipOffset, uint minLength,
		out uint? selectedOffset)
	{
		selectedOffset = null;
		foreach ( var candidate in previews )
		{
			if ( skipOffset.HasValue && candidate.Offset == skipOffset.Value )
			{
				continue;
			}

			if ( candidate.Length < minLength )
			{
				continue;
			}

			if ( candidate.Width != 0 && candidate.Width < minWidth )
			{
				continue;
			}

			if ( WritePreview(input, candidate, outputPath) )
			{
				selectedOffset = candidate.Offset;
				return true;
			}
		}

		foreach ( var candidate in previews )
		{
			if ( skipOffset.HasValue && candidate.Offset == skipOffset.Value )
			{
				continue;
			}

			if ( candidate.Length < minLength )
			{
				continue;
			}

			if ( WritePreview(input, candidate, outputPath) )
			{
				selectedOffset = candidate.Offset;
				return true;
			}
		}

		return false;
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
		bool littleEndian, List<PreviewCandidate> previews, HashSet<uint> visited, int depth,
		IfdPathContext context)
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

		// Secondary sanity check: reject IFDs consuming >50% of file as corrupted.
		var entryBytes = ( long ) entryCount * 12;
		if ( entryBytes > input.Length * MaxIfdSizeRatio )
		{
			logger.LogDebug(
				$"[EmbeddedPreviewExtractor] IFD at offset {offset} skipped: entryCount {entryCount} would require {entryBytes} bytes ({( double ) entryBytes / input.Length:P} of {input.Length} byte file), likely corrupted");
			return;
		}

		if ( entryCount == 0 )
		{
			return;
		}

		var entryBuf = ArrayPool<byte>.Shared.Rent(( int ) entryBytes);

		try
		{
			if ( !TryReadExact(input, entryBuf, 0, ( int ) entryBytes) )
			{
				return;
			}

			ParseIfdEntries(input, entryBuf.AsSpan(0, ( int ) entryBytes), littleEndian, previews,
				visited,
				depth, context);
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
			ParseIfdRecursive(input, nextIfd, littleEndian, previews, visited, depth + 1,
				context with
				{
					RootIfdIndex = context.RootIfdIndex + 1
				});
		}
	}

	private void ParseIfdEntries(
		Stream input,
		ReadOnlySpan<byte> entries,
		bool littleEndian,
		List<PreviewCandidate> previews,
		HashSet<uint> visited,
		int depth,
		IfdPathContext context)
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
			AddCandidate(previews, jpegOffset, jpegLength, ifdWidth, ifdHeight,
				ResolveSourceKind(context));
		}
		else if ( !stripMulti && stripOffset > 0 && stripLength >= MinJpegBytes )
		{
			// Single-strip — might be an embedded JPEG (e.g. in some Canon IFDs).
			AddCandidate(previews, stripOffset, stripLength, ifdWidth, ifdHeight,
				ResolveSourceKind(context));
		}
		else if ( jpegOffset > 0 && jpegLength > 0 )
		{
			// Keep tiny JPEGs only as fallback; they are filtered during selection.
			AddCandidate(previews, jpegOffset, jpegLength, ifdWidth, ifdHeight,
				ResolveSourceKind(context));
		}
		else if ( !stripMulti && stripOffset > 0 && stripLength > 0 )
		{
			AddCandidate(previews, stripOffset, stripLength, ifdWidth, ifdHeight,
				ResolveSourceKind(context));
		}
		// Multi-strip raw data is intentionally skipped; it's sensor data, not a preview.

		for ( var i = 0; i < subIfdOffsets.Count; i++ )
		{
			ParseIfdRecursive(input, subIfdOffsets[i], littleEndian, previews, visited,
				depth + 1,
				new IfdPathContext(true, i + 1, context.RootIfdIndex));
		}
	}

	private static PreviewSourceKind ResolveSourceKind(IfdPathContext context)
	{
		if ( context.IsSubIfd )
		{
			return context.SubIfdOrdinal <= 1
				? PreviewSourceKind.JpgFromRaw
				: PreviewSourceKind.OtherImage;
		}

		return context.RootIfdIndex == 0
			? PreviewSourceKind.PreviewImage
			: PreviewSourceKind.Thumbnail;
	}

	private static int GetSourcePriority(PreviewSourceKind kind)
	{
		return kind switch
		{
			PreviewSourceKind.JpgFromRaw => 4,
			PreviewSourceKind.PreviewImage => 3,
			PreviewSourceKind.OtherImage => 2,
			PreviewSourceKind.Thumbnail => 1,
			_ => 0
		};
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
		uint width, uint height,
		PreviewSourceKind sourceKind)
	{
		var candidate = new PreviewCandidate
		{
			Offset = offset,
			Length = length,
			Width = width,
			Height = height,
			SourceKind = sourceKind
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

	private static int CompareCandidates(PreviewCandidate left, PreviewCandidate right)
	{
		var sourceCompare = GetSourcePriority(right.SourceKind).CompareTo(
			GetSourcePriority(left.SourceKind));
		if ( sourceCompare != 0 )
		{
			return sourceCompare;
		}

		var dimensionCompare = CompareByDimensions(right, left);
		if ( dimensionCompare != 0 )
		{
			return dimensionCompare;
		}

		return right.Length.CompareTo(left.Length);
	}

	private static int CompareByDimensions(PreviewCandidate left, PreviewCandidate right)
	{
		var leftArea = (long)left.Width * left.Height;
		var rightArea = (long)right.Width * right.Height;

		if ( leftArea != 0 && rightArea != 0 && leftArea != rightArea )
		{
			return leftArea.CompareTo(rightArea);
		}

		if ( left.Width != right.Width )
		{
			return left.Width.CompareTo(right.Width);
		}

		return left.Height.CompareTo(right.Height);
	}

	private static bool IsBetterCandidate(PreviewCandidate left, PreviewCandidate right)
	{
		return CompareCandidates(left, right) < 0;
	}

	// -------------------------------------------------------------------------
	// I/O

	private bool WritePreview(Stream input, PreviewCandidate preview, string outputPath)
	{
		if ( !TrySeek(input, preview.Offset) )
		{
			logger.LogDebug(
				$"[EmbeddedPreviewExtractor] Failed to seek to offset {preview.Offset} for JPEG preview");
			return false;
		}

		// Sanity check: reject previews > 50 MB
		if ( preview.Length > MaxPreviewBytes )
		{
			logger.LogDebug(
				$"[EmbeddedPreviewExtractor] Skipping preview at offset {preview.Offset}: size {preview.Length} exceeds max {MaxPreviewBytes}");
			return false;
		}

		using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

		long remaining = preview.Length;
		var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
		long bytesWritten = 0;

		try
		{
			while ( remaining > 0 )
			{
				var read = input.Read(buffer, 0, ( int ) Math.Min(buffer.Length, remaining));

				if ( read == 0 )
				{
					logger.LogDebug(
						$"[EmbeddedPreviewExtractor] Truncated read at offset {preview.Offset}: got {bytesWritten}/{preview.Length} bytes");
					break;
				}

				output.Write(buffer, 0, read);
				bytesWritten += read;
				remaining -= read;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}

		// Validate extracted JPEG
		if ( bytesWritten == 0 )
		{
			logger.LogDebug(
				$"[EmbeddedPreviewExtractor] Rejecting empty preview at offset {preview.Offset}");
			try
			{
				File.Delete(outputPath);
			}
			catch
			{
				/* ignore */
			}

			return false;
		}

		if ( remaining != 0 )
		{
			logger.LogDebug(
				$"[EmbeddedPreviewExtractor] Rejecting truncated preview at offset {preview.Offset}: got {bytesWritten}/{preview.Length} bytes");
			try
			{
				File.Delete(outputPath);
			}
			catch
			{
				/* ignore */
			}

			return false;
		}

		// Verify JPEG signature
		if ( !ValidateJpegFile(outputPath, preview.Offset) )
		{
			try
			{
				File.Delete(outputPath);
			}
			catch
			{
				/* ignore */
			}

			return false;
		}

		return true;
	}

	private bool ValidateJpegFile(string filePath, uint expectedOffset)
	{
		try
		{
			var fileInfo = new FileInfo(filePath);
			if ( fileInfo.Length < 4 )
			{
				logger.LogDebug(
					$"[EmbeddedPreviewExtractor] Rejecting file at offset {expectedOffset}: too small ({fileInfo.Length} bytes)");
				return false;
			}

			using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			Span<byte> header = stackalloc byte[4];
			if ( fs.Read(header) < 4 )
			{
				return false;
			}

			// JPEG must start with FFD8 (SOI marker)
			if ( header[0] != 0xFF || header[1] != 0xD8 )
			{
				logger.LogDebug(
					$"[EmbeddedPreviewExtractor] Rejecting file at offset {expectedOffset}: invalid JPEG SOI marker ({header[0]:X2} {header[1]:X2})");
				return false;
			}

			// Ensure JPEG contains a scan marker; avoids metadata-only blobs with SOI/EOI.
			if ( !HasStartOfScanMarker(fs) )
			{
				logger.LogDebug(
					$"[EmbeddedPreviewExtractor] Rejecting file at offset {expectedOffset}: no SOS marker found");
				return false;
			}

			// Quick EOF check: JPEG should end with FFD9 (EOI marker)
			fs.Seek(-2, SeekOrigin.End);
			Span<byte> footer = stackalloc byte[2];
			if ( fs.Read(footer) < 2 || footer[0] != 0xFF || footer[1] != 0xD9 )
			{
				logger.LogDebug(
					$"[EmbeddedPreviewExtractor] Rejecting file at offset {expectedOffset}: missing JPEG EOI marker");
				return false;
			}

			return true;
		}
		catch ( Exception ex )
		{
			logger.LogDebug(
				$"[EmbeddedPreviewExtractor] Exception validating file at offset {expectedOffset}: {ex.Message}");
			return false;
		}
	}

	private static bool HasStartOfScanMarker(Stream stream)
	{
		stream.Seek(2, SeekOrigin.Begin); // after SOI
		Span<byte> marker = stackalloc byte[2];
		Span<byte> lenBuf = stackalloc byte[2];

		while ( stream.Position + 1 < stream.Length )
		{
			if ( stream.Read(marker) < 2 )
			{
				return false;
			}

			if ( marker[0] != 0xFF )
			{
				continue;
			}

			// Skip fill bytes (FF FF ...)
			while ( marker[1] == 0xFF )
			{
				if ( stream.Read(marker.Slice(1, 1)) < 1 )
				{
					return false;
				}
			}

			var code = marker[1];
			if ( code == 0xDA )
			{
				return true; // SOS
			}

			if ( code == 0xD9 )
			{
				return false; // EOI before SOS
			}

			if ( code is >= 0xD0 and <= 0xD7 || code == 0x01 )
			{
				continue; // markers without length
			}

			if ( stream.Read(lenBuf) < 2 )
			{
				return false;
			}

			var segmentLength = (lenBuf[0] << 8) | lenBuf[1];
			if ( segmentLength < 2 )
			{
				return false;
			}

			var payloadLength = segmentLength - 2;
			if ( stream.Position + payloadLength > stream.Length )
			{
				return false;
			}

			if ( IsSofMarker(code) )
			{
				if ( IsLosslessSofMarker(code) )
				{
					return false;
				}

				var precision = stream.ReadByte();
				if ( precision is not 8 and not 12 )
				{
					return false;
				}

				payloadLength -= 1;
			}

			if ( payloadLength > 0 )
			{
				stream.Seek(payloadLength, SeekOrigin.Current);
			}
		}

		return false;
	}

	private static bool IsSofMarker(int marker)
	{
		return marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9
		       or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF;
	}

	private static bool IsLosslessSofMarker(int marker)
	{
		return marker is 0xC3 or 0xC7 or 0xCB or 0xCF;
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
		public PreviewSourceKind SourceKind;
	}

	private readonly record struct IfdPathContext(
		bool IsSubIfd,
		int SubIfdOrdinal,
		int RootIfdIndex);

	private enum PreviewSourceKind
	{
		Thumbnail,
		OtherImage,
		PreviewImage,
		JpgFromRaw
	}
}
