using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

/// <summary>
///     Extracts embedded JPEG previews from TIFF-based RAW formats.
///     Supports: DNG (Adobe), CR2 (Canon EOS), NEF (Nikon), ARW (Sony)
///     High-performance implementation focusing on metadata extraction only.
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

	// TIFF IFD Tags
	private const ushort TagImageWidth = 0x0100;
	private const ushort TagImageLength = 0x0101;
	private const ushort TagCompression = 0x0103;
	private const ushort TagJpegOffset = 0x0201;
	private const ushort TagJpegLength = 0x0202;
	private const ushort TagSubIfds = 0x014A;
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
		string? outputLargePath)
	{
		if (
			!_subPathStorage.ExistFile(subPathRawFile) )
		{
			return false;
		}

		try
		{
			await using var input = _subPathStorage.ReadStream(subPathRawFile);
			await using var output = outputLargePath != null ? new MemoryStream() : null;
			var rawFlavor = RawFlavorHelper.GetRawFlavorFromPath(subPathRawFile);

			var ok = await TryExtractFromStream(input, output,
				$"Reference: {subPathRawFile}", rawFlavor);
			if ( !ok || outputLargePath == null || output == null )
			{
				return ok;
			}

			output.Seek(0, SeekOrigin.Begin);
			return await _tempStorage.WriteStreamAsync(output, outputLargePath);
		}
		catch ( Exception ex )
		{
			_logger.LogDebug(
				$"[EmbeddedPreviewExtractor] Failed to extract from {subPathRawFile}: {ex.Message}");
			return false;
		}
	}

	private async Task<bool> TryExtractFromStream(Stream input, Stream? outputLarge,
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


		// Select and extract best preview
		var best = SelectBestPreview(candidates);
		if ( best == null )
		{
			return false;
		}

		return await ExtractPreviewToStream(input, best, outputLarge);
	}

	private static bool TryParseTiffHeader(Stream s, out bool littleEndian, out uint firstIfdOffset)
	{
		littleEndian = false;
		firstIfdOffset = 0;

		Span<byte> header = stackalloc byte[8];
		if ( s.Read(header) < 8 )
		{
			return false;
		}

		// Check byte order
		littleEndian = header[0] == 'I' && header[1] == 'I';
		if ( !littleEndian && !( header[0] == 'M' && header[1] == 'M' ) )
		{
			return false;
		}

		// Check magic number (42) - this is not a joke
		var magic = ReadUInt16(header[2..], littleEndian);
		if ( magic != 42 )
		{
			return false;
		}

		// Read first IFD offset
		firstIfdOffset = ReadUInt32(header[4..], littleEndian);
		return firstIfdOffset > 0 && firstIfdOffset < s.Length;
	}

	private void ParseIfdRecursive(Stream input, uint offset, bool littleEndian,
		ParseTraversalContext context, int depth, bool isSubIfd)
	{
		if ( ShouldStopTraversal(context, offset, depth) )
		{
			return;
		}

		if ( !TryMarkVisited(context.Visited, offset) )
		{
			return;
		}

		if ( !TrySeek(input, offset) )
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
		if ( !TryGetRemainingBytes(input, out var remaining) || entryBytes + 4 > remaining )
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

	private static bool ShouldStopTraversal(ParseTraversalContext context, uint offset, int depth)
	{
		return depth > MaxIfdDepth || offset == 0 || context.Previews.Count >= MaxPreviews;
	}

	private static bool TryMarkVisited(HashSet<uint> visited, uint offset)
	{
		return visited.Add(offset) && visited.Count < MaxIfdVisits;
	}

	private void ParseNextIfd(Stream input, bool littleEndian, ParseTraversalContext context,
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

	private void ParseIfdEntries(Stream input, ReadOnlySpan<byte> entries, bool littleEndian,
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

	private static void HandleIfdEntry(Stream input, ReadOnlySpan<byte> entry, bool littleEndian,
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
			case TagMakerNote when n > 4 && value > 0:
				state.HasMakerNote = true;
				state.MakerNoteOffset = value;
				state.MakerNoteLength = n;
				return;
		}
	}

	private static void AddSubIfdOffsets(Stream input, bool littleEndian,
		List<uint> subIfdOffsets, ushort type, uint n, uint value)
	{
		if ( n == 1 )
		{
			subIfdOffsets.Add(value);
			return;
		}

		var boundedCount = ClampIndirectCount(input, value, type, n, MaxSubIfdOffsets);
		ReadIndirectOffsets(input, value, type, boundedCount, littleEndian, subIfdOffsets);
	}

	private static void AppendDirectJpegCandidate(List<PreviewCandidate> previews,
		IfdEntryState state, Stream input, RawFlavor rawFlavor)
	{
		// Strip-based JPEG (Canon CR2 IFD0: 0x0111 / 0x0117, count=1)
		// Canon CR2 IFD3/IFD4 use strip tags for raw lossless data; reject those markers.
		var shouldRejectCanonLosslessStrip = rawFlavor == RawFlavor.CanonCr2 &&
		                                     IsLosslessJpegAtOffset(input, state.StripOffset);
		if ( IsJpegCompression(state.IfdCompression) && state.HasStrip && state.StripOffset > 0 &&
		     state.StripLength >= MinJpegSize &&
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

	private static bool IsJpegCompression(uint compression)
	{
		// TIFF JPEG old-style (6) and new-style (7)
		return compression is 6 or 7;
	}

	private void TryParseMakerNoteCandidate(Stream input, bool littleEndian,
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

	private void ParseSubIfdChain(Stream input, bool littleEndian,
		ParseTraversalContext context, int depth, List<uint> subIfdOffsets)
	{
		foreach ( var subIfdOffset in subIfdOffsets )
		{
			if ( context.Previews.Count >= MaxPreviews )
			{
				return;
			}

			ParseIfdRecursive(input, subIfdOffset, littleEndian, context, depth + 1, true);
		}
	}

	private void ParseMakerNote(Stream input, bool littleEndian, RawFlavor rawFlavor,
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

	private static void ParseSonyMakerNote(Stream input, uint makerNoteOffset, uint makerNoteLength,
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
				resolvedLength = DetectJpegLengthByEoi(input, resolvedOffset,
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

	private static void ParseCanonMakerNote(Stream input, uint makerNoteOffset,
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
		foreach ( var query in queries )
		{
			var (hasPair, rawOffset, rawLength) = ReadIfdTagPair(input, makerNoteOffset,
				makerNoteLength,
				query);

			if ( !hasPair ||
			     !TryResolveMakerNoteOffset(input, makerNoteOffset, rawOffset,
				     out var resolvedOffset) )
			{
				continue;
			}

			var resolvedLength = rawLength;
			if ( resolvedLength < MinJpegSize )
			{
				resolvedLength = DetectJpegLengthByEoi(input, resolvedOffset,
					Math.Min(MaxMakerNoteScanBytes, ( int ) ( input.Length - resolvedOffset )));
			}

			if ( resolvedLength < MinJpegSize )
			{
				continue;
			}

			foundExplicitCandidate = true;
			previews.Add(new PreviewCandidate { Offset = resolvedOffset, Length = resolvedLength });

			if ( previews.Count >= MaxPreviews )
			{
				return;
			}
		}

		if ( foundExplicitCandidate )
		{
			return;
		}

		var boundedCanonScan = Math.Min(makerNoteLength, CanonFallbackScanBytes);
		foreach ( var candidate in ScanJpegsInRange(input, makerNoteOffset, boundedCanonScan) )
		{
			previews.Add(candidate);
			if ( previews.Count >= MaxPreviews )
			{
				break;
			}
		}
	}

	private static (bool HasPair, uint CandidateOffset, uint CandidateLength) ReadIfdTagPair(
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

	private static bool TryReadIfdEntryHeader(Stream input, uint ifdOffset, uint blockLength,
		bool littleEndian, out ushort entryCount, out int entryBytes)
	{
		entryCount = 0;
		entryBytes = 0;

		if ( !TrySeek(input, ifdOffset) || blockLength < 6 )
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

	private static (uint Offset, uint Length) ExtractTagPairValues(ReadOnlySpan<byte> entries,
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

	private static bool TryResolveMakerNoteOffset(Stream input, uint makerNoteBase,
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

	private static bool IsJpegAtOffset(Stream input, uint offset)
	{
		if ( !TrySeek(input, offset) )
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
}
