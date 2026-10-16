using System;
using System.IO;
using SkiaSharp;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;

/// <summary>
///     Shared SkiaSharp image operations (decode, resize, rotate, overlay, encode),
///     used for all image processing
/// </summary>
public static class SkiaImageHelper
{
	/// <summary>
	///     Callers match on this prefix to log a short error message
	/// </summary>
	public const string ImageCannotBeLoaded = "Image cannot be loaded";

	// 1:1 pixel copies, linear is only used for the sub-pixel case
	private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);

	/// <summary>
	///     Decode and apply the EXIF orientation (AutoOrient)
	/// </summary>
	/// <exception cref="InvalidDataException">when the stream is not a supported image</exception>
	public static SKBitmap DecodeAutoOrient(Stream stream)
	{
		using var codec = CreateCodec(stream);
		var origin = codec.EncodedOrigin;
		var info = new SKImageInfo(codec.Info.Width, codec.Info.Height,
			SKImageInfo.PlatformColorType, SKAlphaType.Premul);
		var bitmap = SKBitmap.Decode(codec, info) ??
		             throw new InvalidDataException(ImageCannotBeLoaded);
		if ( origin == SKEncodedOrigin.TopLeft )
		{
			return bitmap;
		}

		using ( bitmap )
		{
			return ApplyOrigin(bitmap, origin);
		}
	}

	/// <summary>
	///     Decode without applying any orientation
	/// </summary>
	public static SKBitmap Decode(Stream stream)
	{
		using var codec = CreateCodec(stream);
		var info = new SKImageInfo(codec.Info.Width, codec.Info.Height,
			SKImageInfo.PlatformColorType, SKAlphaType.Premul);
		return SKBitmap.Decode(codec, info) ??
		       throw new InvalidDataException(ImageCannotBeLoaded);
	}

	private static SKCodec CreateCodec(Stream stream)
	{
		var source = stream;
		if ( !stream.CanSeek )
		{
			source = new MemoryStream();
			stream.CopyTo(source);
			source.Seek(0, SeekOrigin.Begin);
		}

		// SKData copies the bytes, so the stream is not needed after this call
		using var data = SKData.Create(source);
		return SKCodec.Create(data) ?? throw new InvalidDataException(ImageCannotBeLoaded);
	}

	private static SKBitmap ApplyOrigin(SKBitmap source, SKEncodedOrigin origin)
	{
		float w = source.Width;
		float h = source.Height;
		var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
			or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

		// x' = ScaleX * x + SkewX * y + TransX ; y' = SkewY * x + ScaleY * y + TransY
		var matrix = SKMatrix.CreateIdentity();
		switch ( origin )
		{
			case SKEncodedOrigin.TopRight:
				matrix.ScaleX = -1;
				matrix.TransX = w;
				break;
			case SKEncodedOrigin.BottomRight:
				matrix.ScaleX = -1;
				matrix.TransX = w;
				matrix.ScaleY = -1;
				matrix.TransY = h;
				break;
			case SKEncodedOrigin.BottomLeft:
				matrix.ScaleY = -1;
				matrix.TransY = h;
				break;
			case SKEncodedOrigin.LeftTop:
				matrix.ScaleX = 0;
				matrix.ScaleY = 0;
				matrix.SkewX = 1;
				matrix.SkewY = 1;
				break;
			case SKEncodedOrigin.RightTop:
				matrix.ScaleX = 0;
				matrix.ScaleY = 0;
				matrix.SkewX = -1;
				matrix.TransX = h;
				matrix.SkewY = 1;
				break;
			case SKEncodedOrigin.RightBottom:
				matrix.ScaleX = 0;
				matrix.ScaleY = 0;
				matrix.SkewX = -1;
				matrix.TransX = h;
				matrix.SkewY = -1;
				matrix.TransY = w;
				break;
			case SKEncodedOrigin.LeftBottom:
				matrix.ScaleX = 0;
				matrix.ScaleY = 0;
				matrix.SkewX = 1;
				matrix.SkewY = -1;
				matrix.TransY = w;
				break;
		}

		var info = source.Info.WithSize(swap ? source.Height : source.Width,
			swap ? source.Width : source.Height);
		var result = new SKBitmap(info);
		using var image = SKImage.FromBitmap(source);
		using var canvas = new SKCanvas(result);
		canvas.SetMatrix(matrix);
		canvas.DrawImage(image, 0, 0, Sampling);
		return result;
	}

	/// <summary>
	///     Resize, when one of the sides is 0 the aspect ratio is kept
	/// </summary>
	/// <param name="source">input, is not disposed</param>
	/// <param name="width">pixels or 0</param>
	/// <param name="height">pixels or 0</param>
	public static SKBitmap Resize(SKBitmap source, int width, int height)
	{
		if ( width <= 0 && height <= 0 )
		{
			throw new ArgumentOutOfRangeException(nameof(width),
				"width or height should be larger than 0");
		}

		if ( width <= 0 )
		{
			width = Math.Max(1, ( int ) Math.Round(source.Width * ( double ) height / source.Height));
		}
		else if ( height <= 0 )
		{
			height = Math.Max(1, ( int ) Math.Round(source.Height * ( double ) width / source.Width));
		}

		// Mipmaps give an area-aware result when scaling down
		var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
		return source.Resize(source.Info.WithSize(width, height), sampling) ??
		       throw new InvalidOperationException("Resize failed");
	}

	/// <summary>
	///     Portrait and square images are scaled by height, landscape by width
	/// </summary>
	public static SKBitmap ResizeLongSide(SKBitmap source, int size)
	{
		return source.Height >= source.Width
			? Resize(source, 0, size)
			: Resize(source, size, 0);
	}

	public static SKBitmap Crop(SKBitmap source, int x, int y, int width, int height)
	{
		var result = new SKBitmap(source.Info.WithSize(width, height));
		using var image = SKImage.FromBitmap(source);
		using var canvas = new SKCanvas(result);
		canvas.DrawImage(image, -x, -y, Sampling);
		return result;
	}

	/// <summary>
	///     Rotate in steps of 90 degrees (clockwise)
	/// </summary>
	public static SKBitmap Rotate(SKBitmap source, double degrees)
	{
		var normalized = ( ( int ) Math.Round(degrees) % 360 + 360 ) % 360;
		var origin = normalized switch
		{
			0 => SKEncodedOrigin.TopLeft,
			90 => SKEncodedOrigin.RightTop,
			180 => SKEncodedOrigin.BottomRight,
			270 => SKEncodedOrigin.LeftBottom,
			_ => throw new ArgumentOutOfRangeException(nameof(degrees),
				"Only multiples of 90 are supported")
		};

		return origin == SKEncodedOrigin.TopLeft
			? source.Copy()
			: ApplyOrigin(source, origin);
	}

	/// <summary>
	///     Draw the overlay (e.g. logo) on top of the source (mutates source)
	/// </summary>
	public static void DrawOverlay(SKBitmap source, SKBitmap overlay, int x, int y)
	{
		using var image = SKImage.FromBitmap(overlay);
		using var canvas = new SKCanvas(source);
		canvas.DrawImage(image, x, y, Sampling);
	}

	public static void Encode(SKBitmap bitmap, ThumbnailImageFormat imageFormat, Stream output)
	{
		switch ( imageFormat )
		{
			case ThumbnailImageFormat.png:
				Encode(bitmap, SKEncodedImageFormat.Png, 100, output);
				return;
			case ThumbnailImageFormat.webp:
				Encode(bitmap, SKEncodedImageFormat.Webp, 80, output);
				return;
			default:
				Encode(bitmap, SKEncodedImageFormat.Jpeg, 90, output);
				break;
		}
	}

	public static void Encode(SKBitmap bitmap, SKEncodedImageFormat format, int quality,
		Stream output)
	{
		using var image = SKImage.FromBitmap(bitmap);
		using var data = image.Encode(format, quality) ??
		                 throw new InvalidOperationException("Encode failed");
		data.SaveTo(output);
	}
}
