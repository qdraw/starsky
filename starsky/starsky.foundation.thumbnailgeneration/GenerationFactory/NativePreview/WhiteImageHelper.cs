using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview;

public static class WhiteImageHelper
{
	public static bool IsWhiteImage(string path)
	{
		using var image = Image.Load<Rgba32>(path);

		for ( var y = 0; y < image.Height; y++ )
		{
			var pixelRow = image.DangerousGetPixelRowMemory(y).Span;
			for ( var x = 0; x < image.Width; x++ )
			{
				var pixel = pixelRow[x];
				if ( pixel.R != 255 || pixel.G != 255 || pixel.B != 255 )
				{
					return false;
				}
			}
		}

		return true;
	}
}
