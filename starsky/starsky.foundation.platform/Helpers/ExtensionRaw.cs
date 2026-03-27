using System;
using System.Text;

namespace starsky.foundation.platform.Helpers;

public static class ExtensionRaw
{
	private const int ProbeLength = 400;
	private const ushort TagMake = 0x010F;
	private const ushort TagModel = 0x0110;
	private const ushort TagDngVersion = 50706;

	private static readonly (string[] keys, ExtensionRolesHelper.ImageFormat format)[]
		MakeMappings =
		[
			( ["PANASONIC"], ExtensionRolesHelper.ImageFormat.rw2 ),
			( ["SONY"], ExtensionRolesHelper.ImageFormat.arw ),
			( ["NIKON"], ExtensionRolesHelper.ImageFormat.nef ),
			( ["FUJIFILM", "FUJI"], ExtensionRolesHelper.ImageFormat.raf ),
			( ["OLYMPUS", "OLYMP"], ExtensionRolesHelper.ImageFormat.orf ),
			( ["PENTAX"], ExtensionRolesHelper.ImageFormat.pef ),
			( ["LEICA", "SAMSUNG", "APPLE"], ExtensionRolesHelper.ImageFormat.dng )
		];

	private static readonly (string marker, ExtensionRolesHelper.ImageFormat format)[]
		MarkerMappings =
		[
			( "PANASONIC", ExtensionRolesHelper.ImageFormat.rw2 ),
			( "SONY", ExtensionRolesHelper.ImageFormat.arw ),
			( "NIKON", ExtensionRolesHelper.ImageFormat.nef ),
			( "FUJIFILM", ExtensionRolesHelper.ImageFormat.raf ),
			( "FUJI", ExtensionRolesHelper.ImageFormat.raf ),
			( "OLYMP", ExtensionRolesHelper.ImageFormat.orf ),
			( "PENTAX", ExtensionRolesHelper.ImageFormat.pef ),
			( "CR3", ExtensionRolesHelper.ImageFormat.cr3 ),
			( "CR2", ExtensionRolesHelper.ImageFormat.cr2 ),
			( "X3F", ExtensionRolesHelper.ImageFormat.x3f ),
			( "DNG", ExtensionRolesHelper.ImageFormat.dng )
		];

	public static ExtensionRolesHelper.ImageFormat Detect(byte[]? bytes)
	{
		if ( bytes == null || bytes.Length < 4 )
		{
			return ExtensionRolesHelper.ImageFormat.unknown;
		}

		if ( HasX3FHeader(bytes) )
		{
			return ExtensionRolesHelper.ImageFormat.x3f;
		}

		if ( HasRw2Header(bytes) )
		{
			return ExtensionRolesHelper.ImageFormat.rw2;
		}

		if ( HasCr2Header(bytes) )
		{
			return ExtensionRolesHelper.ImageFormat.cr2;
		}

		var probe = bytes.AsSpan(0, Math.Min(bytes.Length, ProbeLength));

		// RAF can start with "FUJI" instead of TIFF header.
		if ( HasMarker(probe, "FUJI") )
		{
			return ExtensionRolesHelper.ImageFormat.raf;
		}

		if ( !TryGetTiffEndian(probe, out var littleEndian) || probe.Length < 8 )
		{
			return ExtensionRolesHelper.ImageFormat.unknown;
		}

		var firstIfdOffset = ( int ) ReadUInt32(probe, 4, littleEndian);
		
		if ( firstIfdOffset < 8 || firstIfdOffset + 2 > probe.Length ||
		     !TryParseFirstIfdForTags(probe, firstIfdOffset, littleEndian,
			    out var make, out var hasDngTag) )
		{
			return DetectByMarker(probe);
		}

		if ( hasDngTag )
		{
			return ExtensionRolesHelper.ImageFormat.dng;
		}

		var byMake = DetectFromMake(make);
		if ( byMake.HasValue )
		{
			return byMake.Value;
		}

		return DetectByMarker(probe) ;
	}

	private static bool HasX3FHeader(ReadOnlySpan<byte> bytes)
	{
		return bytes[0] == ( byte ) 'F' && bytes[1] == ( byte ) 'O' &&
		       bytes[2] == ( byte ) 'V' && bytes[3] == ( byte ) 'b';
	}

	private static bool HasRw2Header(ReadOnlySpan<byte> bytes)
	{
		// Panasonic RW2 starts with "IIU\0" instead of classic TIFF "II*\0".
		return bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x55 && bytes[3] == 0x00;
	}

	private static bool HasCr2Header(ReadOnlySpan<byte> bytes)
	{
		// Canon CR2: TIFF little-endian header + "CR" and version bytes at offset 8.
		if ( bytes.Length < 12 )
		{
			return false;
		}

		var hasLittleEndianTiff =
			bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00;
		if ( !hasLittleEndianTiff )
		{
			return false;
		}

		return bytes[8] == ( byte ) 'C' && bytes[9] == ( byte ) 'R' &&
		       bytes[10] == 0x02 && bytes[11] == 0x00;
	}

	private static ExtensionRolesHelper.ImageFormat DetectByMarker(ReadOnlySpan<byte> probe)
	{
		foreach ( var (marker, format) in MarkerMappings )
		{
			if ( HasMarker(probe, marker) )
			{
				return format;
			}
		}

		return ExtensionRolesHelper.ImageFormat.unknown;
	}

