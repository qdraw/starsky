using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	TiffEmbedded;

/// <summary>
///     Extracts embedded JPEG previews from TIFF-based RAW formats.
///     Supports: DNG (Adobe), CR2 (Canon EOS), NEF (Nikon), ARW (Sony)
///     Implementation focusing on metadata extraction only.
/// </summary>
public partial class TiffEmbeddedPreviewExtractor
{
	private const int MaxIfdDepth = 6;
	private const int MaxIfdVisits = 64;
	private const int MaxRootIfdChain = 6;
	private const int MaxSubIfdOffsets = 32;
	private const int MaxPreviews = 8;
	private const int MinJpegSize = 4096; // 4KB minimum for valid JPEG
	private const int MaxMakerNoteScanBytes = 50 * 1024 * 1024;
	private const int CanonFallbackScanBytes = 2 * 1024 * 1024;
	private const int CanonMakerNoteIfdProbeBytes = 64;

	// TIFF IFD Tags
	private const ushort TagImageWidth = 0x0100;
	private const ushort TagImageLength = 0x0101;
	private const ushort TagCompression = 0x0103;
	private const ushort TagJpegOffset = 0x0201;
	private const ushort TagJpegLength = 0x0202;
	private const ushort TagSubIfds = 0x014A;
	private const ushort TagExifIfd = 0x8769;
	private const ushort TagMakerNote = 0x927C;

	private const ushort TagSonyPreviewOffset = 0x2010;
	private const ushort TagSonyPreviewLength = 0x2011;
	private const ushort TagSonyPreviewAlt = 0x2020;
	private const ushort TagCanonPreviewOffset = 0x0001;
	private const ushort TagCanonPreviewLength = 0x0004;
	private const ushort TagStripOffsets = 0x0111;
	private const ushort TagStripByteCounts = 0x0117;
	private readonly IWebLogger _logger;
	private readonly IStorage _subPathStorage;
	private readonly IStorage _tempStorage;

	public TiffEmbeddedPreviewExtractor(IWebLogger logger, ISelectorStorage selectorStorage)
	{
		_logger = logger;
		_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_tempStorage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
	}


	public async Task<bool> TryExtract(string subPathRawFile,
		string? outputLargePath, ExtensionRolesHelper.ImageFormat? imageFormat = null)
	{
		if (
			!_subPathStorage.ExistFile(subPathRawFile) )
		{
			return false;
		}

		try
		{
			await using var input = _subPathStorage.ReadStream(subPathRawFile);
			await using var output = new MemoryStream();

			var rawFlavor = RawFlavorHelper.GetRawFlavorFromPath(subPathRawFile);
			if ( imageFormat != null )
			{
				rawFlavor = RawFlavorHelper.GetRawFlavorFromImageFormat(imageFormat);
			}

			var ok = await TryExtractFromStream(input, output,
				$"Reference: {subPathRawFile}", rawFlavor);
			if ( !ok || outputLargePath == null )
			{
				return ok;
			}

			output.Seek(0, SeekOrigin.Begin);
			return await _tempStorage.WriteStreamAsync(output, outputLargePath);
		}
		catch ( Exception ex )
		{
			_logger.LogError(
				$"[EmbeddedPreviewExtractor] Failed to extract from {subPathRawFile}: {ex.Message}");
			return false;
		}
	}

