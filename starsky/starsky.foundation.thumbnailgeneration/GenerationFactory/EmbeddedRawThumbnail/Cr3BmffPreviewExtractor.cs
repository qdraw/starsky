using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

internal class Cr3BmffPreviewExtractor(IWebLogger logger)
{
	public Task<bool> TryExtract(string rawFilePath, string? outputLargePath,
		string? outputMediumPath)
	{
		var ranges = GetMdatRanges(rawFilePath);
		if ( ranges.Count == 0 )
		{
			// fallback to entire file scan when mdat is absent or malformed
			var fileLength = new FileInfo(rawFilePath).Length;
			ranges.Add((0L, fileLength));
		}

		var scanner = new JpegSegmentScanner(logger);
		return Task.FromResult(scanner.TryExtract(rawFilePath, ranges, outputLargePath,
			outputMediumPath));
	}

	private static List<(long Offset, long Length)> GetMdatRanges(string filePath)
	{
		var result = new List<(long Offset, long Length)>();
		using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
			FileShare.Read);
		var fileLength = stream.Length;
		Span<byte> header = stackalloc byte[8];
		Span<byte> extSize = stackalloc byte[8];

		while ( stream.Position + 8 <= fileLength )
		{
			var boxStart = stream.Position;
			if ( stream.Read(header) != 8 )
			{
				break;
			}

			var size32 = BinaryPrimitives.ReadUInt32BigEndian(header[..4]);
			var type = BinaryPrimitives.ReadUInt32BigEndian(header[4..8]);
			long boxSize;
			var headerSize = 8L;

			if ( size32 == 1 )
			{
				if ( stream.Read(extSize) != 8 )
				{
					break;
				}

				boxSize = (long)BinaryPrimitives.ReadUInt64BigEndian(extSize);
				headerSize = 16;
			}
			else if ( size32 == 0 )
			{
				boxSize = fileLength - boxStart;
			}
			else
			{
				boxSize = size32;
			}

			if ( boxSize < headerSize )
			{
				break;
			}

			var payloadOffset = boxStart + headerSize;
			var payloadLength = boxSize - headerSize;
			if ( payloadOffset + payloadLength > fileLength )
			{
				break;
			}

			if ( type == ToType("mdat") && payloadLength > 0 )
			{
				result.Add((payloadOffset, payloadLength));
			}

			stream.Seek(boxStart + boxSize, SeekOrigin.Begin);
		}

		return result;
	}

	private static uint ToType(string value)
	{
		return (uint)(value[0] << 24 | value[1] << 16 | value[2] << 8 | value[3]);
	}
}
