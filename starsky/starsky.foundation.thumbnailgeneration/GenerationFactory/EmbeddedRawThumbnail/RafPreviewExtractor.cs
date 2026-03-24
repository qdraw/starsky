using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Extracts embedded JPEG previews from Fujifilm RAF containers.
///     Selection prefers JPEG candidates with IPTC APP13 metadata.
/// </summary>
public class RafPreviewExtractor(IWebLogger logger, ISelectorStorage selectorStorage)
{
	private const int MinJpegSize = 4096;
	private const int RafHeaderMinBytes = 0x5C;
	private const int RafPreviewOffsetField = 0x54;
	private const int RafPreviewLengthField = 0x58;
	private static readonly byte[] RafSignature = "FUJIFILMCCD-RAW "u8.ToArray();

	private IStorage SubPathStorage => selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
	private IStorage TempStorage => selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

	public async Task<bool> TryExtract(string subPathRawFile, string? outputLargePath)
	{
		try
		{
			if ( !SubPathStorage.ExistFile(subPathRawFile) )
			{
				return false;
			}

			await using var input = SubPathStorage.ReadStream(subPathRawFile);
			await using var output = new MemoryStream();

			var ok = await TryExtractByRafHeader(input, output);
			if ( !ok )
			{
				output.SetLength(0);
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
				$"[RafPreviewExtractor] Failed to extract from {subPathRawFile}: {exception.Message}");
			return false;
		}
	}

	private static async Task<bool> TryExtractByRafHeader(Stream input, Stream output)
	{
		if ( !input.CanSeek || input.Length < RafHeaderMinBytes )
		{
			return false;
		}

		var (offset, length) = TryReadHeaderPreviewRange(input);
		if ( !IsValidPreviewRange(input, offset, length) )
		{
			return false;
		}

		if ( !HasJpegSoiAt(input, offset) )
		{
			return false;
		}

		return await CopyRange(input, output, offset, length);
	}

	private static (uint Offset, uint Length) TryReadHeaderPreviewRange(Stream input)
	{
		if ( !StreamPrimitives.TrySeek(input, 0) )
		{
			return ( 0, 0 );
		}

		Span<byte> header = stackalloc byte[RafHeaderMinBytes];
		if ( input.Read(header) < RafHeaderMinBytes ||
		     !header[..RafSignature.Length].SequenceEqual(RafSignature) )
		{
			return ( 0, 0 );
		}

		var offset = ReadUInt32BigEndian(header[RafPreviewOffsetField..]);
		var length = ReadUInt32BigEndian(header[RafPreviewLengthField..]);
		return ( offset, length );
	}

	private static bool IsValidPreviewRange(Stream input, uint offset, uint length)
	{
		if ( length < MinJpegSize || offset == 0 )
		{
			return false;
		}

		var end = ( long ) offset + length;
		return end > offset && end <= input.Length;
	}

	private static bool HasJpegSoiAt(Stream input, uint offset)
	{
		if ( !StreamPrimitives.TrySeek(input, offset) )
		{
			return false;
		}

		Span<byte> soi = stackalloc byte[3];
		return input.Read(soi) == 3 && soi[0] == 0xFF && soi[1] == 0xD8 && soi[2] == 0xFF;
	}

	internal static async Task<bool> CopyRange(Stream input, Stream output, uint offset,
		uint length)
	{
		if ( !StreamPrimitives.TrySeek(input, offset) )
		{
			return false;
		}

		var remaining = ( int ) length;
		var buffer = new byte[64 * 1024];
		while ( remaining > 0 )
		{
			var toRead = Math.Min(buffer.Length, remaining);
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

	private static uint ReadUInt32BigEndian(ReadOnlySpan<byte> b)
	{
		return ( ( uint ) b[0] << 24 ) | ( ( uint ) b[1] << 16 ) | ( ( uint ) b[2] << 8 ) | b[3];
	}
}