	private static async Task<bool> TryExtractFromStream(Stream input, Stream? outputLarge,
		string referenceInfo, RawFlavor rawFlavor)
	{
		var candidates = new List<PreviewCandidate>();

		// Parse TIFF header
		if ( !TryParseTiffHeader(input, out var littleEndian, out var firstIfdOffset) )
		{
			return false;
		}

		// Traverse IFD structure
		var traversalContext = new ParseTraversalContext
		{
			Previews = candidates,
			Visited = [],
			ReferenceInfo = referenceInfo,
			RawFlavor = rawFlavor
		};
		ParseIfdRecursive(input, firstIfdOffset, littleEndian, traversalContext, 0, false);

		// Enrich candidates with dimensions when missing by probing JPEG SOF from the stream.
		// This helps selection prefer true higher-resolution previews discovered via MakerNote scans.
		foreach ( var c in traversalContext.Previews )
		{
			if ( ( c.Width != 0 && c.Height != 0 ) ||
			     !TryGetJpegDimensionsAtOffset(input, c.Offset, c.Length, out var w, out var h) )
			{
				continue;
			}

			c.Width = w;
			c.Height = h;
		}

		// Select and extract best preview
		var best = SelectBestPreviewHelper.SelectBestPreview(candidates);
		if ( best == null )
		{
			return false;
		}

		return await ExtractPreview.ExtractPreviewToStream(input, best, outputLarge);
	}

	internal static bool TryParseTiffHeader(Stream s, out bool littleEndian,
		out uint firstIfdOffset)
	{
		littleEndian = false;
		firstIfdOffset = 0;

		Span<byte> header = stackalloc byte[8];
		if ( s.Read(header) < 8 )
		{
			return false;
		}

		// Check byte order (TIFF standard: 'II' for little-endian, 'MM' for big-endian)
		littleEndian = header[0] == 'I' && header[1] == 'I';
		if ( !littleEndian && !( header[0] == 'M' && header[1] == 'M' ) )
		{
			return false;
		}

		// Check magic number (42) - TIFF standard magic constant
		var magic = ReadUInt16(header[2..], littleEndian);
		if ( magic != 42 )
		{
			return false;
		}

		// Read first IFD offset
		firstIfdOffset = ReadUInt32(header[4..], littleEndian);
		return firstIfdOffset > 0 && firstIfdOffset <= s.Length;
	}

	internal static void ParseIfdRecursive(Stream input, uint offset, bool littleEndian,
		ParseTraversalContext context, int depth, bool isSubIfd)
	{
		// Check depth and preview capacity limits
		if ( depth > MaxIfdDepth || context.Previews.Count >= MaxPreviews )
		{
			return;
		}

		// offset = 0 is a sentinel value meaning "no more IFDs" - return early without visiting
		if ( offset == 0 )
		{
			return;
		}

		// Try to mark this offset as visited to prevent cycles
		if ( !TryMarkVisited(context.Visited, offset) )
		{
			return;
		}

		if ( !StreamPrimitives.TrySeek(input, offset) )
		{
			return;
		}

		Span<byte> countBuf = stackalloc byte[2];
		if ( input.Read(countBuf) < 2 )
		{
			return;
		}

		var entryCount = ReadUInt16(countBuf, littleEndian);
		if ( entryCount == 0 || entryCount > 10000 )
		{
			return;
		}

		// Early bounds check
		var entryBytes = ( long ) entryCount * 12;
		if ( !StreamPrimitives.TryGetRemainingBytes(input, out var remaining) ||
		     entryBytes + 4 > remaining )
		{
			return;
		}

		var entryBuf = ArrayPool<byte>.Shared.Rent(( int ) entryBytes);

		try
		{
			if ( input.Read(entryBuf, 0, ( int ) entryBytes) < entryBytes )
			{
				return;
			}

			ParseIfdEntries(input, entryBuf.AsSpan(0, ( int ) entryBytes), littleEndian,
				context, depth);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(entryBuf);
		}

		ParseNextIfd(input, littleEndian, context, depth, isSubIfd);
	}


	private static bool TryMarkVisited(HashSet<uint> visited, uint offset)
	{
		if ( visited.Count >= MaxIfdVisits )
		{
			return false;
		}

		return visited.Add(offset);
	}

