using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

internal sealed class JpegSegmentScanner(IWebLogger logger)
{
	private const int MinJpegBytes = 4 * 1024;

	internal readonly record struct Segment(long Offset, long Length, bool IsDecodableByImageSharp);

	public bool TryExtract(string inputPath, IReadOnlyList<(long Offset, long Length)> ranges,
		string? outputLargePath, string? outputMediumPath)
	{
		using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		var segments = FindSegments(input, ranges);
		if ( segments.Count == 0 )
		{
			return false;
		}

		var hasNonTiny = segments.Any(p => p.Length >= MinJpegBytes);
		var ordered = segments
			.OrderByDescending(p => p.IsDecodableByImageSharp)
			.ThenByDescending(p => p.Length)
			.ToList();
		var primary = hasNonTiny ? ordered.Where(p => p.Length >= MinJpegBytes).ToList() : ordered;

		var ok = true;
		Segment? selectedLarge = null;
		if ( outputLargePath is not null )
		{
			ok &= TryWriteFirstValid(input, primary, outputLargePath, out selectedLarge)
			      || (!hasNonTiny && TryWriteFirstValid(input, ordered, outputLargePath,
				      out selectedLarge));
		}

		if ( outputMediumPath is not null )
		{
			var mediumCandidates = primary.Where(p => selectedLarge == null ||
			                                         p.Offset != selectedLarge.Value.Offset)
				.ToList();
			var mediumWritten = TryWriteFirstValid(input, mediumCandidates, outputMediumPath, out _)
			                  || (!hasNonTiny && TryWriteFirstValid(input,
				                  ordered.Where(p => selectedLarge == null ||
				                                       p.Offset != selectedLarge.Value.Offset)
					                  .ToList(), outputMediumPath, out _));

			if ( !mediumWritten && selectedLarge.HasValue && outputLargePath is not null &&
			     File.Exists(outputLargePath) )
			{
				File.Copy(outputLargePath, outputMediumPath, true);
				mediumWritten = true;
			}

			ok &= mediumWritten;
		}

		return ok;
	}

	private static List<Segment> FindSegments(Stream input,
		IReadOnlyList<(long Offset, long Length)> ranges)
	{
		var result = new List<Segment>();

		foreach ( var (rangeOffset, rangeLength) in ranges )
		{
			if ( rangeOffset < 0 || rangeLength <= 0 || rangeOffset + rangeLength > input.Length )
			{
				continue;
			}

			ScanRange(input, rangeOffset, rangeLength, result);
		}

		return result;
	}

	private static void ScanRange(Stream input, long rangeOffset, long rangeLength,
		List<Segment> result)
	{
		input.Seek(rangeOffset, SeekOrigin.Begin);
		var end = rangeOffset + rangeLength;
		var previous = -1;

		while ( input.Position < end )
		{
			var current = input.ReadByte();
			if ( current < 0 )
			{
				break;
			}

			if ( previous == 0xFF && current == 0xD8 )
			{
				var start = input.Position - 2;
				if ( TryGetJpegLength(input, start, end, out var length,
					     out var isDecodableByImageSharp) )
				{
					result.Add(new Segment(start, length, isDecodableByImageSharp));
					input.Seek(start + length, SeekOrigin.Begin);
					previous = -1;
					continue;
				}
			}

			previous = current;
		}
	}

