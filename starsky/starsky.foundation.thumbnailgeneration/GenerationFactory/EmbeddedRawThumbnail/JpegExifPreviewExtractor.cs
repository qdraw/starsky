using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Extracts embedded JPEG previews from JPEG files that contain EXIF (APP1) TIFF payloads.
///     This reuses the TIFF-IFD parsing implemented in TiffEmbeddedPreviewExtractor by
///     copying the APP1 TIFF payload into a MemoryStream and calling the TIFF parsing helpers.
/// </summary>
public class JpegExifPreviewExtractor(IWebLogger logger, ISelectorStorage selectorStorage)
{
	private readonly IWebLogger _logger = logger;

	private readonly IStorage _subPathStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private readonly IStorage _tempStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

	public async Task<bool> TryExtract(string subPathRawFile, string? outputLargePath)
	{
		if ( !_subPathStorage.ExistFile(subPathRawFile) )
		{
			return false;
		}

		try
		{
			await using var input = _subPathStorage.ReadStream(subPathRawFile);
			await using var output = new MemoryStream();

			var ok = await TryExtractFromStream(input, output);
			if ( !ok || outputLargePath == null )
			{
				return ok;
			}

			if ( output.Length == 0 )
			{
				return false;
			}

			output.Seek(0, SeekOrigin.Begin);
			return await _tempStorage.WriteStreamAsync(output, outputLargePath);
		}
		catch ( Exception ex )
		{
			_logger.LogError(
				$"[JpegExifPreviewExtractor] Failed to extract from {subPathRawFile}: {ex.Message}");
			return false;
		}
	}

	private static async Task<bool> TryExtractFromStream(Stream input, Stream? outputLarge)
	{
		// Verify JPEG SOI
		if ( !StreamPrimitives.TrySeek(input, 0) )
		{
			return false;
		}

		var soi = new byte[2];
		var r = await input.ReadAsync(soi.AsMemory(0, 2));
		if ( r < 2 )
		{
			return false;
		}

		if ( soi[0] != 0xFF || soi[1] != 0xD8 )
		{
			return false;
		}

		// Scan markers until APP1 (0xE1) is found; support padding (0xFF..)
		while ( true )
		{
			var markerPrefix = input.ReadByte();
			if ( markerPrefix == -1 )
			{
				return false;
			}

			if ( markerPrefix != 0xFF )
			{
				continue;
			}

			int marker;
			do
			{
				marker = input.ReadByte();
				if ( marker == -1 )
				{
					return false;
				}
			} while ( marker == 0xFF );

			// EOI
			if ( marker == 0xD9 )
			{
				break;
			}

			// Standalone markers (RST0..RST7 or TEM) have no length/payload
			if ( ( marker >= 0xD0 && marker <= 0xD7 ) || marker == 0x01 )
			{
				continue;
			}

			// Read big-endian length
			var lenBuf = new byte[2];
			var rl = await input.ReadAsync(lenBuf, 0, 2);
			if ( rl < 2 )
			{
				return false;
			}

			var segLen = ( lenBuf[0] << 8 ) | lenBuf[1];
			var payloadSize = Math.Max(0, segLen - 2);

			if ( marker == 0xE1 ) // APP1 - Exif
			{
				var payload = new byte[payloadSize];
				var rp = await input.ReadAsync(payload, 0, payloadSize);
				if ( rp != payloadSize )
				{
					return false;
				}

				// Exif header: "Exif\0\0"
				if ( payloadSize >= 6 && payload[0] == ( byte ) 'E' && payload[1] == ( byte ) 'x' &&
				     payload[2] == ( byte ) 'i' && payload[3] == ( byte ) 'f' && payload[4] == 0 &&
				     payload[5] == 0 )
				{
					// TIFF payload starts at offset 6
					using var tiffMs = new MemoryStream(payload, 6, payload.Length - 6, false);

					if ( !TiffEmbeddedPreviewExtractor.TryParseTiffHeader(tiffMs,
						    out var littleEndian, out var firstIfdOffset) )
					{
						// Not a valid TIFF header inside EXIF
						return false;
					}

					var candidates = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
					var ctx = new TiffEmbeddedPreviewExtractor.ParseTraversalContext
					{
						Previews = candidates,
						Visited = new HashSet<uint>(),
						ReferenceInfo = "EXIF(APP1)",
						RawFlavor = RawFlavor.Unknown
					};

					// Traverse TIFF IFDs within the APP1 payload
					TiffEmbeddedPreviewExtractor.ParseIfdRecursive(tiffMs, firstIfdOffset,
						littleEndian, ctx, 0, false);

					var best = SelectBestPreviewHelper.SelectBestPreview(candidates);
					if ( best == null )
					{
						return false;
					}

					if ( outputLarge == null )
					{
						return true;
					}

					var preview =
						new TiffEmbeddedPreviewExtractor.PreviewCandidate
						{
							Offset = best.Offset, Length = best.Length
						};
					return await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(tiffMs,
						preview, outputLarge);
				}

				// if not EXIF header, continue scanning
			}
			else
			{
				// skip payload
				if ( input.CanSeek )
				{
					if ( !StreamPrimitives.TrySeek(input, input.Position + payloadSize) )
					{
						return false;
					}
				}
				else
				{
					var skipBuf = new byte[4096];
					var remaining = payloadSize;
					while ( remaining > 0 )
					{
						var toRead = Math.Min(skipBuf.Length, remaining);
						var rr = await input.ReadAsync(skipBuf, 0, toRead);
						if ( rr <= 0 )
						{
							return false;
						}

						remaining -= rr;
					}
				}
			}
		}

		return false;
	}
}
