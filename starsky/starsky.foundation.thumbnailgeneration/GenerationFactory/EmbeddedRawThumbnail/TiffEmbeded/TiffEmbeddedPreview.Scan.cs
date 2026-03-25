using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

public partial class TiffEmbeddedPreviewExtractor
{
	/// <summary>
	///     Returns true when the JPEG at <paramref name="offset" /> is lossless.
	///     Canon CR2 stores lossless raw strips starting with FF D8 FF C4 (SOI + DHT, no DQT).
	///     ImageSharp cannot decode these; they must be excluded from preview candidates.
	/// </summary>
	internal static bool IsLosslessJpegAtOffset(Stream input, uint offset)
	{
		if ( !StreamPrimitives.TrySeek(input, offset) )
		{
			return false;
		}

		Span<byte> header = stackalloc byte[4];
		if ( input.Read(header) < 4 )
		{
			return false;
		}

		// FF D8 = SOI; FF C4 = DHT without prior DQT -> lossless JPEG
		// FF D8 = SOI; FF C3 = SOF3 = lossless sequential
		if ( header[0] != 0xFF || header[1] != 0xD8 || header[2] != 0xFF )
		{
			return false;
		}

		return header[3] == 0xC4 || header[3] == 0xC3;
	}

	private static uint DetectJpegLengthByEoi(Stream input, uint startOffset, int maxScanBytes)
	{
		return JpegScannerUtilities.DetectJpegLengthFromStart(input, startOffset, maxScanBytes);
	}

	internal static IEnumerable<PreviewCandidate?> ScanJpegsInRange(Stream input, uint rangeOffset,
		uint rangeLength)
	{
		var maxScan = ( int ) Math.Min(rangeLength, MaxMakerNoteScanBytes);
		if ( maxScan < 4 || !StreamPrimitives.TrySeek(input, rangeOffset) )
		{
			yield break;
		}

		var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
		try
		{
			var scanned = 0;
			var b0 = -1;
			var b1 = -1;
			while ( scanned < maxScan )
			{
				var toRead = Math.Min(buffer.Length, maxScan - scanned);
				var read = input.Read(buffer, 0, toRead);
				if ( read <= 0 )
				{
					break;
				}

				for ( var i = 0; i < read; i++ )
				{
					var b2 = buffer[i];
					if ( !IsJpegStartMarker(b0, b1, b2) )
					{
						b0 = b1;
						b1 = b2;
						continue;
					}

					var soi = ( uint ) ( rangeOffset + scanned + i - 2 );
					if ( TryBuildScanCandidate(input, soi,
						    maxScan - ( scanned + i - 2 ),
						    out var candidate) )
					{
						yield return candidate;
					}

					b0 = b1;
					b1 = b2;
				}

				scanned += read;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsJpegStartMarker(int b0, int b1, int b2)
	{
		return b0 == 0xFF && b1 == 0xD8 && b2 == 0xFF;
	}

	internal static bool TryBuildScanCandidate(Stream input, uint soi, int remaining,
		out PreviewCandidate? candidate)
	{
		candidate = null;
		var resumePosition = input.Position;

		if ( IsLosslessJpegAtOffset(input, soi) )
		{
			input.Seek(resumePosition, SeekOrigin.Begin);
			return false;
		}

		var length = DetectJpegLengthByEoi(input, soi, remaining);
		input.Seek(resumePosition, SeekOrigin.Begin);
		if ( length < MinJpegSize )
		{
			return false;
		}

		candidate = new PreviewCandidate { Offset = soi, Length = length };
		return true;
	}
}
