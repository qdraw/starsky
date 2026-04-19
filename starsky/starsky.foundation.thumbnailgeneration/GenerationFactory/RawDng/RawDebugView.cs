namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class RawDebugView
{
	internal static byte[,] CreateRawGrayscale(DngRawImage raw)
	{
		var h = raw.Height;
		var w = raw.Width;
		var output = new byte[h, w];
		for ( var y = 0; y < h; y++ )
		{
			for ( var x = 0; x < w; x++ )
			{
				var normalized = RawNormalization.NormalizeSample(raw.Bayer[y, x],
					raw.BlackLevel, raw.WhiteLevel);
				output[y, x] = ( byte ) ( normalized * 255f + 0.5f );
			}
		}

		return output;
	}
}

