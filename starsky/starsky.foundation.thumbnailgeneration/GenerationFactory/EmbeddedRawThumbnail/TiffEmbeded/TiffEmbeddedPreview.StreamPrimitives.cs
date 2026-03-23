using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

public partial class TiffEmbeddedPreviewExtractor
{
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
	private static uint ReadScalarValue(ushort type, uint rawValue, bool littleEndian)
	{
		return type switch
		{
			3 => littleEndian ? rawValue & 0xFFFF : ( rawValue >> 16 ) & 0xFFFF,
			4 => rawValue,
			_ => 0
		};
	}
}
