using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Extracts embedded JPEG previews from TIFF-based RAW formats.
///     Supports: DNG (Adobe), CR2 (Canon EOS), NEF (Nikon), ARW (Sony)
///     High-performance implementation focusing on metadata extraction only.
/// </summary>
public class EmbeddedPreviewExtractor
{
	private readonly IWebLogger _logger;
	private readonly IStorage _subPathStorage;
	private readonly IStorage _tempStorage;

	public EmbeddedPreviewExtractor(IWebLogger logger, ISelectorStorage selectorStorage)
	{
		_logger = logger;
		_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_tempStorage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
	}

	private const int MaxIfdDepth = 6;
	private const int MaxIfdVisits = 64;
	private const int MaxRootIfdChain = 6;
	private const int MaxSubIfdOffsets = 32;
	private const int MaxPreviews = 8;
	private const int MinJpegSize = 4096; // 4KB minimum for valid JPEG

	// TIFF IFD Tags
	private const ushort TagImageWidth = 0x0100;
	private const ushort TagImageLength = 0x0101;
	private const ushort TagJpegOffset = 0x0201;
	private const ushort TagJpegLength = 0x0202;
	private const ushort TagSubIfds = 0x014A;
	private const ushort TagMakerNote = 0x927C;


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
			using var input = _subPathStorage.ReadStream(subPathRawFile);
			await using var output = outputLargePath != null ? new MemoryStream() : null;

			var ok = await TryExtract(input, output, $"Reference: {subPathRawFile}");
			if ( !ok || outputLargePath == null )
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

	public async Task<bool> TryExtract(Stream input, Stream? outputLarge,
		string referenceInfo = "Reference: stream")
	{
		if ( input.CanSeek )
		{
			return await TryExtractFromStream(input, outputLarge, referenceInfo);
		}

		_logger.LogDebug(
			$"[EmbeddedPreviewExtractor] Input stream must support seek. {referenceInfo}");
		return false;

	}

	private async Task<bool> TryExtractFromStream(Stream input, Stream? outputLarge, string referenceInfo)
	{
		var candidates = new List<PreviewCandidate>();

		// Parse TIFF header
		if ( !TryParseTiffHeader(input, out var littleEndian, out var firstIfdOffset) )
		{
			return false;
		}

		// Traverse IFD structure
		var visited = new HashSet<uint>();
		ParseIfdRecursive(input, firstIfdOffset, littleEndian, candidates, visited, 0, false,
			referenceInfo);

		if ( candidates.Count == 0 )
		{
			return false;
		}

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
		List<PreviewCandidate> previews, HashSet<uint> visited, int depth, bool isSubIfd,
		string referenceInfo)
	{
		if ( depth > MaxIfdDepth || offset == 0 || previews.Count >= MaxPreviews )
		{
			return;
		}

		if ( !visited.Add(offset) || visited.Count >= MaxIfdVisits )
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

			ParseIfdEntries(input, entryBuf.AsSpan(0, ( int ) entryBytes), littleEndian, previews,
				visited, depth, referenceInfo);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(entryBuf);
		}

