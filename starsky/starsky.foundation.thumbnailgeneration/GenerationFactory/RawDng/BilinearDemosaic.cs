using System;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class BilinearDemosaic
{
	internal static float[,,] Demosaic(float[,] normalizedBayer, byte[] cfaPattern)
	{
		if ( cfaPattern.Length < 4 )
		{
			throw new ArgumentException("CFA pattern must contain at least 4 entries", nameof(cfaPattern));
		}

		var height = normalizedBayer.GetLength(0);
		var width = normalizedBayer.GetLength(1);
		var rgb = new float[height, width, 3];

		for ( var y = 0; y < height; y++ )
		{
			for ( var x = 0; x < width; x++ )
			{
				for ( var channel = 0; channel < 3; channel++ )
				{
					rgb[y, x, channel] = InterpolateChannel(normalizedBayer, cfaPattern, x, y,
						channel);
				}
			}
		}

		return rgb;
	}

	private static float InterpolateChannel(float[,] bayer, byte[] cfaPattern, int x, int y,
		int channel)
	{
		if ( CfaChannelAt(cfaPattern, x, y) == channel )
		{
			return bayer[y, x];
		}

		var width = bayer.GetLength(1);
		var height = bayer.GetLength(0);
		var sum = 0f;
		var count = 0;

		for ( var ny = Math.Max(0, y - 1); ny <= Math.Min(height - 1, y + 1); ny++ )
		{
			for ( var nx = Math.Max(0, x - 1); nx <= Math.Min(width - 1, x + 1); nx++ )
			{
				if ( CfaChannelAt(cfaPattern, nx, ny) != channel )
				{
					continue;
				}

				sum += bayer[ny, nx];
				count++;
			}
		}

		if ( count > 0 )
		{
			return sum / count;
		}

		return bayer[y, x];
	}

	private static int CfaChannelAt(byte[] cfaPattern, int x, int y)
	{
		return cfaPattern[( y & 1 ) * 2 + ( x & 1 )];
	}
}


