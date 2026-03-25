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

		return await ProcessJpegMarkersAsync(input, outputLarge);
	}

	private static async Task<bool> ProcessJpegMarkersAsync(Stream input, Stream? outputLarge)
	{
		while (true)
		{
			var marker = ReadNextMarker(input);
			if (marker == -1)
			{
				return false; // EOF / error
			}

			// EOI
			if (marker == 0xD9)
			{
				break;
			}

			// Standalone markers (no payload)
			if (IsStandaloneMarker(marker))
			{
				continue;
			}

			var result = await ProcessNonStandaloneMarkerAsync(input, marker, outputLarge).ConfigureAwait(false);
			switch (result)
			{
				case MarkerProcessingResult.Found:
					return true;
				case MarkerProcessingResult.Error:
					return false;
				case MarkerProcessingResult.Continue:
				default:
					continue;
			}
		}

		return false;
	}

	private enum MarkerProcessingResult
	{
		Continue,
		Found,
		Error
	}

	private static async Task<MarkerProcessingResult> ProcessNonStandaloneMarkerAsync(Stream input, int marker, Stream? outputLarge)
	{
		// Read the segment length for non-standalone markers
		if (!TryReadSegmentLength(input, out var segLen))
		{
			return MarkerProcessingResult.Error;
		}

		var payloadSize = Math.Max(0, segLen - 2);

		if (marker != 0xE1)
		{
			// Not APP1 — skip the payload
			var ok = await SkipSegmentAsync(input, payloadSize).ConfigureAwait(false);
			return ok ? MarkerProcessingResult.Continue : MarkerProcessingResult.Error;
		}

		// APP1: read the payload and attempt to process EXIF TIFF payload
		var payload = await ReadSegmentPayloadAsync(input, payloadSize).ConfigureAwait(false);
		if (payload == null)
		{
			return MarkerProcessingResult.Error;
		}

		var processed = await ProcessApp1PayloadAsync(payload, outputLarge).ConfigureAwait(false);
		return processed ? MarkerProcessingResult.Found : MarkerProcessingResult.Continue;
	}

	// helpers
	private static int ReadNextMarker(Stream input)
	{
		int prefix;
		do
		{
			prefix = input.ReadByte();
			if ( prefix == -1 )
			{
				return -1;
			}
		} while ( prefix != 0xFF );

		int marker;
		do
		{
			marker = input.ReadByte();
			if ( marker == -1 )
			{
				return -1;
			}
		} while ( marker == 0xFF );

		return marker;
	}

	private static bool IsStandaloneMarker(int marker)
	{
		return marker is >= 0xD0 and <= 0xD7 or 0x01;
	}

	private static bool TryReadSegmentLength(Stream input, out int segLen)
	{
		segLen = 0;
		var lenBuf = new byte[2];
		var rl = input.Read(lenBuf, 0, 2);
		if ( rl < 2 )
		{
			return false;
		}

		segLen = ( lenBuf[0] << 8 ) | lenBuf[1];
		return true;
	}

	private static async Task<byte[]?> ReadSegmentPayloadAsync(Stream input, int payloadSize)
	{
		if ( payloadSize == 0 )
		{
			return [];
		}

		var payload = new byte[payloadSize];
		var read = 0;
		while ( read < payloadSize )
		{
			var r = await input.ReadAsync(payload.AsMemory(read, payloadSize - read));
			if ( r <= 0 )
			{
				return null;
			}

			read += r;
		}

		return payload;
	}

	private static async Task<bool> SkipSegmentAsync(Stream input, int payloadSize)
	{
		if ( payloadSize == 0 )
		{
			return true;
		}

		if ( input.CanSeek )
		{
			return StreamPrimitives.TrySeek(input, input.Position + payloadSize);
		}

		var skipBuf = new byte[4096];
		var remaining = payloadSize;
		while ( remaining > 0 )
		{
			var toRead = Math.Min(skipBuf.Length, remaining);
			var rr = await input.ReadAsync(skipBuf.AsMemory(0, toRead));
			if ( rr <= 0 )
			{
				return false;
			}

			remaining -= rr;
		}

		return true;
	}

	private static async Task<bool> ProcessApp1PayloadAsync(byte[] payload, Stream? outputLarge)
	{
		// Exif header: "Exif\0\0"
		if ( payload.Length < 6 || payload[0] != ( byte ) 'E' || payload[1] != ( byte ) 'x' ||
		     payload[2] != ( byte ) 'i' || payload[3] != ( byte ) 'f' || payload[4] != 0 ||
		     payload[5] != 0 )
		{
			return false;
		}

		using var tiffMs = new MemoryStream(payload, 6, payload.Length - 6, false);

		if ( !TiffEmbeddedPreviewExtractor.TryParseTiffHeader(tiffMs, out var littleEndian,
			    out var firstIfdOffset) )
		{
			// Not a valid TIFF header inside EXIF
			return false;
		}

		var candidates = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
		var ctx = new TiffEmbeddedPreviewExtractor.ParseTraversalContext
		{
			Previews = candidates,
			Visited = [],
			ReferenceInfo = "EXIF(APP1)",
			RawFlavor = RawFlavor.Unknown
		};

		// Traverse TIFF IFDs within the APP1 payload
		TiffEmbeddedPreviewExtractor.ParseIfdRecursive(tiffMs, firstIfdOffset, littleEndian, ctx, 0,
			false);

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
		return await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(tiffMs, preview,
			outputLarge);
	}
}
