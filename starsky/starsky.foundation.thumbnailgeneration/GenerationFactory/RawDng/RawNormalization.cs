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
				// Determine which CFA channel this pixel belongs to
				var cfaIndex = ( y & 1 ) * 2 + ( x & 1 );
				var cfaChannel = cfaPattern[cfaIndex];
				
				// Clamp to available per-channel levels (max 4 CFA channels)
				var blackLevel = cfaChannel < blackLevels.Length ? blackLevels[cfaChannel] : 0f;
				var whiteLevel = cfaChannel < whiteLevels.Length ? whiteLevels[cfaChannel] : 65535f;

				normalized[y, x] = NormalizeSample(bayer[y, x], blackLevel, whiteLevel);
			}
		}

		return normalized;
	}

	/// <summary>
	/// Legacy overload for backward compatibility. Falls back to uniform black/white levels.
	/// </summary>
	internal static float[,] NormalizeBayerToLinear(ushort[,] bayer, float blackLevel,
		float whiteLevel)
	{
		var blackLevels = new[] { blackLevel, blackLevel, blackLevel, blackLevel };
		var whiteLevels = new[] { whiteLevel, whiteLevel, whiteLevel, whiteLevel };
		var cfaPattern = new byte[] { 0, 1, 1, 2 }; // Assume RGGB
		return NormalizeBayerToLinear(bayer, blackLevels, whiteLevels, cfaPattern);
	}
}