	internal static void ParseNextIfd(Stream input, bool littleEndian,
		ParseTraversalContext context,
		int depth, bool isSubIfd)
	{
		if ( isSubIfd )
		{
			return;
		}

		Span<byte> nextBuf = stackalloc byte[4];
		if ( input.Read(nextBuf) != 4 )
		{
			return;
		}

		var nextIfd = ReadUInt32(nextBuf, littleEndian);
		if ( nextIfd == 0 || depth >= MaxRootIfdChain )
		{
			return;
		}

		ParseIfdRecursive(input, nextIfd, littleEndian, context, depth + 1, false);
	}

	private static void ParseIfdEntries(Stream input, ReadOnlySpan<byte> entries, bool littleEndian,
		ParseTraversalContext context, int depth)
	{
		var state = new IfdEntryState();
		var subIfdOffsets = new List<uint>(4);

		var count = entries.Length / 12;

		for ( var i = 0; i < count; i++ )
		{
			HandleIfdEntry(input, entries.Slice(i * 12, 12), littleEndian, subIfdOffsets,
				state);
		}

		AppendDirectJpegCandidate(context.Previews, state, input, context.RawFlavor);
		TryParseMakerNoteCandidate(input, littleEndian, context, state);
		ParseSubIfdChain(input, littleEndian, context, depth, subIfdOffsets);
	}

	internal static void HandleIfdEntry(Stream input, ReadOnlySpan<byte> entry, bool littleEndian,
		List<uint> subIfdOffsets, IfdEntryState state)
	{
		var tag = ReadUInt16(entry, littleEndian);
		var type = ReadUInt16(entry[2..], littleEndian);
		var n = ReadUInt32(entry[4..], littleEndian);
		var value = ReadUInt32(entry[8..], littleEndian);

		switch ( tag )
		{
			case TagCompression when n == 1:
				state.IfdCompression = ReadScalarValue(type, value, littleEndian);
				return;
			case TagImageWidth when n == 1:
				state.IfdWidth = ReadScalarValue(type, value, littleEndian);
				return;
			case TagImageLength when n == 1:
				state.IfdHeight = ReadScalarValue(type, value, littleEndian);
				return;
			case TagStripOffsets when n == 1:
				state.StripOffset = value;
				state.HasStrip = true;
				return;
			case TagStripByteCounts when n == 1:
				state.StripLength = value;
				return;
			case TagJpegOffset:
				state.JpegOffset = value;
				state.HasJpeg = true;
				return;
			case TagJpegLength:
				state.JpegLength = value;
				return;
			case TagSubIfds:
				AddSubIfdOffsets(input, littleEndian, subIfdOffsets, type, n, value);
				return;
				case TagExifIfd when n == 1 && value > 0:
					subIfdOffsets.Add(value);
					return;
			case TagMakerNote when n > 4 && value > 0:
				state.HasMakerNote = true;
				state.MakerNoteOffset = value;
				state.MakerNoteLength = n;
				return;
		}
	}

	internal static void AddSubIfdOffsets(Stream input, bool littleEndian,
		List<uint> subIfdOffsets, ushort type, uint n, uint value)
	{
		if ( n == 1 )
		{
			subIfdOffsets.Add(value);
			return;
		}

		var boundedCount =
			StreamPrimitives.ClampIndirectCount(input, value, type, n, MaxSubIfdOffsets);
		StreamPrimitives.ReadIndirectOffsets(input, value, type, boundedCount, littleEndian,
			subIfdOffsets);
	}

