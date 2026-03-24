using System.Collections.Generic;
using System.Runtime.CompilerServices;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

public static class SelectBestPreviewHelper
{
	internal static TiffEmbeddedPreviewExtractor.PreviewCandidate? SelectBestPreview(
		List<TiffEmbeddedPreviewExtractor.PreviewCandidate> candidates)
	{
		if ( candidates.Count == 0 )
		{
			return null;
		}

		TiffEmbeddedPreviewExtractor.PreviewCandidate? best = null;

		foreach ( var candidate in candidates )
		{
			if ( best == null )
			{
				best = candidate;
				continue;
			}

			if ( ShouldReplaceBest(best, candidate) )
			{
				best = candidate;
			}
		}

		return best;
	}

	private static bool ShouldReplaceBest(TiffEmbeddedPreviewExtractor.PreviewCandidate best,
		TiffEmbeddedPreviewExtractor.PreviewCandidate candidate)
	{
		var candidatePixels = GetPixelCount(candidate);
		var bestPixels = GetPixelCount(best);
		var candidateHasDimensions = candidatePixels > 0;
		var bestHasDimensions = bestPixels > 0;

		if ( candidateHasDimensions != bestHasDimensions )
		{
			return ResolveMixedDimensionPreference(candidate, best, candidateHasDimensions);
		}

		// Prefer higher resolution preview when dimensions are available; fallback to byte size.
		return candidatePixels > bestPixels ||
		       ( candidatePixels == bestPixels && candidate.Length > best.Length );
	}

	private static bool ResolveMixedDimensionPreference(
		TiffEmbeddedPreviewExtractor.PreviewCandidate candidate,
		TiffEmbeddedPreviewExtractor.PreviewCandidate best,
		bool candidateHasDimensions)
	{
		if ( candidateHasDimensions )
		{
			return !ShouldPreferUnknownDimensions(best.Length, candidate.Length);
		}

		return ShouldPreferUnknownDimensions(candidate.Length, best.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong GetPixelCount(TiffEmbeddedPreviewExtractor.PreviewCandidate candidate)
	{
		return ( ulong ) candidate.Width * candidate.Height;
	}

	private static bool ShouldPreferUnknownDimensions(uint unknownLength, uint knownLength)
	{
		// Some RAW files expose only tiny IFD dimensions for thumbnails while the true
		// preview is discovered by scanning MakerNote JPEG blobs without dimensions.
		return unknownLength >= ( ulong ) knownLength * 2;
	}
}
