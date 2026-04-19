using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class RawNormalization
{
	internal static float NormalizeSample(ushort value, float blackLevel, float whiteLevel)
	{
		var range = whiteLevel - blackLevel;
		if ( range <= 0f )
		{
			return 0f;
		}

		return Math.Clamp(( value - blackLevel ) / range, 0f, 1f);
	}

	/// <summary>
	/// Normalizes Bayer CFA data to linear [0..1] using per-channel black/white levels.
	/// According to DNG spec, BlackLevel and WhiteLevel can be arrays with one value per CFA channel.
	/// </summary>
	internal static float[,] NormalizeBayerToLinear(ushort[,] bayer, float[] blackLevels,
		float[] whiteLevels, byte[] cfaPattern)
	{
		var height = bayer.GetLength(0);
		var width = bayer.GetLength(1);
		var normalized = new float[height, width];

		for ( var y = 0; y < height; y++ )
		{
			for ( var x = 0; x < width; x++ )
			{
				// CFA site index in 2x2 tile: 0..3
				var cfaIndex = ( y & 1 ) * 2 + ( x & 1 );
				var cfaChannel = cfaPattern[cfaIndex];
				
				// DNG arrays can be per-site (common: 4 entries), per-color, or scalar.
				var blackLevel = ResolveLevel(blackLevels, cfaIndex, cfaChannel, 0f);
				var whiteLevel = ResolveLevel(whiteLevels, cfaIndex, cfaChannel, 65535f);

				normalized[y, x] = NormalizeSample(bayer[y, x], blackLevel, whiteLevel);
			}
		}

		return normalized;
	}

	private static float ResolveLevel(float[] levels, int cfaIndex, int cfaChannel,
		float fallback)
	{
		if ( levels.Length == 0 )
		{
			return fallback;
		}

		if ( levels.Length == 1 )
		{
			return levels[0];
		}

		// For arrays with 2+ values, prefer per-CFA-site indexing (standard DNG interpretation)
		// This handles cases where each value corresponds to a position in the 2×2 Bayer pattern:
		// [0,1,1,2] RGGB pattern with levels [60,50,50,60] means:
		//   Site 0 (R) → levels[0] = 60
		//   Site 1 (G) → levels[1] = 50
		//   Site 2 (G) → levels[2] = 50 (second green, can differ from site 1)
		//   Site 3 (B) → levels[3] = 60
		// This is the standard interpretation and works correctly for both Leica and standard cameras.
		
		if ( cfaIndex >= 0 && cfaIndex < levels.Length )
		{
			return levels[cfaIndex];
		}

		// Fallback: interpret as per-color indexing if cfaIndex is out of range.
		// This handles cases with fewer than 4 values.
		if ( cfaChannel >= 0 && cfaChannel < levels.Length )
		{
			return levels[cfaChannel];
		}

		return levels[^1];
	}
}
