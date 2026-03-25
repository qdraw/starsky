using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Extracts embedded JPEG previews from lightweight RAW containers (FFF/X3F).
///     Selection prefers JPEG candidates with IPTC APP13 metadata.
/// </summary>
public class LightweightContainerPreviewExtractor(
	IWebLogger logger,
	ISelectorStorage selectorStorage)
{
	private const int MinJpegSize = 4096;
	private const int MaxTiffHeaderScanBytes = 64 * 1024;

	private IStorage SubPathStorage => selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
	private IStorage TempStorage => selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

	public async Task<bool> TryExtract(string subPathRawFile, string? outputLargePath)
	{
		if ( !SubPathStorage.ExistFile(subPathRawFile) )
		{
			return false;
		}

		try
		{
			await using var input = SubPathStorage.ReadStream(subPathRawFile);
			await using var output = new MemoryStream();

			var extension = Path.GetExtension(subPathRawFile).ToLowerInvariant();
			var ok = extension == ".x3f" &&
			         await TryExtractX3FTaggedPreview(input, output);

			if ( !ok )
			{
				ok = await ContainerJpegScanner.TryExtractBestPreview(input, output);
			}

			if ( !ok || outputLargePath == null || output.Length == 0 )
			{
				return ok;
			}

			output.Seek(0, SeekOrigin.Begin);
			return await TempStorage.WriteStreamAsync(output, outputLargePath);
		}
		catch ( Exception exception )
		{
			logger.LogDebug(
				$"[LightweightContainerPreviewExtractor] Failed to extract from {subPathRawFile}: {exception.Message}");
			return false;
		}
	}

	internal static async Task<bool> TryExtractX3FTaggedPreview(Stream input, Stream output)
	{
		if ( !TryParseTiffHeader(input, out var tiffBase, out var littleEndian,
			    out var firstIfdRelative) )
		{
			return false;
		}

		var ifdOffset = tiffBase + firstIfdRelative;
		for ( var i = 0; i < 4 && ifdOffset > 0; i++ )
		{
			if ( !TryReadIfdJpegPair(input, ifdOffset, littleEndian,
				    out var candidateOffset, out var candidateLength, out var compression,
				    out var nextIfdRelative) )
			{
				break;
			}

			if ( compression is 6 or 7 && TryResolveAndValidateOffset(input, tiffBase,
				    candidateOffset, candidateLength, out var resolvedOffset) )
			{
				return await CopyRangeAsync(input, output, resolvedOffset, candidateLength);
			}

			if ( nextIfdRelative == 0 )
			{
				break;
			}

			ifdOffset = tiffBase + nextIfdRelative;
		}

		return false;
	}

	internal static bool TryParseTiffHeader(Stream input, out int tiffBase, out bool littleEndian,
		out uint firstIfdRelative)
	{
		tiffBase = -1;
		littleEndian = false;
		firstIfdRelative = 0;

		if ( !input.CanSeek || input.Length < 512 )
		{
			return false;
		}

		var found = FindTiffHeaderOffset(input);
		if ( found < 0 || !StreamPrimitives.TrySeek(input, found) )
		{
			return false;
		}

		var endianBuf = new byte[2];
		if ( input.Read(endianBuf, 0, 2) != 2 )
		{
			return false;
		}

		littleEndian = endianBuf[0] == 0x49 && endianBuf[1] == 0x49;
		if ( !littleEndian && !( endianBuf[0] == 0x4D && endianBuf[1] == 0x4D ) )
		{
			return false;
		}

		if ( !TryReadUInt16(input, littleEndian, out var magic) || magic != 42 )
		{
			return false;
		}

		if ( !TryReadUInt32(input, littleEndian, out firstIfdRelative) )
		{
			return false;
		}

		tiffBase = found;
		return true;
	}

	internal static int FindTiffHeaderOffset(Stream input)
	{
		if ( !StreamPrimitives.TrySeek(input, 0) )
		{
			return -1;
		}

		var scanLength = ( int ) Math.Min(MaxTiffHeaderScanBytes, input.Length);
		var bytes = new byte[scanLength];
		if ( input.Read(bytes, 0, scanLength) < 8 )
		{
			return -1;
		}

		for ( var i = 0; i <= scanLength - 4; i++ )
		{
			var isLittle = bytes[i] == 0x49 && bytes[i + 1] == 0x49 && bytes[i + 2] == 0x2A &&
			               bytes[i + 3] == 0x00;
			var isBig = bytes[i] == 0x4D && bytes[i + 1] == 0x4D && bytes[i + 2] == 0x00 &&
			            bytes[i + 3] == 0x2A;
			if ( isLittle || isBig )
			{
				return i;
			}
		}

		return -1;
	}

	internal static bool TryReadIfdJpegPair(Stream input, long ifdOffset, bool littleEndian,
		out uint jpegOffset, out uint jpegLength, out ushort compression, out uint nextIfdRelative)
	{
		jpegOffset = 0;
		jpegLength = 0;
		compression = 0;
		nextIfdRelative = 0;

		// Read entry count
		if ( !StreamPrimitives.TrySeek(input, ifdOffset) ||
		     !TryReadUInt16(input, littleEndian, out var entryCount) )
		{
			return false;
		}

		if ( entryCount > 1024 )
		{
			return false;
		}

		for ( var i = 0; i < entryCount; i++ )
		{
			if ( !TryReadIfdEntry(input, littleEndian, out var tag, out var type, out var count,
				    out var value) )
			{
				return false;
			}

			if ( count != 1 )
			{
				continue;
			}

			HandleIfdEntry(tag, type, value, littleEndian, ref compression, ref jpegOffset,
				ref jpegLength);
		}

		return TryReadUInt32(input, littleEndian, out nextIfdRelative);
	}

	internal static bool TryReadIfdEntry(Stream input, bool littleEndian, out ushort tag,
		out ushort type,
		out uint count, out uint value)
	{
		tag = 0;
		type = 0;
		count = 0;
		value = 0;
		if ( !TryReadUInt16(input, littleEndian, out tag) ||
		     !TryReadUInt16(input, littleEndian, out type) ||
		     !TryReadUInt32(input, littleEndian, out count) ||
		     !TryReadUInt32(input, littleEndian, out value) )
		{
			return false;
		}

		return true;
	}

	internal static void HandleIfdEntry(ushort tag, ushort type, uint value, bool littleEndian,
		ref ushort compression, ref uint jpegOffset, ref uint jpegLength)
	{
		switch ( tag )
		{
			case 0x0103: // Compression
				if ( type == 3 )
				{
					ushort compVal;
					if ( littleEndian )
					{
						compVal = ( ushort ) ( value & 0xFFFF );
					}
					else
					{
						compVal = ( ushort ) ( value >> 16 );
					}

					compression = compVal;
				}
				else
				{
					compression = ( ushort ) value;
				}

				break;
			case 0x0201: // Thumbnail offset
				jpegOffset = value;
				break;
			case 0x0202: // Thumbnail length
				jpegLength = value;
				break;
		}
	}

	internal static bool TryResolveAndValidateOffset(Stream input, int tiffBase,
		uint candidateOffset,
		uint candidateLength,
		out uint resolvedOffset)
	{
		resolvedOffset = 0;
		if ( candidateOffset == 0 || candidateLength < MinJpegSize )
		{
			return false;
		}

		if ( IsValidJpegRange(input, candidateOffset, candidateLength) )
		{
			resolvedOffset = candidateOffset;
			return true;
		}

		var relativeOffset = ( uint ) ( tiffBase + ( int ) candidateOffset );
		if ( !IsValidJpegRange(input, relativeOffset, candidateLength) )
		{
			return false;
		}

		resolvedOffset = relativeOffset;
		return true;
	}

	internal static bool IsValidJpegRange(Stream input, uint offset, uint length)
	{
		if ( offset + length > input.Length || !StreamPrimitives.TrySeek(input, offset) )
		{
			return false;
		}

		var marker = new byte[3];
		if ( input.Read(marker, 0, 3) != 3 )
		{
			return false;
		}

		return marker.SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF });
	}

	internal static async Task<bool> CopyRangeAsync(Stream input, Stream output, uint offset,
		uint length)
	{
		if ( !StreamPrimitives.TrySeek(input, offset) )
		{
			return false;
		}

		var remaining = ( long ) length;
		var buffer = new byte[64 * 1024];
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

	internal static bool TryReadUInt16(Stream input, bool littleEndian, out ushort value)
	{
		var b = new byte[2];
		value = 0;
		if ( input.Read(b, 0, 2) != 2 )
		{
			return false;
		}

		value = littleEndian
			? ( ushort ) ( b[0] | ( b[1] << 8 ) )
			: ( ushort ) ( ( b[0] << 8 ) | b[1] );
		return true;
	}

	internal static bool TryReadUInt32(Stream input, bool littleEndian, out uint value)
	{
		var b = new byte[4];
		value = 0;
		if ( input.Read(b, 0, 4) != 4 )
		{
			return false;
		}

		value = littleEndian
			? ( uint ) ( b[0] | ( b[1] << 8 ) | ( b[2] << 16 ) | ( b[3] << 24 ) )
			: ( ( uint ) b[0] << 24 ) | ( ( uint ) b[1] << 16 ) | ( ( uint ) b[2] << 8 ) | b[3];
		return true;
	}
}