	internal static void AppendDirectJpegCandidate(List<PreviewCandidate> previews,
		IfdEntryState state, Stream input, RawFlavor rawFlavor)
	{
		// Strip-based JPEG (Canon CR2 IFD0: 0x0111 / 0x0117, count=1)
		// Canon CR2 IFD3/IFD4 use strip tags for raw lossless data; reject those markers.
		var shouldRejectCanonLosslessStrip = rawFlavor == RawFlavor.CanonCr2 &&
		                                     IsLosslessJpegAtOffset(input, state.StripOffset);
		if ( IsJpegCompression(state.IfdCompression) && state is
			     { HasStrip: true, StripOffset: > 0, StripLength: >= MinJpegSize } &&
		     !shouldRejectCanonLosslessStrip )
		{
			previews.Add(new PreviewCandidate
			{
				Offset = state.StripOffset,
				Length = state.StripLength,
				Width = state.IfdWidth,
				Height = state.IfdHeight
			});
		}

		// Standard JPEG-in-IFD (0x0201 / 0x0202)
		if ( !state.HasJpeg || state.JpegOffset == 0 || state.JpegLength < MinJpegSize )
		{
			return;
		}

		previews.Add(new PreviewCandidate
		{
			Offset = state.JpegOffset,
			Length = state.JpegLength,
			Width = state.IfdWidth,
			Height = state.IfdHeight
		});
	}

	internal static bool IsJpegCompression(uint compression)
	{
		// TIFF JPEG old-style (6) and new-style (7)
		return compression is 6 or 7;
	}

	private static void TryParseMakerNoteCandidate(Stream input, bool littleEndian,
		ParseTraversalContext context, IfdEntryState state)
	{
		if ( !state.HasMakerNote || context.Previews.Count >= MaxPreviews )
		{
			return;
		}

		ParseMakerNote(input, littleEndian, context.RawFlavor, state.MakerNoteOffset,
			state.MakerNoteLength,
			context.Previews);
	}

	internal static bool ParseSubIfdChain(Stream input, bool littleEndian,
		ParseTraversalContext context, int depth, List<uint> subIfdOffsets)
	{
		foreach ( var subIfdOffset in subIfdOffsets )
		{
			if ( context.Previews.Count >= MaxPreviews )
			{
				return false;
			}

			ParseIfdRecursive(input, subIfdOffset, littleEndian, context, depth + 1, true);
		}

		return true;
	}

	internal static void ParseMakerNote(Stream input, bool littleEndian, RawFlavor rawFlavor,
		uint makerNoteOffset, uint makerNoteLength, List<PreviewCandidate> previews)
	{
		if ( makerNoteOffset == 0 || makerNoteLength == 0 )
		{
			return;
		}

		var boundedLength = Math.Min(makerNoteLength, MaxMakerNoteScanBytes);
		if ( makerNoteOffset + boundedLength > input.Length )
		{
			boundedLength = makerNoteOffset >= input.Length
				? 0
				: ( uint ) ( input.Length - makerNoteOffset );
		}

		if ( boundedLength == 0 )
		{
			return;
		}

		switch ( rawFlavor )
		{
			case RawFlavor.SonyArw:
				ParseSonyMakerNote(input, makerNoteOffset, boundedLength, littleEndian, previews);
				break;
			case RawFlavor.CanonCr2:
				ParseCanonMakerNote(input, makerNoteOffset, boundedLength, littleEndian, previews);
				break;
		}
	}

	internal static void ParseSonyMakerNote(Stream input, uint makerNoteOffset,
		uint makerNoteLength,
		bool littleEndian, List<PreviewCandidate> previews)
	{
		var (hasPair, rawOffset, rawLength) = ReadIfdTagPair(input, makerNoteOffset,
			makerNoteLength,
			new IfdTagPairQuery(TagSonyPreviewOffset, TagSonyPreviewLength, TagSonyPreviewAlt,
				littleEndian));

		if ( hasPair &&
		     TryResolveMakerNoteOffset(input, makerNoteOffset, rawOffset, out var resolvedOffset) )
		{
			var resolvedLength = rawLength;
			if ( resolvedLength < MinJpegSize )
			{
				resolvedLength = JpegScannerUtilities.DetectJpegLengthFromStart(input,
					resolvedOffset,
					Math.Min(MaxMakerNoteScanBytes, ( int ) ( input.Length - resolvedOffset )));
			}

			if ( resolvedLength >= MinJpegSize )
			{
				previews.Add(new PreviewCandidate
				{
					Offset = resolvedOffset, Length = resolvedLength
				});
			}
		}
	}

