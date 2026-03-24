using System;
using System.Buffers;
using System.IO;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     High-performance utilities for scanning JPEG structure and detecting EOI markers.
///     Handles both cases: raw JPEG data and JPEG data with SOI marker at specified offset.
/// </summary>
internal static class JpegScannerUtilities
{
	private const int JpegScanBufferSize = 64 * 1024;

	/// <summary>
	///     Detects the length of a JPEG by scanning for the EOI (End Of Image) marker (0xFF 0xD9).
	///     Call this when you have a pointer to the SOI (Start Of Image) marker itself.
	/// </summary>
	/// <param name="input">The stream to scan</param>
	/// <param name="soiOffset">Offset pointing to the SOI marker (FF D8)</param>
	/// <param name="maxScanBytes">Maximum bytes to scan from SOI + 2</param>
	/// <returns>Total JPEG length from SOI, or 0 if not found</returns>
	internal static uint DetectJpegLengthFromSoi(Stream input, uint soiOffset, int maxScanBytes)
	{
		// From SOI position, skip the 2-byte SOI marker before scanning for EOI
		if ( maxScanBytes < 4 || !StreamPrimitives.TrySeek(input, soiOffset + 2) )
		{
			return 0;
		}

		return ScanForEoiMarker(input, maxScanBytes, 2);
	}

	/// <summary>
	///     Detects the length of a JPEG by scanning for the EOI (End Of Image) marker (0xFF 0xD9).
	///     Call this when you have a pointer to the start of the JPEG data (including SOI).
	/// </summary>
	/// <param name="input">The stream to scan</param>
	/// <param name="startOffset">Offset pointing to the start of JPEG data (including SOI marker)</param>
	/// <param name="maxScanBytes">Maximum bytes to scan from startOffset</param>
	/// <returns>Total JPEG length from startOffset, or 0 if not found</returns>
	internal static uint DetectJpegLengthFromStart(Stream input, uint startOffset, int maxScanBytes)
	{
		if ( maxScanBytes < 2 || !StreamPrimitives.TrySeek(input, startOffset) )
		{
			return 0;
		}

		return ScanForEoiMarker(input, maxScanBytes, 0);
	}

	/// <summary>
	///     Internal helper to scan for the EOI marker (0xFF 0xD9).
	/// </summary>
	/// <param name="input">The stream positioned at the start of scan</param>
	/// <param name="maxScanBytes">Maximum bytes to scan</param>
	/// <param name="startOffset">Bytes already scanned (to adjust position in return value)</param>
	/// <returns>Length from original offset, or 0 if EOI not found</returns>
	private static uint ScanForEoiMarker(Stream input, int maxScanBytes, int startOffset)
	{
		var buffer = ArrayPool<byte>.Shared.Rent(JpegScanBufferSize);
		try
		{
			var scanned = startOffset;
			var previous = -1;

			while ( scanned < maxScanBytes )
			{
				var toRead = Math.Min(buffer.Length, maxScanBytes - scanned);
				var read = input.Read(buffer, 0, toRead);
				if ( read <= 0 )
				{
					break;
				}

				for ( var i = 0; i < read; i++ )
				{
					var current = buffer[i];
					if ( previous == 0xFF && current == 0xD9 )
					{
						return ( uint ) ( scanned + i + 1 );
					}

					previous = current;
				}

				scanned += read;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}

		return 0;
	}
}
