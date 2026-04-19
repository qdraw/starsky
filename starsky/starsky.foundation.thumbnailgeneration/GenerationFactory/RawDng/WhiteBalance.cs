using System;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class WhiteBalance
{
	internal static float[] GainsFromAsShotNeutral(float[] asShotNeutral)
	{
		if ( asShotNeutral.Length < 3 )
		{
			return [1f, 1f, 1f];
		}

		// AsShotNeutral values represent the RGB values that produce neutral under capture illuminant.
		// To correct white balance, we invert these values to get gains.
		var r = asShotNeutral[0];
		var g = asShotNeutral[1];
		var b = asShotNeutral[2];

		if (r <= 0f || g <= 0f || b <= 0f)
		{
			return [1f, 1f, 1f];
		}

		// Compute gains as inverse of neutral values
		var gainR = 1f / r;
		var gainG = 1f / g;
		var gainB = 1f / b;

		// Normalize by GREEN channel (standard practice).
		// Green is the most perceptually important channel.
		return [gainR / gainG, 1f, gainB / gainG];
	}

	internal static void ApplyInPlace(float[,,] linearRgb, float[] gains)
	{
		if ( gains.Length < 3 )
		{
			return;
		}

		var height = linearRgb.GetLength(0);
		var width = linearRgb.GetLength(1);
		for ( var y = 0; y < height; y++ )
		{
			for ( var x = 0; x < width; x++ )
			{
				linearRgb[y, x, 0] *= gains[0];
				linearRgb[y, x, 1] *= gains[1];
				linearRgb[y, x, 2] *= gains[2];
			}
		}
	}
}