		// Next IFD pointer (only for root IFD chain)
		if ( !isSubIfd )
		{
			Span<byte> nextBuf = stackalloc byte[4];
			if ( input.Read(nextBuf) == 4 )
			{
				var nextIfd = ReadUInt32(nextBuf, littleEndian);
				if ( nextIfd != 0 && depth < MaxRootIfdChain )
				{
					ParseIfdRecursive(input, nextIfd, littleEndian, previews, visited, depth + 1,
						false, referenceInfo);
				}
			}
		}
	}

	private void ParseIfdEntries(Stream input, ReadOnlySpan<byte> entries, bool littleEndian,
		List<PreviewCandidate> previews, HashSet<uint> visited, int depth,
		string referenceInfo)
	{
		uint jpegOffset = 0;
		uint jpegLength = 0;
		uint ifdWidth = 0;
		uint ifdHeight = 0;
		var hasJpeg = false;
		var subIfdOffsets = new List<uint>(4);

		var count = entries.Length / 12;

		for ( var i = 0; i < count; i++ )
		{
			var e = entries.Slice(i * 12, 12);

			var tag = ReadUInt16(e, littleEndian);
			var type = ReadUInt16(e[2..], littleEndian);
			var n = ReadUInt32(e[4..], littleEndian);
			var value = ReadUInt32(e[8..], littleEndian);

			switch ( tag )
			{
				case TagImageWidth:
					if ( n == 1 )
					{
						ifdWidth = ReadScalarValue(type, value);
					}

					break;

				case TagImageLength:
					if ( n == 1 )
					{
						ifdHeight = ReadScalarValue(type, value);
					}

					break;

				case TagJpegOffset:
					jpegOffset = value;
					hasJpeg = true;
					break;

				case TagJpegLength:
					jpegLength = value;
					break;

				case TagSubIfds:
					if ( n == 1 )
					{
						subIfdOffsets.Add(value);
					}
					else
					{
						var boundedCount = ClampIndirectCount(input, value, type, n,
							MaxSubIfdOffsets);
						ReadIndirectOffsets(input, value, type, boundedCount, littleEndian,
							subIfdOffsets);
					}

					break;
			}
		}

		// Add JPEG preview if found
		if ( hasJpeg && jpegOffset > 0 && jpegLength >= MinJpegSize )
		{
			previews.Add(new PreviewCandidate
			{
				Offset = jpegOffset,
				Length = jpegLength,
				Width = ifdWidth,
				Height = ifdHeight
			});
		}

		// Process SubIFDs (these are thumbnail/preview IFDs)
		foreach ( var subIfdOffset in subIfdOffsets )
		{
			if ( previews.Count >= MaxPreviews )
			{
				break;
			}

			ParseIfdRecursive(input, subIfdOffset, littleEndian, previews, visited, depth + 1,
				true, referenceInfo);
		}
	}

	private static PreviewCandidate? SelectBestPreview(List<PreviewCandidate> candidates)
	{
		if ( candidates.Count == 0 )
		{
			return null;
		}

		PreviewCandidate? best = null;

		foreach ( var candidate in candidates )
		{
			if ( best == null )
			{
				best = candidate;
				continue;
			}

			var candidatePixels = ( ulong ) candidate.Width * candidate.Height;
			var bestPixels = ( ulong ) best.Width * best.Height;

			// Prefer higher resolution preview when dimensions are available; fallback to byte size.
			if ( candidatePixels > bestPixels ||
			     ( candidatePixels == bestPixels && candidate.Length > best.Length ) )
			{
				best = candidate;
			}
		}

		return best;
	}

	private static async Task<bool> ExtractPreviewToStream(Stream input, PreviewCandidate preview,
		Stream? output)
	{
		if ( !TryValidateJpegOffset(input, preview.Offset, preview.Length) )
		{
			return false;
		}

		if ( output == null )
		{
			return true; // Success, just not saving
		}

		try
		{
			if ( !TrySeek(input, preview.Offset) )
			{
				return false;
			}

			var buffer = ArrayPool<byte>.Shared.Rent(65536);
			try
			{
				var remaining = ( long ) preview.Length;
				while ( remaining > 0 )
				{
					var toRead = ( int ) Math.Min(65536, remaining);
					var read = input.Read(buffer, 0, toRead);
					if ( read == 0 )
					{
						break;
					}

					await output.WriteAsync(buffer.AsMemory(0, read));
					remaining -= read;
				}

				return remaining == 0;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
		catch
		{
			return false;
		}
	}

	private static bool TryValidateJpegOffset(Stream s, uint offset, uint length)
	{
		if ( offset + length > s.Length )
		{
			return false;
		}

		// Check JPEG SOI marker
		if ( !TrySeek(s, offset) )
		{
			return false;
		}

		Span<byte> marker = stackalloc byte[3];
		if ( s.Read(marker) < 3 )
		{
			return false;
		}

		// JPEG should start with 0xFFD8FF
		return marker[0] == 0xFF && marker[1] == 0xD8 && marker[2] == 0xFF;
	}

	private static bool TrySeek(Stream s, uint offset)
	{
		if ( !s.CanSeek )
		{
			return false;
		}

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

	private static bool TryGetRemainingBytes(Stream s, out long remaining)
	{
		remaining = 0;

		if ( !s.CanSeek )
		{
			return false;
		}

		try
		{
			remaining = s.Length - s.Position;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static uint ClampIndirectCount(Stream s, uint offset, ushort type, uint requested,
		uint hardCap)
	{
		if ( requested == 0 )
		{
			return 0;
		}

		var bounded = Math.Min(requested, hardCap);
		var bytesPerValue = type == 3 ? 2u : 4u;

		try
		{
			if ( offset >= s.Length )
			{
				return 0;
			}

			var availableBytes = ( ulong ) ( s.Length - offset );
			var maxFromFile = availableBytes / bytesPerValue;
			return ( uint ) Math.Min(bounded, maxFromFile);
		}
		catch
		{
			return bounded;
		}
	}

	private static void ReadIndirectOffsets(Stream s, uint offset, ushort type, uint count,
		bool littleEndian, List<uint> offsets)
	{
		if ( count == 0 || !TrySeek(s, offset) )
		{
			return;
		}

		var bytesNeeded = ( int ) count * ( type == 3 ? 2 : 4 );
		var buf = ArrayPool<byte>.Shared.Rent(bytesNeeded);

		try
		{
			if ( s.Read(buf, 0, bytesNeeded) < bytesNeeded )
			{
				return;
			}

			for ( var i = 0; i < count; i++ )
			{
				var val = type == 3
					? ReadUInt16(buf.AsSpan(i * 2, 2), littleEndian)
					: ReadUInt32(buf.AsSpan(i * 4, 4), littleEndian);
				offsets.Add(val);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buf);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ushort ReadUInt16(ReadOnlySpan<byte> b, bool littleEndian)
	{
		return littleEndian
			? ( ushort ) ( b[0] | ( b[1] << 8 ) )
			: ( ushort ) ( ( b[0] << 8 ) | b[1] );
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ReadUInt32(ReadOnlySpan<byte> b, bool littleEndian)
	{
		return littleEndian
			? ( uint ) ( b[0] | ( b[1] << 8 ) | ( b[2] << 16 ) | ( b[3] << 24 ) )
			: ( uint ) ( ( b[0] << 24 ) | ( b[1] << 16 ) | ( b[2] << 8 ) | b[3] );
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ReadScalarValue(ushort type, uint rawValue)
	{
		return type switch
		{
			3 => rawValue & 0xFFFF,
			4 => rawValue,
			_ => 0
		};
	}

	private sealed record PreviewCandidate
	{
		public uint Offset { get; set; }
		public uint Length { get; set; }
		public uint Width { get; set; }
		public uint Height { get; set; }
	}
}
