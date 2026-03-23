using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

internal static class ContainerJpegScanner
{
	private const int MinJpegSize = 4096;
	private const int MaxCandidates = 8;
	private const int MaxJpegProbe = 128 * 1024 * 1024;

	internal sealed record PreviewCandidate(uint Offset, uint Length, bool HasIptc);

	public static async Task<bool> TryExtractBestPreview(Stream input, Stream? output)
	{
		if ( !input.CanSeek || input.Length < MinJpegSize )
		{
			return false;
		}

		var candidates = ScanCandidates(input);
		var best = SelectBest(candidates);
		if ( best == null )
		{
			return false;
		}

		if ( output == null )
		{
			return true;
		}

		return await CopyRangeToOutput(input, output, best.Offset, best.Length);
	}

	private static List<PreviewCandidate> ScanCandidates(Stream input)
	{
		var candidates = new List<PreviewCandidate>();
		if ( !TrySeek(input, 0) )
		{
			return candidates;
		}

		var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
		try
		{
			long scanned = 0;
			var previous = -1;
			while ( scanned < input.Length && candidates.Count < MaxCandidates )
			{
				var toRead = ( int ) Math.Min(buffer.Length, input.Length - scanned);
				var read = input.Read(buffer, 0, toRead);
				if ( read <= 0 )
				{
					break;
				}

				for ( var i = 0; i < read - 1 && candidates.Count < MaxCandidates; i++ )
				{
					var current = buffer[i];
					var next = buffer[i + 1];
					if ( previous == 0xFF && current == 0xD8 && next == 0xFF )
					{
						var soi = ( uint ) ( scanned + i - 1 );
						var maxProbe = ( int ) Math.Min(MaxJpegProbe, input.Length - soi);
						var length = DetectJpegLengthByEoi(input, soi, maxProbe);
						if ( length >= MinJpegSize )
						{
							var hasIptc = HasIptcApp13(input, soi, length);
							candidates.Add(new PreviewCandidate(soi, length, hasIptc));
						}
					}

					previous = current;
				}

				if ( read > 0 )
				{
					previous = buffer[read - 1];
				}
				scanned += read;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}

		return candidates;
	}

	private static PreviewCandidate? SelectBest(List<PreviewCandidate> candidates)
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

			if ( candidate.HasIptc && !best.HasIptc )
			{
				best = candidate;
				continue;
			}

			if ( candidate.HasIptc == best.HasIptc && candidate.Length > best.Length )
			{
				best = candidate;
			}
		}

		return best;
	}

	private static async Task<bool> CopyRangeToOutput(Stream input, Stream output, uint offset,
		uint length)
	{
		if ( !TrySeek(input, offset) || offset + length > input.Length )
		{
			return false;
		}

		var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
		try
		{
			var remaining = ( long ) length;
			while ( remaining > 0 )
			{
				var toRead = ( int ) Math.Min(buffer.Length, remaining);
				var read = await input.ReadAsync(buffer.AsMemory(0, toRead));
				if ( read <= 0 )
				{
					return false;
				}

				await output.WriteAsync(buffer.AsMemory(0, read));
				remaining -= read;
			}
			return true;
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	private static uint DetectJpegLengthByEoi(Stream input, uint soiOffset, int maxBytes)
	{
		if ( maxBytes < 4 || !TrySeek(input, soiOffset + 2) )
		{
			return 0;
		}

		var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
		try
		{
			var scanned = 2;
			var previous = -1;
			while ( scanned < maxBytes )
			{
				var toRead = Math.Min(buffer.Length, maxBytes - scanned);
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

	private static bool HasIptcApp13(Stream input, uint offset, uint length)
	{
		if ( length < 8 || !TrySeek(input, offset + 2) )
		{
			return false;
		}

		var end = ( long ) offset + length;
		while ( input.Position + 4 <= end )
		{
			var markerPrefix = input.ReadByte();
			if ( markerPrefix != 0xFF )
			{
				continue;
			}

			var marker = input.ReadByte();
			if ( marker < 0 )
			{
				return false;
			}

			while ( marker == 0xFF )
			{
				marker = input.ReadByte();
				if ( marker < 0 )
				{
					return false;
				}
			}

			if ( marker is 0xD9 or 0xDA )
			{
				return false;
			}

			if ( marker is >= 0xD0 and <= 0xD7 or 0x01 )
			{
				continue;
			}

			Span<byte> lenBuffer = stackalloc byte[2];
			if ( input.Read(lenBuffer) < 2 )
			{
				return false;
			}

			var segmentLength = ( lenBuffer[0] << 8 ) | lenBuffer[1];
			if ( segmentLength < 2 )
			{
				return false;
			}

			var payloadLength = segmentLength - 2;
			if ( input.Position + payloadLength > end )
			{
				return false;
			}

			if ( marker == 0xED )
			{
				var probeLength = Math.Min(payloadLength, 64);
				var probe = ArrayPool<byte>.Shared.Rent(probeLength);
				try
				{
					if ( input.Read(probe, 0, probeLength) < probeLength )
					{
						return false;
					}

					var text = System.Text.Encoding.ASCII.GetString(probe, 0, probeLength);
					if ( text.Contains("Photoshop 3.0", StringComparison.Ordinal) ||
					     text.Contains("8BIM", StringComparison.Ordinal) )
					{
						return true;
					}
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(probe);
				}

				if ( payloadLength > probeLength )
				{
					input.Seek(payloadLength - probeLength, SeekOrigin.Current);
				}
			}
			else
			{
				input.Seek(payloadLength, SeekOrigin.Current);
			}
		}

		return false;
	}

	private static bool TrySeek(Stream input, long offset)
	{
		if ( !input.CanSeek )
		{
			return false;
		}

		if ( offset < 0 || offset > input.Length )
		{
			return false;
		}

		input.Seek(offset, SeekOrigin.Begin);
		return true;
	}
}

