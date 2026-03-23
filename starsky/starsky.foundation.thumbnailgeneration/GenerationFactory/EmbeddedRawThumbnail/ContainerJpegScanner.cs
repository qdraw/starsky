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
						TryAddJpegCandidate(input, soi, candidates);
					}

					previous = current;
				}

				previous = buffer[read - 1];
				scanned += read;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}

		return candidates;
	}

	private static void TryAddJpegCandidate(Stream input, uint soi, List<PreviewCandidate> candidates)
	{
		var maxProbe = ( int ) Math.Min(MaxJpegProbe, input.Length - soi);
		var length = DetectJpegLengthByEoi(input, soi, maxProbe);
		if ( length < MinJpegSize )
		{
			return;
		}

		var hasIptc = HasIptcApp13(input, soi, length);
		candidates.Add(new PreviewCandidate(soi, length, hasIptc));
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
			if ( best == null || candidate.HasIptc && !best.HasIptc )
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
			if ( !TrySkipToMarker(input, end, out var marker) )
			{
				break;
			}

			if ( IsJpegStreamEndMarker(marker) )
			{
				return false;
			}

			if ( IsStandaloneMarker(marker) )
			{
				continue;
			}

			if ( !TryReadSegmentPayloadLength(input, out var payloadLength) )
			{
				return false;
			}

			if ( input.Position + payloadLength > end )
			{
				return false;
			}

			if ( AdvanceSegmentAndCheckIptc(input, marker, payloadLength) )
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Scans forward until a valid JPEG marker (0xFF + non-padding byte) is found within
	/// <paramref name="end"/>, handling 0xFF padding bytes.
	/// </summary>
	private static bool TrySkipToMarker(Stream input, long end, out int marker)
	{
		marker = -1;
		while ( input.Position < end )
		{
			var b = input.ReadByte();
			if ( b < 0 )
			{
				return false;
			}

			if ( b != 0xFF )
			{
				continue;
			}

			do
			{
				marker = input.ReadByte();
				if ( marker < 0 )
				{
					return false;
				}
			} while ( marker == 0xFF );

			return true;
		}

		return false;
	}

	/// <summary>Returns true for EOI (0xD9) and SOS (0xDA) markers that signal the end of scannable data.</summary>
	private static bool IsJpegStreamEndMarker(int marker) => marker is 0xD9 or 0xDA;

	/// <summary>Returns true for JPEG markers that have no length/payload (RST0–RST7, TEM).</summary>
	private static bool IsStandaloneMarker(int marker) =>
		marker is >= 0xD0 and <= 0xD7 or 0x01;

	/// <summary>
	/// Reads the 2-byte segment length field and returns the payload length (length − 2).
	/// </summary>
	private static bool TryReadSegmentPayloadLength(Stream input, out int payloadLength)
	{
		payloadLength = 0;
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

		payloadLength = segmentLength - 2;
		return true;
	}

	/// <summary>
	/// Seeks past <paramref name="payloadLength"/> bytes for non-APP13 segments, or
	/// delegates to <see cref="IsIptcApp13Payload"/> for APP13 (0xED).
	/// </summary>
	private static bool AdvanceSegmentAndCheckIptc(Stream input, int marker, int payloadLength)
	{
		if ( marker != 0xED )
		{
			input.Seek(payloadLength, SeekOrigin.Current);
			return false;
		}

		return IsIptcApp13Payload(input, payloadLength);
	}

	/// <summary>
	/// Probes the first 64 bytes of an APP13 payload for the IPTC/Photoshop signature,
	/// then advances the stream to the end of the segment.
	/// </summary>
	private static bool IsIptcApp13Payload(Stream input, int payloadLength)
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

