using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	private const int MinJpegSize = 4096; // 4KB minimum for valid JPEG
	private const int MaxBoxDepth = 6;
	private const int MaxBoxesVisited = 256;

	// ISO Base Media File Format box types
	private const uint BoxTypeFtyp = 0x66747970; // 'ftyp' - File type box
	private const uint BoxTypeWide = 0x77696465; // 'wide' - Wide box (deprecated)
	private const uint BoxTypeMdat = 0x6D646174; // 'mdat' - Media data box
	private const uint BoxTypeMoov = 0x6D6F6F76; // 'moov' - Movie box
	private const uint BoxTypeTrak = 0x7472616B; // 'trak' - Track box
	private const uint BoxTypeUdta = 0x75647461; // 'udta' - User data box
	private const uint BoxTypeMeta = 0x6D657461; // 'meta' - Metadata box
	private const uint BoxTypeIlst = 0x696C7374; // 'ilst' - Item list box
	private const uint BoxTypeCr3Preview = 0xC5A1DC00; // Canon CR3 preview data (approximate)

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
			using var input = _subPathStorage.ReadStream(subPathRawFile);
			await using var output = outputLargePath != null ? new MemoryStream() : null;

			var preview = await TryExtractFromStream(input,
				$"Reference: {subPathRawFile}");
			if ( preview == null )
			{
				return false;
			}

			if ( output == null )
			{
				return true; // Success, just not saving
			}

			output.Write(preview, 0, preview.Length);
			output.Seek(0, SeekOrigin.Begin);
			return await _tempStorage.WriteStreamAsync(output, outputLargePath);
		}
		catch ( Exception ex )
		{
			_logger.LogDebug(
				$"[ContainerFormatPreviewExtractor] Failed to extract from {subPathRawFile}: {ex.Message}");
			return false;
		}
	}

	private async Task<byte[]?> TryExtractFromStream(Stream input, string referenceInfo)
	{
		// Verify ISOBMFF format
		if ( !TryVerifyIsobmffFormat(input, out var containerType) )
		{
			_logger.LogDebug($"[ContainerFormatPreviewExtractor] {referenceInfo}: Not a valid ISOBMFF container");
			return null;
		}

		_logger.LogDebug($"[ContainerFormatPreviewExtractor] {referenceInfo}: Detected {containerType} container");

		// For ISOBMFF containers, scan the entire file for JPEG data
		// This is more reliable than trying to navigate box hierarchy for various formats
		var previews = ScanForJpegs(input);

		if ( previews.Count == 0 )
		{
			_logger.LogDebug($"[ContainerFormatPreviewExtractor] {referenceInfo}: No JPEG preview found in container");
			return null;
		}

		// Select best preview (largest)
		var best = previews[0];
		return best;
	}

	private bool TryVerifyIsobmffFormat(Stream input, out string containerType)
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
		var brand = new string(new[] { ( char ) header[8], ( char ) header[9], ( char ) header[10], ( char ) header[11] });

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

	private List<byte[]> ScanForJpegs(Stream input)
	{
		var previews = new List<byte[]>();
		if ( input.Length < MinJpegSize )
		{
			return previews;
		}

		input.Seek(0, SeekOrigin.Begin);
		var buffer = new byte[256 * 1024]; // 256KB buffer
		var b0 = -1;
		var b1 = -1;
		var fileOffset = 0;

		while ( fileOffset < input.Length )
		{
			var toRead = ( int ) Math.Min(buffer.Length, input.Length - fileOffset);
			var bytesRead = input.Read(buffer, 0, toRead);
			if ( bytesRead <= 0 )
			{
				break;
			}

			for ( var i = 0; i < bytesRead; i++ )
			{
				var b2 = buffer[i];

				// Look for JPEG SOI marker (0xFF 0xD8)
				if ( b0 == 0xFF && b1 == 0xD8 && b2 == 0xFF )
				{
					var jpegStart = fileOffset + i - 2;
					var jpegLength = DetectJpegLength(input, ( uint ) jpegStart);

					if ( jpegLength >= MinJpegSize )
					{
						if ( TrySeek(input, ( uint ) jpegStart) )
						{
							var jpegData = new byte[jpegLength];
							if ( input.Read(jpegData, 0, ( int ) jpegLength) == jpegLength )
							{
								previews.Add(jpegData);
								i += ( int ) jpegLength - 2; // Skip past this JPEG
							}
						}
					}
				}

				b0 = b1;
				b1 = b2;
			}

			fileOffset += bytesRead;
		}

		// Sort by size (largest first)
		previews.Sort((a, b) => b.Length.CompareTo(a.Length));
		return previews;
	}

	private static uint DetectJpegLength(Stream input, uint jpegStart)
	{
		if ( !TrySeek(input, jpegStart) )
		{
			return 0;
		}

		// Scan for JPEG EOI marker (0xFF 0xD9)
		var buffer = new byte[64 * 1024];
		var scanned = 0;
		const int maxScan = 100 * 1024 * 1024; // 100 MB max
		var previous = -1;

		while ( scanned < maxScan )
		{
			var toRead = Math.Min(buffer.Length, maxScan - scanned);
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

		return 0; // EOI not found
	}

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

	// ...existing code...
	private static uint ReadUInt32BigEndian(ReadOnlySpan<byte> b)
	{
		return ( ( uint ) b[0] << 24 ) | ( ( uint ) b[1] << 16 ) | ( ( uint ) b[2] << 8 ) | b[3];
	}
}