	internal static void ParseCanonMakerNote(Stream input, uint makerNoteOffset,
		uint makerNoteLength,
		bool littleEndian, List<PreviewCandidate> previews)
	{
		var queries = new[]
		{
			new IfdTagPairQuery(TagCanonPreviewOffset, TagCanonPreviewLength, 0, littleEndian),
			new IfdTagPairQuery(TagJpegOffset, TagJpegLength, 0, littleEndian),
			new IfdTagPairQuery(TagStripOffsets, TagStripByteCounts, 0, littleEndian)
		};

		var foundExplicitCandidate = false;
		var seenCandidates = new HashSet<ulong>();
		var ifdOffsets = GetCanonMakerNoteIfdOffsets(input, makerNoteOffset, makerNoteLength,
			littleEndian);
		foreach ( var query in queries )
		{
			foreach ( var ifdOffset in ifdOffsets )
			{
				var ifdBlockLength = ( uint ) ( ( ulong ) makerNoteOffset + makerNoteLength -
				                                ifdOffset );
				var (hasPair, rawOffset, rawLength) = ReadIfdTagPair(input, ifdOffset,
					ifdBlockLength,
					query);

				if ( !hasPair )
				{
					continue;
				}

				if ( !TryResolveMakerNoteOffset(input, makerNoteOffset, rawOffset,
					     out var resolvedOffset) &&
				     !TryResolveMakerNoteOffset(input, ifdOffset, rawOffset,
					     out resolvedOffset) )
				{
					continue;
				}

				var resolvedLength = rawLength;
				if ( resolvedLength < MinJpegSize )
				{
					resolvedLength = JpegScannerUtilities.DetectJpegLengthFromStart(input,
						resolvedOffset,
						Math.Min(MaxMakerNoteScanBytes,
							( int ) ( input.Length - resolvedOffset )));
				}

				if ( resolvedLength < MinJpegSize )
				{
					continue;
				}

				if ( !seenCandidates.Add(( ( ulong ) resolvedOffset << 32 ) | resolvedLength) )
				{
					continue;
				}

				foundExplicitCandidate = true;
				previews.Add(new PreviewCandidate
				{
					Offset = resolvedOffset, Length = resolvedLength
				});

				if ( previews.Count >= MaxPreviews )
				{
					return;
				}
			}
		}

		if ( foundExplicitCandidate )
		{
			return;
		}

		var boundedCanonScan = Math.Min(makerNoteLength, CanonFallbackScanBytes);
		foreach ( var candidate in ScanJpegsInRange(input, makerNoteOffset, boundedCanonScan) )
		{
			previews.Add(candidate!);
			if ( previews.Count >= MaxPreviews )
			{
				break;
			}
		}
	}

	internal static List<uint> GetCanonMakerNoteIfdOffsets(Stream input, uint makerNoteOffset,
		uint makerNoteLength, bool littleEndian)
	{
		var offsets = new List<uint> { makerNoteOffset };
		if ( makerNoteLength <= 6 )
		{
			return offsets;
		}

		var maxProbe = Math.Min(CanonMakerNoteIfdProbeBytes, makerNoteLength - 6);
		for ( uint delta = 1; delta <= maxProbe; delta++ )
		{
			var ifdOffset = makerNoteOffset + delta;
			var ifdBlockLength = makerNoteLength - delta;
			if ( ifdBlockLength < 6 )
			{
				break;
			}

			if ( !TryReadIfdEntryHeader(input, ifdOffset, ifdBlockLength, littleEndian, out _,
				    out _) )
			{
				continue;
			}

			offsets.Add(ifdOffset);
		}

		return offsets;
	}

