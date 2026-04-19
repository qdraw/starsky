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

	internal static float[,] NormalizeBayerToLinear(ushort[,] bayer, float blackLevel,
		float whiteLevel)
	{
		var height = bayer.GetLength(0);
		var width = bayer.GetLength(1);
		var normalized = new float[height, width];

		for ( var y = 0; y < height; y++ )
		{
			for ( var x = 0; x < width; x++ )
			{
				normalized[y, x] = NormalizeSample(bayer[y, x], blackLevel, whiteLevel);
			}
		}

		return normalized;
	}
}