	private static bool TryGetTiffEndian(ReadOnlySpan<byte> bytes, out bool littleEndian)
	{
		littleEndian = bytes[0] == 0x49 && bytes[1] == 0x49;
		if ( littleEndian )
		{
			return bytes[2] == 0x2A && bytes[3] == 0x00;
		}

		var bigEndian = bytes[0] == 0x4D && bytes[1] == 0x4D;
		if ( !bigEndian )
		{
			return false;
		}

		return bytes[2] == 0x00 && bytes[3] == 0x2A;
	}

	private static bool TryParseFirstIfdForTags(ReadOnlySpan<byte> bytes, int ifdOffset,
		bool littleEndian, out string? make, out bool hasDngTag)
	{
		make = null;
		string? model = null;
		hasDngTag = false;

		var entryCount = ReadUInt16(bytes, ifdOffset, littleEndian);
		var entriesOffset = ifdOffset + 2;
		for ( var i = 0; i < entryCount; i++ )
		{
			var entryPos = entriesOffset + i * 12;
			if ( entryPos + 12 > bytes.Length )
			{
				break;
			}

			ParseIfdEntry(bytes, entryPos, littleEndian, ref make, ref model, ref hasDngTag);
		}

		return hasDngTag || !string.IsNullOrEmpty(make) || !string.IsNullOrEmpty(model);
	}

	private static void ParseIfdEntry(ReadOnlySpan<byte> bytes, int entryPos, bool littleEndian,
		ref string? make, ref string? model, ref bool hasDngTag)
	{
		var tag = ReadUInt16(bytes, entryPos, littleEndian);
		if ( tag == TagDngVersion )
		{
			hasDngTag = true;
			return;
		}

		if ( tag != TagMake && tag != TagModel )
		{
			return;
		}

		var value = TryReadAsciiTagValue(bytes, entryPos, littleEndian);
		if ( string.IsNullOrEmpty(value) )
		{
			return;
		}

		if ( tag == TagMake )
		{
			make = value;
		}
		else
		{
			model = value;
		}
	}

	private static string? TryReadAsciiTagValue(ReadOnlySpan<byte> bytes, int entryPos,
		bool littleEndian)
	{
		var type = ReadUInt16(bytes, entryPos + 2, littleEndian);
		if ( type != 2 )
		{
			return null;
		}

		var count = ( int ) ReadUInt32(bytes, entryPos + 4, littleEndian);
		if ( count <= 0 )
		{
			return null;
		}

		var dataOffset = ( int ) ReadUInt32(bytes, entryPos + 8, littleEndian);
		return ExtractAsciiUpper(bytes, entryPos + 8, dataOffset, count);
	}

	private static string? ExtractAsciiUpper(ReadOnlySpan<byte> bytes, int inlineOffset,
		int dataOffset, int count)
	{
		var valueOffset = count <= 4 ? inlineOffset : dataOffset;
		if ( valueOffset < 0 || valueOffset + count > bytes.Length )
		{
			return null;
		}

		var realLength = count;
		while ( realLength > 0 && bytes[valueOffset + realLength - 1] == 0 )
		{
			realLength--;
		}

		if ( realLength <= 0 )
		{
			return null;
		}

		return Encoding.ASCII.GetString(bytes.Slice(valueOffset, realLength)).ToUpperInvariant();
	}

	private static ExtensionRolesHelper.ImageFormat? DetectFromMake(string? make)
	{
		if ( string.IsNullOrEmpty(make) )
		{
			return null;
		}

		foreach ( var mapping in MakeMappings )
		{
			foreach ( var key in mapping.keys )
			{
				if ( make.Contains(key, StringComparison.Ordinal) )
				{
					return mapping.format;
				}
			}
		}

		return null;
	}

	private static bool HasMarker(ReadOnlySpan<byte> bytes, string marker)
	{
		if ( bytes.Length < marker.Length )
		{
			return false;
		}

		for ( var i = 0; i <= bytes.Length - marker.Length; i++ )
		{
			var match = true;
			for ( var j = 0; j < marker.Length; j++ )
			{
				var ch = bytes[i + j];
				if ( ch >= ( byte ) 'a' && ch <= ( byte ) 'z' )
				{
					ch = ( byte ) ( ch - 32 );
				}

				if ( ch != marker[j] )
				{
					match = false;
					break;
				}
			}

			if ( match )
			{
				return true;
			}
		}

		return false;
	}

	private static uint ReadUInt32(ReadOnlySpan<byte> bytes, int offset, bool littleEndian)
	{
		if ( littleEndian )
		{
			return ( uint ) ( bytes[offset] | ( bytes[offset + 1] << 8 ) |
			                  ( bytes[offset + 2] << 16 ) | ( bytes[offset + 3] << 24 ) );
		}

		return ( uint ) ( ( bytes[offset] << 24 ) | ( bytes[offset + 1] << 16 ) |
		                  ( bytes[offset + 2] << 8 ) | bytes[offset + 3] );
	}

	private static ushort ReadUInt16(ReadOnlySpan<byte> bytes, int offset, bool littleEndian)
	{
		if ( littleEndian )
		{
			return ( ushort ) ( bytes[offset] | ( bytes[offset + 1] << 8 ) );
		}

		return ( ushort ) ( ( bytes[offset] << 8 ) | bytes[offset + 1] );
	}
}