	private static bool TryGetJpegLength(Stream input, long start, long rangeEnd,
		out long length, out bool isDecodableByImageSharp)
	{
		length = 0;
		isDecodableByImageSharp = true;
		input.Seek(start, SeekOrigin.Begin);

		if ( ReadByte(input, rangeEnd) != 0xFF || ReadByte(input, rangeEnd) != 0xD8 )
		{
			return false;
		}

		var hasSos = false;
		var hasSof = false;
		while ( input.Position < rangeEnd )
		{
			if ( !TryReadMarker(input, rangeEnd, out var marker) )
			{
				return false;
			}

			if ( marker == 0xD9 )
			{
				if ( !hasSos || !hasSof )
				{
					return false;
				}

				length = input.Position - start;
				return true;
			}

			if ( marker is >= 0xD0 and <= 0xD7 || marker == 0x01 )
			{
				continue;
			}

			var segmentLength = ReadUInt16BigEndian(input, rangeEnd);
			if ( segmentLength < 2 )
			{
				return false;
			}

			var payloadLength = segmentLength - 2;
			if ( input.Position + payloadLength > rangeEnd )
			{
				return false;
			}

			if ( IsSofMarker(marker) )
			{
				hasSof = true;
				if ( IsLosslessSofMarker(marker) )
				{
					isDecodableByImageSharp = false;
				}

				var precision = ReadByte(input, rangeEnd);
				if ( precision is not 8 and not 12 )
				{
					isDecodableByImageSharp = false;
				}

				payloadLength -= 1;
			}

			if ( marker == 0xDA )
			{
				if ( payloadLength > 0 )
				{
					input.Seek(payloadLength, SeekOrigin.Current);
				}

				if ( !TryFindEntropyEnd(input, rangeEnd, start, out length) )
				{
					return false;
				}

				return hasSof;
			}

			if ( payloadLength > 0 )
			{
				input.Seek(payloadLength, SeekOrigin.Current);
			}
		}

		return false;
	}

	private static bool TryFindEntropyEnd(Stream input, long rangeEnd, long start,
		out long length)
	{
		length = 0;
		var previous = -1;
		while ( input.Position < rangeEnd )
 		{
			var current = ReadByte(input, rangeEnd);
			if ( previous == 0xFF && current == 0xD9 )
 			{
 				length = input.Position - start;
 				return true;
 			}

			previous = current;
 		}

 		return false;
 	}

	private static bool TryReadMarker(Stream input, long rangeEnd, out int marker)
	{
		marker = -1;
		var prefix = ReadByte(input, rangeEnd);
		while ( prefix != 0xFF )
		{
			prefix = ReadByte(input, rangeEnd);
		}

		do
		{
			marker = ReadByte(input, rangeEnd);
		} while ( marker == 0xFF );

		return marker >= 0;
	}

	private static int ReadByte(Stream input, long rangeEnd)
	{
		if ( input.Position >= rangeEnd )
		{
			return -1;
		}

		return input.ReadByte();
	}

	private static int ReadUInt16BigEndian(Stream input, long rangeEnd)
	{
		var high = ReadByte(input, rangeEnd);
		var low = ReadByte(input, rangeEnd);
		if ( high < 0 || low < 0 )
		{
			return -1;
		}

		return (high << 8) | low;
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

	private bool TryWriteFirstValid(Stream input, List<Segment> segments, string outputPath,
		out Segment? selected)
	{
		selected = null;
		foreach ( var segment in segments )
		{
			if ( !WriteSegment(input, segment, outputPath) )
			{
				continue;
			}

			selected = segment;
			return true;
		}

		return false;
	}

	private bool WriteSegment(Stream input, Segment segment, string outputPath)
	{
		input.Seek(segment.Offset, SeekOrigin.Begin);
		using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
		var remaining = segment.Length;
		var buffer = new byte[64 * 1024];
		while ( remaining > 0 )
		{
			var read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
			if ( read <= 0 )
			{
				return false;
			}

			output.Write(buffer, 0, read);
			remaining -= read;
		}

		if ( !HasSosMarker(outputPath) )
		{
			try
			{
				File.Delete(outputPath);
			}
			catch
			{
				// ignore cleanup failures
			}

			logger.LogDebug($"[JpegSegmentScanner] rejected segment at {segment.Offset} without SOS marker");
			return false;
		}

		return true;
	}

	private static bool HasSosMarker(string outputPath)
	{
		using var stream = new FileStream(outputPath, FileMode.Open, FileAccess.Read);
		if ( stream.Length < 4 )
		{
			return false;
		}

		if ( stream.ReadByte() != 0xFF || stream.ReadByte() != 0xD8 )
		{
			return false;
		}

		var previous = -1;
		while ( stream.Position < stream.Length )
		{
			var current = stream.ReadByte();
			if ( current < 0 )
			{
				return false;
			}

			if ( previous == 0xFF && current == 0xDA )
			{
				return true;
			}

			previous = current;
		}

		return false;
	}
}
