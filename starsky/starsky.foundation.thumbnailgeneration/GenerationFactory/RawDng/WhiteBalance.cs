namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class WhiteBalance
{
	internal static float[] GainsFromAsShotNeutral(float[] asShotNeutral)
	{
		if ( asShotNeutral.Length < 3 )
		{
			return [1f, 1f, 1f];
		}

		var r = InverseOrOne(asShotNeutral[0]);
		var g = InverseOrOne(asShotNeutral[1]);
		var b = InverseOrOne(asShotNeutral[2]);

		// Normalize by green to keep G at 1.0 in linear space.
		if ( g <= 0f )
		{
			return [1f, 1f, 1f];
		}

		return [r / g, 1f, b / g];
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

	private static float InverseOrOne(float value)
	{
		return value > 0f ? 1f / value : 1f;
	}
}


