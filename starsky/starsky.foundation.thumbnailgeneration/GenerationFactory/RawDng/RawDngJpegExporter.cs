using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

internal static class RawDngJpegExporter
{
	internal static bool TryWriteDisplayRgbAsJpeg(float[,,] displayRgb, Stream output,
		out string error, int quality = 90)
	{
		error = string.Empty;
		try
		{
			var h = displayRgb.GetLength(0);
			var w = displayRgb.GetLength(1);
			using var image = new Image<Rgb24>(w, h);
			for ( var y = 0; y < h; y++ )
			{
				var row = image.GetPixelRowSpan(y);
				for ( var x = 0; x < w; x++ )
				{
					row[x] = new Rgb24(
						ToByte(displayRgb[y, x, 0]),
						ToByte(displayRgb[y, x, 1]),
						ToByte(displayRgb[y, x, 2]));
				}
			}

			image.Save(output, new JpegEncoder { Quality = quality });
			return true;
		}
		catch ( Exception ex )
		{
			error = ex.Message;
			return false;
		}
	}

	private static byte ToByte(float value)
	{
		var v = float.IsFinite(value) ? value : 0f;
		v = Math.Clamp(v, 0f, 1f);
		return ( byte ) ( v * 255f + 0.5f );
	}
}