	internal static (bool HasPair, uint CandidateOffset, uint CandidateLength) ReadIfdTagPair(
		Stream input,
		uint ifdOffset,
		uint blockLength,
		IfdTagPairQuery query)
	{
		if ( !TryReadIfdEntryHeader(input, ifdOffset, blockLength, query.LittleEndian,
			    out var entryCount, out var entryBytes) )
		{
			return ( false, 0, 0 );
		}

		var buf = ArrayPool<byte>.Shared.Rent(entryBytes);
		try
		{
			if ( !TryReadFull(input, buf, entryBytes) )
			{
				return ( false, 0, 0 );
			}

			var (candidateOffset, candidateLength) = ExtractTagPairValues(
				buf.AsSpan(0, entryBytes), entryCount, query);
			return ( candidateOffset > 0, candidateOffset, candidateLength );
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buf);
		}
	}

	internal static bool TryReadIfdEntryHeader(Stream input, uint ifdOffset, uint blockLength,
		bool littleEndian, out ushort entryCount, out int entryBytes)
	{
		entryCount = 0;
		entryBytes = 0;

		if ( !StreamPrimitives.TrySeek(input, ifdOffset) || blockLength < 6 )
		{
			return false;
		}

		Span<byte> countBuf = stackalloc byte[2];
		if ( input.Read(countBuf) < 2 )
		{
			return false;
		}

		entryCount = ReadUInt16(countBuf, littleEndian);
		if ( entryCount is 0 or > 512 )
		{
			return false;
		}

		entryBytes = entryCount * 12;
		return entryBytes + 6 <= blockLength;
	}

	private static bool TryReadFull(Stream input, byte[] buffer, int bytesToRead)
	{
		return input.Read(buffer, 0, bytesToRead) >= bytesToRead;
	}

	internal static (uint Offset, uint Length) ExtractTagPairValues(ReadOnlySpan<byte> entries,
		int entryCount,
		IfdTagPairQuery query)
	{
		var candidateOffset = 0u;
		var candidateLength = 0u;

		for ( var i = 0; i < entryCount; i++ )
		{
			var e = entries.Slice(i * 12, 12);
			var tag = ReadUInt16(e, query.LittleEndian);
			var type = ReadUInt16(e[2..], query.LittleEndian);
			var n = ReadUInt32(e[4..], query.LittleEndian);
			if ( n != 1 )
			{
				continue;
			}

			var value = ReadScalarValue(type, ReadUInt32(e[8..], query.LittleEndian),
				query.LittleEndian);
			if ( tag == query.PrimaryOffsetTag )
			{
				candidateOffset = value;
				continue;
			}

			if ( tag == query.PrimaryLengthTag )
			{
				candidateLength = value;
				continue;
			}

			if ( query.AltTag == 0 || tag != query.AltTag )
			{
				continue;
			}

			if ( candidateOffset == 0 )
			{
				candidateOffset = value;
			}
			else if ( candidateLength == 0 )
			{
				candidateLength = value;
			}
		}

		return ( candidateOffset, candidateLength );
	}

	internal static bool TryResolveMakerNoteOffset(Stream input, uint makerNoteBase,
		uint rawOffset, out uint resolvedOffset)
	{
		resolvedOffset = 0;
		if ( rawOffset == 0 )
		{
			return false;
		}

		if ( rawOffset < input.Length && IsJpegAtOffset(input, rawOffset) )
		{
			resolvedOffset = rawOffset;
			return true;
		}

		var relative = makerNoteBase + rawOffset;
		if ( relative < input.Length && IsJpegAtOffset(input, relative) )
		{
			resolvedOffset = relative;
			return true;
		}

		return false;
	}

	internal static bool IsJpegAtOffset(Stream input, uint offset)
	{
		if ( !StreamPrimitives.TrySeek(input, offset) )
		{
			return false;
		}

		Span<byte> header = stackalloc byte[3];
		if ( input.Read(header) < 3 )
		{
			return false;
		}

		return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
	}

	// Read minimal JPEG SOF to discover width/height for a JPEG located at `offset` with known `length`.
	internal static bool TryGetJpegDimensionsAtOffset(Stream input, uint offset, uint length,
		out uint width, out uint height)
	{
		width = 0;
		height = 0;
		if ( length < MinJpegSize )
		{
			return false;
		}

		if ( !StreamPrimitives.TrySeek(input, offset) )
		{
			return false;
		}

		var buf = new byte[4096];
		var toRead = ( int ) Math.Min(( uint ) buf.Length, length);
		if ( input.Read(buf, 0, toRead) < 2 )
		{
			return false;
		}

		// quick SOI check
		if ( buf[0] != 0xFF || buf[1] != 0xD8 )
		{
			return false;
		}

		var pos = 2;
		while ( TryFindNextMarker(buf, toRead, ref pos, out var marker, out var segLen) )
		{
			if ( IsSofMarker(marker) )
			{
				// attempt in-buffer parse
				if ( TryParseSofFromBuffer(buf, toRead, pos, segLen, out width, out height) )
				{
					return true;
				}

				// fallback: read segment from stream and parse
				var segStart = offset + pos + 2;
				if ( ReadSegmentAndParseSof(input, segStart, segLen, out width, out height) )
				{
					return width > 0 && height > 0;
				}

				return false;
			}

			pos += 2 + Math.Max(0, segLen);
		}

		return false;
	}

	private static bool IsSofMarker(int marker)
	{
		// SOF marker family contains frame header with height/width
		return marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA
			or 0xCB or 0xCD or 0xCE or 0xCF;
	}

	private static bool TryParseSofFromBuffer(byte[] buf, int toRead, int pos, int segLen,
		out uint width, out uint height)
	{
		width = 0;
		height = 0;
		// Need at least 7 bytes of payload for SOF: [precision(1)] [height(2)] [width(2)] and ensure the
		// buffer contains the full segment payload (pos points at marker start).
		if ( segLen < 7 || ( long ) pos + 2 + segLen > toRead )
		{
			return false;
		}

		// payload layout: [length(2)] [precision(1)] [height(2)] [width(2)] ...
		height = ( uint ) ( ( buf[pos + 5] << 8 ) | buf[pos + 6] );
		width = ( uint ) ( ( buf[pos + 7] << 8 ) | buf[pos + 8] );
		return width > 0 && height > 0;
	}

	private static bool ReadSegmentAndParseSof(Stream input, long segmentStart, int segLen,
		out uint width, out uint height)
	{
		width = 0;
		height = 0;
		if ( !StreamPrimitives.TrySeek(input, segmentStart) )
		{
			return false;
		}

		var seg = new byte[segLen];
		if ( input.Read(seg, 0, segLen) < segLen )
		{
			return false;
		}

		// seg is read starting at the length bytes; the payload follows

		height = ( uint ) ( ( seg[1] << 8 ) | seg[2] );
		width = ( uint ) ( ( seg[3] << 8 ) | seg[4] );
		return width > 0 && height > 0;
	}

	private static bool TryFindNextMarker(byte[] buf, int toRead, ref int pos, out int marker,
		out int segLen)
	{
		marker = -1;
		segLen = 0;
		// allow cases where we can read marker bytes (pos+1) even if the 2-byte length
		// may not yet be present in the buffer; the subsequent check handles that.
		while ( pos + 2 <= toRead )
		{
			if ( buf[pos] != 0xFF )
			{
				pos++;
				continue;
			}

			marker = buf[pos + 1] & 0xFF;
			switch ( marker )
			{
				case 0xFF:
					pos++;
					continue;
				case 0xD8:
				case 0xD9:
					pos += 2;
					continue;
			}

			if ( pos + 4 > toRead )
			{
				return false;
			}

			segLen = ( buf[pos + 2] << 8 ) | buf[pos + 3];
			return true;
		}

		return false;
	}
}
