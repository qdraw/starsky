using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using starsky.foundation.platform.Enums;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;

[TestClass]
public class SkiaImageHelperTests
{
	/// <summary>
	///     red top left pixel, the rest is blue
	/// </summary>
	private static SKBitmap RedTopLeft(int width, int height)
	{
		var bitmap = new SKBitmap(width, height);
		bitmap.Erase(SKColors.Blue);
		bitmap.SetPixel(0, 0, SKColors.Red);
		return bitmap;
	}

	[TestMethod]
	[DataRow(90, 4, 3, 3, 0)] // top right
	[DataRow(180, 3, 4, 2, 3)] // bottom right
	[DataRow(270, 4, 3, 0, 2)] // bottom left
	[DataRow(-90, 4, 3, 0, 2)]
	[DataRow(0, 3, 4, 0, 0)]
	public void Rotate_MovesTopLeftPixel(int degrees, int expectedWidth, int expectedHeight,
		int x, int y)
	{
		// 3 wide, 4 high
		using var source = RedTopLeft(3, 4);
		using var result = SkiaImageHelper.Rotate(source, degrees);

		Assert.AreEqual(expectedWidth, result.Width);
		Assert.AreEqual(expectedHeight, result.Height);
		Assert.AreEqual(SKColors.Red, result.GetPixel(x, y));
	}

