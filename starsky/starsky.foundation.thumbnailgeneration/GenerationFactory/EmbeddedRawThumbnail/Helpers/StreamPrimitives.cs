using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbedded;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;

internal static class StreamPrimitives
{
	internal static bool TrySeek(Stream input, long offset)
	{
		if ( !input.CanSeek )
		{
			return false;
		}

		if ( offset < 0 || offset > input.Length )
		{
			return false;
		}

		try
		{
			input.Seek(offset, SeekOrigin.Begin);
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool TryGetRemainingBytes(Stream s, out long remaining)
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

	internal static uint ClampIndirectCount(Stream s, uint offset, ushort type, uint requested,
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

	internal static void ReadIndirectOffsets(Stream s, uint offset, ushort type, uint count,
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
					? TiffEmbeddedPreviewExtractor.ReadUInt16(
						buf.AsSpan(i * 2, 2), littleEndian)
					: TiffEmbeddedPreviewExtractor.ReadUInt32(
						buf.AsSpan(i * 4, 4), littleEndian);
				offsets.Add(val);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buf);
		}
	}
}
