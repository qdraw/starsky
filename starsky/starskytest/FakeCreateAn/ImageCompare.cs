using System;
using System.IO;
using SkiaSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;

namespace starskytest.FakeCreateAn;

/// <summary>
///     Compare images between two image providers, because pixels never match exactly
/// </summary>
public static class ImageCompare
{
	public static SKBitmap Decode(byte[] bytes)
	{
		using var stream = new MemoryStream(bytes);
		return SkiaImageHelper.Decode(stream);
	}

	public static bool IsJpeg(byte[] bytes)
	{
		return bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
	}

	/// <summary>
	///     Mean absolute difference per RGB channel (0-255), within the rectangle
	/// </summary>
	public static double MeanAbsoluteDifference(SKBitmap a, SKBitmap b, SKRectI? area = null)
	{
		if ( a.Width != b.Width || a.Height != b.Height )
		{
			throw new ArgumentException("Images should have the same size");
		}

		var rect = area ?? new SKRectI(0, 0, a.Width, a.Height);
		long sum = 0;
		long count = 0;
		for ( var y = rect.Top; y < rect.Bottom; y++ )
		{
			for ( var x = rect.Left; x < rect.Right; x++ )
			{
				var pa = a.GetPixel(x, y);
				var pb = b.GetPixel(x, y);
				sum += Math.Abs(pa.Red - pb.Red) + Math.Abs(pa.Green - pb.Green) +
				       Math.Abs(pa.Blue - pb.Blue);
				count += 3;
			}
		}

		return count == 0 ? 0 : ( double ) sum / count;
	}
}