	[TestMethod]
	[DataRow(45)]
	[DataRow(200)]
	[DataRow(360 + 45)]
	public void Rotate_NotMultipleOf90_Throws(int degrees)
	{
		using var source = RedTopLeft(3, 4);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
			SkiaImageHelper.Rotate(source, degrees));
	}

	[TestMethod]
	public void Resize_KeepsAspectRatio_Width()
	{
		using var source = RedTopLeft(200, 100);
		using var result = SkiaImageHelper.Resize(source, 50, 0);
		Assert.AreEqual(50, result.Width);
		Assert.AreEqual(25, result.Height);
	}

	[TestMethod]
	public void Resize_KeepsAspectRatio_Height()
	{
		using var source = RedTopLeft(200, 100);
		using var result = SkiaImageHelper.Resize(source, 0, 50);
		Assert.AreEqual(100, result.Width);
		Assert.AreEqual(50, result.Height);
	}

	[TestMethod]
	public void Resize_BothZero_Throws()
	{
		using var source = RedTopLeft(200, 100);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
			SkiaImageHelper.Resize(source, 0, 0));
	}

	[TestMethod]
	[DataRow(200, 100, 50, 50, 25)] // landscape by width
	[DataRow(100, 200, 50, 25, 50)] // portrait by height
	[DataRow(100, 100, 50, 50, 50)] // square
	public void ResizeLongSide(int width, int height, int size, int expectedWidth,
		int expectedHeight)
	{
		using var source = RedTopLeft(width, height);
		using var result = SkiaImageHelper.ResizeLongSide(source, size);
		Assert.AreEqual(expectedWidth, result.Width);
		Assert.AreEqual(expectedHeight, result.Height);
	}

	[TestMethod]
	public void Crop_Size()
	{
		using var source = RedTopLeft(20, 10);
		using var result = SkiaImageHelper.Crop(source, 5, 2, 10, 4);
		Assert.AreEqual(10, result.Width);
		Assert.AreEqual(4, result.Height);
		Assert.AreEqual(SKColors.Blue, result.GetPixel(0, 0));
	}

	[TestMethod]
	public void DrawOverlay_BottomRight()
	{
		using var source = new SKBitmap(10, 10);
		source.Erase(SKColors.Blue);
		using var overlay = new SKBitmap(2, 2);
		overlay.Erase(SKColors.Red);

		SkiaImageHelper.DrawOverlay(source, overlay, 8, 8);

		Assert.AreEqual(SKColors.Red, source.GetPixel(9, 9));
		Assert.AreEqual(SKColors.Blue, source.GetPixel(7, 7));
	}

	[TestMethod]
	public void DecodeAutoOrient_Corrupt_Throws()
	{
		using var stream = new MemoryStream([1, 2, 3, 4]);
		var exception = Assert.ThrowsExactly<InvalidDataException>(() =>
			SkiaImageHelper.DecodeAutoOrient(stream));
		Assert.StartsWith(SkiaImageHelper.ImageCannotBeLoaded, exception.Message);
	}

	[TestMethod]
	public void DecodeAutoOrient_NonSeekableStream()
	{
		using var stream = new NonSeekableStream(CreateAnSyntheticImage.Jpeg(40, 20));
		using var result = SkiaImageHelper.DecodeAutoOrient(stream);
		Assert.AreEqual(40, result.Width);
		Assert.AreEqual(20, result.Height);
	}

	/// <summary>
	///     The source is a 24x16 image with a red top left quadrant, the other quadrants are
	///     blue. The redX and redY are in the middle of the red quadrant after the orientation
	///     of the (EXIF) image is applied
	/// </summary>
	[TestMethod]
	[DataRow(1, 24, 16, 6, 4)] // normal
	[DataRow(2, 24, 16, 18, 4)] // mirrored horizontal
	[DataRow(3, 24, 16, 18, 12)] // rotated 180 degrees
	[DataRow(4, 24, 16, 6, 12)] // mirrored vertical
	[DataRow(5, 16, 24, 4, 6)] // transposed
	[DataRow(6, 16, 24, 12, 6)] // rotated 90 degrees clockwise
	[DataRow(7, 16, 24, 12, 18)] // transverse
	[DataRow(8, 16, 24, 4, 18)] // rotated 270 degrees clockwise
	public void DecodeAutoOrient_ExifOrientation(int orientation, int expectedWidth,
		int expectedHeight, int redX, int redY)
	{
		using var stream =
			new MemoryStream(CreateAnImageWithExifOrientation.Jpeg(24, 16, orientation));
		using var result = SkiaImageHelper.DecodeAutoOrient(stream);

		Assert.AreEqual(expectedWidth, result.Width, "width");
		Assert.AreEqual(expectedHeight, result.Height, "height");

		AssertIsRed(result.GetPixel(redX, redY), $"{redX},{redY} should be red");
		// the diagonal is always in one of the blue quadrants
		var blueX = expectedWidth - 1 - redX;
		var blueY = expectedHeight - 1 - redY;
		AssertIsBlue(result.GetPixel(blueX, blueY), $"{blueX},{blueY} should be blue");
	}

	[TestMethod]
	public void Decode_AjpegWithExifOrientation_DoesNotTransform()
	{
		using var stream = new MemoryStream(CreateAnImageWithExifOrientation.Jpeg(24, 16, 6));
		using var result = SkiaImageHelper.Decode(stream);

		Assert.AreEqual(24, result.Width);
		Assert.AreEqual(16, result.Height);
		AssertIsRed(result.GetPixel(6, 4), "the red quadrant should not be moved");
	}

	[TestMethod]
	public void Encode_UnknownFormat_Throws()
	{
		using var source = RedTopLeft(20, 10);
		using var output = new MemoryStream();
		var exception = Assert.ThrowsExactly<InvalidOperationException>(() =>
			SkiaImageHelper.Encode(source, ( SKEncodedImageFormat ) 999, 90, output));
		Assert.AreEqual("Encode failed", exception.Message);
	}

	private static void AssertIsRed(SKColor color, string because)
	{
		Assert.IsGreaterThan(color.Blue, color.Red, because);
		Assert.IsGreaterThan(color.Green, color.Red, because);
	}

	private static void AssertIsBlue(SKColor color, string because)
	{
		Assert.IsGreaterThan(color.Red, color.Blue, because);
	}

	[TestMethod]
	[DataRow(ThumbnailImageFormat.jpg, new byte[] { 0xFF, 0xD8 })]
	[DataRow(ThumbnailImageFormat.png, new byte[] { 0x89, 0x50, 0x4E, 0x47 })]
	[DataRow(ThumbnailImageFormat.webp, new byte[] { 0x52, 0x49, 0x46, 0x46 })] // RIFF
	public void Encode_Format(ThumbnailImageFormat format, byte[] magic)
	{
		using var source = RedTopLeft(20, 10);
		using var output = new MemoryStream();
		SkiaImageHelper.Encode(source, format, output);

		var bytes = output.ToArray();
		Assert.AreSequenceEqual(magic, bytes[..magic.Length]);

		using var decoded = SkiaImageHelper.Decode(new MemoryStream(bytes));
		Assert.AreEqual(20, decoded.Width);
		Assert.AreEqual(10, decoded.Height);
	}

	private sealed class NonSeekableStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override bool CanSeek => false;
	}
}
