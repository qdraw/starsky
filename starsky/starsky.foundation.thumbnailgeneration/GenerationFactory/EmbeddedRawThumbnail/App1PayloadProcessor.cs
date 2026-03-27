using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbedded;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

internal static class App1PayloadProcessor
{
	public static async Task<bool> Process(byte[] payload, Stream? outputLarge,
		Stream? originalStream, long payloadStart)
	{
		if ( !IsValidExifHeader(payload) )
		{
			return false;
		}

		using var tiffMs = new MemoryStream(payload, 6, payload.Length - 6, false);

		if ( !TiffEmbeddedPreviewExtractor.TryParseTiffHeader(tiffMs, out var littleEndian,
			    out var firstIfdOffset) )
		{
			return false; // Not a valid TIFF header inside EXIF
		}

		var candidates = new List<PreviewCandidate>();

		var ctx = new ParseTraversalContext
		{
			Previews = candidates,
			Visited = new HashSet<uint>(),
			ReferenceInfo = "EXIF(APP1)",
			RawFlavor = RawFlavor.Unknown
		};

		// Traverse TIFF IFDs within the APP1 payload
		TiffEmbeddedPreviewExtractor.ParseIfdRecursive(
			tiffMs,
			firstIfdOffset, littleEndian, ctx, 0,
			false);

		// Optionally include scanned JPEGs from the original stream (if available)
		AddScannedCandidates(originalStream, candidates);

		// Fill missing dimensions by probing the appropriate streams
		EnrichCandidatesWithDimensions(candidates, tiffMs, originalStream, payloadStart);

		var best = SelectBestPreviewHelper.SelectBestPreview(candidates);
		if ( best == null )
		{
			return false;
		}

		if ( outputLarge == null )
		{
			return true;
		}

		return await TryExtractBestPreview(best, tiffMs,
				originalStream, payloadStart, outputLarge)
			.ConfigureAwait(false);
	}

	private static bool IsValidExifHeader(byte[] payload)
	{
		return !( payload.Length < 6 ||
		          payload[0] != ( byte ) 'E' ||
		          payload[1] != ( byte ) 'x' ||
		          payload[2] != ( byte ) 'i' ||
		          payload[3] != ( byte ) 'f' ||
		          payload[4] != 0 ||
		          payload[5] != 0 );
	}

	internal static void AddScannedCandidates(Stream? originalStream,
		List<PreviewCandidate> candidates)
	{
		if ( originalStream == null )
		{
			return;
		}

		try
		{
			var maxLen = originalStream.Length > uint.MaxValue
				? uint.MaxValue
				: ( uint ) originalStream.Length;
			foreach ( var scanCandidate in TiffEmbeddedPreviewExtractor.ScanJpegsInRange(
				         originalStream, 0, maxLen) )
			{
				if ( scanCandidate == null )
				{
					continue;
				}

				// skip the primary JPEG at offset 0
				if ( scanCandidate.Offset == 0 )
				{
					continue;
				}

				candidates.Add(scanCandidate);
				if ( candidates.Count >= 16 )
				{
					break;
				}
			}
		}
		catch
		{
			// ignore scan failures and continue with available candidates
		}
	}

	private static void EnrichCandidatesWithDimensions(
		List<PreviewCandidate> candidates, Stream tiffMs,
		Stream? originalStream, long payloadStart)
	{
		// only for candidates that don't already have dimensions from TIFF tags
		foreach ( var c in candidates.Where(c
			         => c.Width == 0 || c.Height == 0)
		        )
		{
			// Try mapped original stream first: payloadStart + 6 + offset
			if ( originalStream != null )
			{
				var mapped = payloadStart + 6 + c.Offset;
				if ( mapped >= 0 && mapped + c.Length <= originalStream.Length &&
				     TiffEmbeddedPreviewExtractor.TryGetJpegDimensionsAtOffset(originalStream,
					     ( uint ) mapped, c.Length,
					     out var wMapped, out var hMapped) )
				{
					c.Width = wMapped;
					c.Height = hMapped;
					continue;
				}

				// fallback: treat offset as absolute in the original stream
				if ( c.Offset < originalStream.Length &&
				     TiffEmbeddedPreviewExtractor.TryGetJpegDimensionsAtOffset(
					     originalStream,
					     c.Offset, c.Length,
					     out var wAbs, out var hAbs)
				   )
				{
					c.Width = wAbs;
					c.Height = hAbs;
					continue;
				}
			}

			// final fallback: try inside APP1 TIFF memory stream
			if ( TiffEmbeddedPreviewExtractor.TryGetJpegDimensionsAtOffset(tiffMs, c.Offset,
				    c.Length, out var w, out var h) )
			{
				c.Width = w;
				c.Height = h;
			}
		}
	}

	internal static async Task<bool> TryExtractBestPreview(
		PreviewCandidate best, Stream tiffMs, Stream? originalStream,
		long payloadStart, Stream outputLarge)
	{
		// Prefer extraction from the APP1 memory stream when the candidate lies within it.
		if ( best.Offset + best.Length <= ( uint ) tiffMs.Length )
		{
			var preview =
				new PreviewCandidate { Offset = best.Offset, Length = best.Length };
			return await TiffEmbeddedPreviewExtractor
				.ExtractPreviewToStream(tiffMs, preview, outputLarge).ConfigureAwait(false);
		}

		if ( originalStream == null )
		{
			return false;
		}

		// Try the mapped offset next (payloadStart + 6 + offset)
		var mappedBest = payloadStart + 6 + best.Offset;
		if ( mappedBest >= 0 && mappedBest + best.Length <= originalStream.Length )
		{
			var preview = new PreviewCandidate
			{
				Offset = ( uint ) mappedBest, Length = best.Length
			};
			if ( TiffEmbeddedPreviewExtractor.TryValidateJpegOffset(originalStream, preview.Offset,
				    preview.Length) )
			{
				return await TiffEmbeddedPreviewExtractor
					.ExtractPreviewToStream(originalStream, preview, outputLarge)
					.ConfigureAwait(false);
			}
		}

		// Lastly, try interpreting the offset as an absolute offset in the original stream
		if ( best.Offset + best.Length > ( uint ) originalStream.Length )
		{
			return false;
		}

		var preview2 =
			new PreviewCandidate { Offset = best.Offset, Length = best.Length };
		if ( TiffEmbeddedPreviewExtractor.TryValidateJpegOffset(originalStream, preview2.Offset,
			    preview2.Length) )
		{
			return await TiffEmbeddedPreviewExtractor
				.ExtractPreviewToStream(originalStream, preview2, outputLarge)
				.ConfigureAwait(false);
		}

		return false;
	}
}
