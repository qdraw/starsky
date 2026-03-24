using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Extracts embedded JPEG previews from ISO Base Media File Format (ISOBMFF) containers.
///     Supports: CR3 (Canon EOS RAW 3), HEIF/HEIC (Apple, etc.)
///     Parses box hierarchies to find preview metadata and extract JPEG images.
/// </summary>
public class ContainerFormatPreviewExtractor
{
	private const int MinJpegSize = 4096;
	private const uint BoxTypeFtyp = 0x66747970; // 'ftyp' - File type box

	private readonly IWebLogger _logger;
	private readonly IStorage _subPathStorage;
	private readonly IStorage _tempStorage;

	public ContainerFormatPreviewExtractor(IWebLogger logger, ISelectorStorage selectorStorage)
	{
		_logger = logger;
		_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_tempStorage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
	}

	/// <summary>
	///     Attempts to extract preview from ISOBMFF container (CR3, HEIF, etc.)
	/// </summary>
	public async Task<bool> TryExtract(string subPathRawFile, string? outputLargePath)
	{
		if ( !_subPathStorage.ExistFile(subPathRawFile) )
		{
			return false;
		}

		try
		{
			await using var input = _subPathStorage.ReadStream(subPathRawFile);
			await using var output = 
				outputLargePath != null ? new MemoryStream() : null;

			var ok = TryExtractFromStream(input,
				$"Reference: {subPathRawFile}", output);
			if ( !ok )
			{
				return false;
			}

			if ( output == null )
			{
				return true;
			}

			output.Seek(0, SeekOrigin.Begin);
			return 
			       await _tempStorage.WriteStreamAsync(output, outputLargePath!);
		}
		catch ( Exception ex )
		{
			_logger.LogDebug(
				$"[ContainerFormatPreviewExtractor] " +
				$"Failed to extract from {subPathRawFile}: {ex.Message}");
			return false;
		}
	}

	private bool TryExtractFromStream(Stream input, string referenceInfo, Stream? output)
	{
		// Verify ISOBMFF format
		if ( !TryVerifyIsobmffFormat(input, out var containerType) )
		{
			_logger.LogDebug($"[ContainerFormatPreviewExtractor] {referenceInfo}: " +
			                 $"Not a valid ISOBMFF container");
			return false;
		}

		_logger.LogDebug($"[ContainerFormatPreviewExtractor] {referenceInfo}: " +
		                 $"Detected {containerType} container");

		var ok = TryExtractBestJpegByStrictScan(input, output);
		if ( ok )
		{
			return true;
		}

		_logger.LogDebug($"[ContainerFormatPreviewExtractor] {referenceInfo}: " +
		                 $"No JPEG preview found in container");
		return false;

	}

	private static bool TryExtractBestJpegByStrictScan(Stream input, Stream? output)
	{
		if ( !input.CanSeek || input.Length < MinJpegSize )
		{
			return false;
		}

		var bytes = ReadAllBytes(input);
		var (bestOffset, bestLength) = FindBestJpegRange(bytes);
		if ( bestOffset < 0 )
		{
			return false;
		}

		output?.Write(bytes, bestOffset, bestLength);
		return true;
	}

	private static byte[] ReadAllBytes(Stream input)
	{
		input.Seek(0, SeekOrigin.Begin);
		using var ms = new MemoryStream(( int ) Math.Min(input.Length, int.MaxValue));
		input.CopyTo(ms);
		return ms.ToArray();
	}

	private static (int Offset, int Length) FindBestJpegRange(byte[] bytes)
	{
		var bestOffset = -1;
		var bestLength = 0;
		var i = 0;

		while ( i <= bytes.Length - 3 )
		{
			if ( !IsJpegStart(bytes, i) )
			{
				i++;
				continue;
			}

			var end = FindJpegEnd(bytes, i + 3);
			if ( end < 0 )
			{
				i++;
				continue;
			}

			var length = end - i + 1;
			if ( length >= MinJpegSize && length > bestLength )
			{
				bestOffset = i;
				bestLength = length;
			}

			i = end + 1;
		}

		return (bestOffset, bestLength);
	}

	private static bool IsJpegStart(byte[] bytes, int index)
	{
		return bytes[index] == 0xFF && bytes[index + 1] == 0xD8 && bytes[index + 2] == 0xFF;
	}

	private static int FindJpegEnd(byte[] bytes, int start)
	{
		for ( var j = start; j < bytes.Length; j++ )
		{
			if ( bytes[j - 1] == 0xFF && bytes[j] == 0xD9 )
			{
				return j;
			}
		}

		return -1;
	}

	private static bool TryVerifyIsobmffFormat(Stream input, out string containerType)
	{
		containerType = "Unknown";
		if ( input.Length < 32 )
		{
			return false;
		}

		input.Seek(0, SeekOrigin.Begin);
		Span<byte> header = stackalloc byte[32];
		if ( input.Read(header) < 32 )
		{
			return false;
		}

		// First box must be 'ftyp'
		var boxSize = ReadUInt32BigEndian(header[0..4]);
		var boxType = ReadUInt32BigEndian(header[4..8]);

		if ( boxType != BoxTypeFtyp || boxSize < 20 || boxSize > input.Length )
		{
			return false;
		}

		// Check brand
		var brand = new string(new[] { ( char ) header[8], ( char ) header[9], 
			( char ) header[10], ( char ) header[11] });

		containerType = brand.Trim('\0') switch
		{
			"crx " => "CR3 (Canon RAW 3)",
			"mif1" => "HEIF (High Efficiency Image Format)",
			"heic" => "HEIC (Apple)",
			"heix" => "HEIC (10-bit)",
			_ => $"ISOBMFF ({brand})"
		};

		return true;
	}

	private static uint ReadUInt32BigEndian(ReadOnlySpan<byte> b)
	{
		return ( ( uint ) b[0] << 24 ) | ( ( uint ) b[1] << 16 ) | ( ( uint ) b[2] << 8 ) | b[3];
	}
}

