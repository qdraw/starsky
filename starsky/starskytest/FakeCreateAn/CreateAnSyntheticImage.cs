using System.IO;
using SkiaSharp;

namespace starskytest.FakeCreateAn;

/// <summary>
///     Deterministic image with gradients and shapes, large enough to exercise down scaling
/// </summary>
public static class CreateAnSyntheticImage
{
	public static byte[] Jpeg(int width, int height)
	{
		using var bitmap = new SKBitmap(new SKImageInfo(width, height,
			SKColorType.Rgba8888, SKAlphaType.Opaque));
		using ( var canvas = new SKCanvas(bitmap) )
		{
			using var shader = SKShader.CreateLinearGradient(new SKPoint(0, 0),
				new SKPoint(width, height), [SKColors.DarkBlue, SKColors.Orange],
				SKShaderTileMode.Clamp);
			using var gradient = new SKPaint { Shader = shader };
			canvas.DrawRect(0, 0, width, height, gradient);

			using var circle = new SKPaint { Color = SKColors.White, IsAntialias = true };
			canvas.DrawCircle(width / 3f, height / 2f, height / 4f, circle);
			using var box = new SKPaint { Color = SKColors.DarkGreen };
			canvas.DrawRect(width * 0.6f, height * 0.2f, width * 0.25f, height * 0.5f, box);
		}

		using var image = SKImage.FromBitmap(bitmap);
		using var data = image.Encode(SKEncodedImageFormat.Jpeg, 95);
		using var stream = new MemoryStream();
		data.SaveTo(stream);
		return stream.ToArray();
	}
}
