using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	TiffEmbedded;

public partial class TiffEmbeddedPreviewExtractor
{
	/// <summary>
	///     Returns true when the JPEG at <paramref name="offset" /> is lossless.
	///     Canon CR2 raw strips often start with FF D8 FF C4 because the first marker is DHT,
	///     but valid preview JPEGs may do the same. Distinguish them by scanning until the first
	///     SOF marker and classifying only lossless SOF variants as raw strips.
	/// </summary>
	internal static bool IsLosslessJpegAtOffset(Stream input, uint offset)
	{
		if ( !StreamPrimitives.TrySeek(input, offset) )
		{
			return false;
		}

		var buffer = new byte[1024];
		var read = input.Read(buffer, 0, buffer.Length);
		if ( read < 4 )
		{
			return false;
		}

		if ( buffer[0] != 0xFF || buffer[1] != 0xD8 || buffer[2] != 0xFF )
		{
			return false;
		}

		var pos = 2;
		while ( TryFindNextMarker(buffer, read, ref pos, out var marker, out var segLen) )
		{
			if ( IsSofMarker(marker) )
			{
				return IsLosslessSofMarker(marker);
			}

			if ( marker == 0xDA )
			{
				return false;
			}

			pos += 2 + Math.Max(0, segLen);
		}

		return false;
	}

	private static bool IsLosslessSofMarker(int marker)
	{
		return marker is 0xC3 or 0xC7 or 0xCB or 0xCF;
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

		var length = JpegScannerUtilities.DetectJpegLengthFromStart(input, soi, remaining);
		input.Seek(resumePosition, SeekOrigin.Begin);
		if ( length < MinJpegSize )
		{
			return false;
		}

		candidate = new PreviewCandidate { Offset = soi, Length = length };
		return true;
	}
}
