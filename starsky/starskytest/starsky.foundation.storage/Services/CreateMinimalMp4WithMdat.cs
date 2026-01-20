using System;
using System.IO;

namespace starskytest.starsky.foundation.storage.Services;

public static class CreateMinimalMp4WithMdatHelper
{
	/// <summary>
	/// Creates a minimal valid MP4 file with ftyp and mdat atoms
	/// </summary>
	internal static byte[] CreateMinimalMp4WithMdat(byte[] mdatContent)
	{
		using var ms = new MemoryStream();

		// Write ftyp atom (file type box)
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		ms.Write(ftypSize, 0, 4);
		ms.Write("ftyp"u8.ToArray(), 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4); // major brand
		ms.Write(new byte[4], 0, 4); // minor version
		ms.Write("isom"u8.ToArray(), 0, 4); // compatible brand

		// Write mdat atom (media data)
		var mdatSize = ( uint ) ( 8 + mdatContent.Length );
		var mdatSizeBytes = BitConverter.GetBytes(mdatSize);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSizeBytes);
		}

		ms.Write(mdatSizeBytes, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);
		ms.Write(mdatContent, 0, mdatContent.Length);

		return ms.ToArray();
	}

	/// <summary>
	/// Creates an MP4 with extended size (size field = 1)
	/// </summary>
	internal static byte[] CreateMp4WithExtendedSize(byte[] mdatContent)
	{
		using var ms = new MemoryStream();

		// Write ftyp atom
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		ms.Write(ftypSize, 0, 4);
		ms.Write("ftyp"u8.ToArray(), 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);
		ms.Write(new byte[4], 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);

		// Write mdat atom with extended size
		var extendedSizeMarker = BitConverter.GetBytes(( uint ) 1);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(extendedSizeMarker);
		}

		ms.Write(extendedSizeMarker, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);

		// Write actual size as 64-bit value
		var actualSize = ( ulong ) ( 16 + mdatContent.Length ); // 4 + 4 + 8 + content
		var actualSizeBytes = BitConverter.GetBytes(actualSize);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(actualSizeBytes);
		}

		ms.Write(actualSizeBytes, 0, 8);

		ms.Write(mdatContent, 0, mdatContent.Length);

		return ms.ToArray();
	}
}
