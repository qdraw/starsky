using System;
using System.IO;
using SkiaSharp;

namespace starskytest.FakeCreateAn;

/// <summary>
///     Jpeg with an EXIF Orientation tag, so a decoder has to transform the pixels.
///     The red block is in the top left corner, the other three quadrants are blue, so the
///     corner of the block tells which transformation is applied.
/// </summary>
public static class CreateAnImageWithExifOrientation
{
	/// <summary>
	///     1:1 pixel copy, the orientation is only stored in the EXIF tag
	/// </summary>
	public static byte[] Jpeg(int width, int height, int orientation)
	{
		using var bitmap = new SKBitmap(new SKImageInfo(width, height,
			SKColorType.Rgba8888, SKAlphaType.Opaque));
		bitmap.Erase(SKColors.Blue);
		using ( var canvas = new SKCanvas(bitmap) )
		using ( var red = new SKPaint() )
		{
			red.Color = SKColors.Red;
			canvas.DrawRect(0, 0, width / 2f, height / 2f, red);
		}

		using var image = SKImage.FromBitmap(bitmap);
		using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
		using var stream = new MemoryStream();
		data.SaveTo(stream);
		return AddExifOrientation(stream.ToArray(), orientation);
	}

	/// <summary>
	///     Insert an EXIF APP1 segment (with only the Orientation tag) after the Start Of Image
	/// </summary>
	private static byte[] AddExifOrientation(byte[] jpeg, int orientation)
	{
		using var output = new MemoryStream();
		output.Write(jpeg.AsSpan(0, 2)); // SOI
		output.Write(App1Segment(orientation));
		output.Write(jpeg.AsSpan(2));
		return output.ToArray();
	}

	private static byte[] App1Segment(int orientation)
	{
		// TIFF header (big endian) with one IFD0 entry: Orientation (0x0112) of type SHORT
		byte[] tiffHeaderAndValue =
		[
			0x4D, 0x4D, 0x00, 0x2A, // "MM" and 42
			0x00, 0x00, 0x00, 0x08, // offset of IFD0
			0x00, 0x01, // one entry in IFD0
			0x01, 0x12, // tag Orientation
			0x00, 0x03, // type SHORT
			0x00, 0x00, 0x00, 0x01, // one value
			0x00, ( byte ) orientation, 0x00, 0x00, // the value itself
			0x00, 0x00, 0x00, 0x00 // no next IFD
		];

		var exifPrefix = "Exif"u8.ToArray();
		// the length includes the two length bytes, but not the marker itself
		var length = 2 + exifPrefix.Length + 1 + 1 + tiffHeaderAndValue.Length;

		using var segment = new MemoryStream();
		segment.WriteByte(0xFF);
		segment.WriteByte(0xE1);
		segment.WriteByte(( byte ) ( length >> 8 ));
		segment.WriteByte(( byte ) ( length & 0xFF ));
		segment.Write(exifPrefix);
		segment.WriteByte(0x00); // string terminator of "Exif"
		segment.WriteByte(0x00); // padding
		segment.Write(tiffHeaderAndValue);
		return segment.ToArray();
	}
}
