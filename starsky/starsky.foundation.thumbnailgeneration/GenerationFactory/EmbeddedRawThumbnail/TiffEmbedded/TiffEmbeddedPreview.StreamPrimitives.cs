using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	TiffEmbedded;

public partial class TiffEmbeddedPreviewExtractor
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ushort ReadUInt16(ReadOnlySpan<byte> b, bool littleEndian)
	{
		return littleEndian
			? ( ushort ) ( b[0] | ( b[1] << 8 ) )
			: ( ushort ) ( ( b[0] << 8 ) | b[1] );
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ReadUInt32(ReadOnlySpan<byte> b, bool littleEndian)
	{
		return littleEndian
			? ( uint ) ( b[0] | ( b[1] << 8 ) | ( b[2] << 16 ) | ( b[3] << 24 ) )
			: ( uint ) ( ( b[0] << 24 ) | ( b[1] << 16 ) | ( b[2] << 8 ) | b[3] );
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ReadScalarValue(ushort type, uint rawValue, bool littleEndian)
	{
		return type switch
		{
			3 => littleEndian ? rawValue & 0xFFFF : ( rawValue >> 16 ) & 0xFFFF,
			4 => rawValue,
			_ => 0
		};
	}
}
