using System.Runtime.InteropServices;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

public static class WhiteImageDetectorMacOsBindings
{
	// ImageIO
	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageSourceCreateWithURL(IntPtr url, IntPtr options);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageSourceCreateImageAtIndex(IntPtr source, int index,
		IntPtr options);

	// CoreGraphics
	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern IntPtr CGDataProviderCopyData(IntPtr provider);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern IntPtr CGImageGetDataProvider(IntPtr image);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern int CGImageGetWidth(IntPtr image);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern int CGImageGetHeight(IntPtr image);

	public static bool IsImageWhite(string imagePath)
	{
		var url = CoreFoundationMacOsBindings.CreateCFStringCreateWithCString(imagePath);

		var imageSource = CGImageSourceCreateWithURL(url, IntPtr.Zero);
		if ( imageSource == IntPtr.Zero )
		{
			return false;
		}

		var cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, IntPtr.Zero);
		if ( cgImage == IntPtr.Zero )
		{
			return false;
		}

		// Ensure the image is decoded into a standard RGB color space
		var colorSpace = CGColorSpaceCreateDeviceRGB();
		var context = CGBitmapContextCreate(IntPtr.Zero, CGImageGetWidth(cgImage),
			CGImageGetHeight(cgImage), 8, 0, colorSpace, 0x02 /* kCGImageAlphaNoneSkipLast */);
		if ( context == IntPtr.Zero )
		{
			return false;
		}

		CGContextDrawImage(context,
			new CGRect(0, 0, CGImageGetWidth(cgImage), CGImageGetHeight(cgImage)), cgImage);
		var data = CGBitmapContextGetData(context);
		if ( data == IntPtr.Zero )
		{
			return false;
		}

		var width = CGImageGetWidth(cgImage);
		var height = CGImageGetHeight(cgImage);
		var bytesPerRow = CGBitmapContextGetBytesPerRow(context);
		var bytesPerPixel = 4; // RGBA

		var pixelData = new byte[bytesPerRow * height];
		Marshal.Copy(data, pixelData, 0, pixelData.Length);

		for ( var y = 0; y < height; y++ )
		{
			for ( var x = 0; x < width; x++ )
			{
				var offset = y * bytesPerRow + x * bytesPerPixel;
				var r = pixelData[offset];
				var g = pixelData[offset + 1];
				var b = pixelData[offset + 2];

				if ( r != 255 || g != 255 || b != 255 )
				{
					return false; // Found a non-white pixel
				}
			}
		}

		return true;
	}

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern IntPtr CGColorSpaceCreateDeviceRGB();

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern IntPtr CGBitmapContextCreate(IntPtr data, int width, int height,
		int bitsPerComponent, int bytesPerRow, IntPtr colorSpace, uint bitmapInfo);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern void CGContextDrawImage(IntPtr context, CGRect rect, IntPtr image);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern IntPtr CGBitmapContextGetData(IntPtr context);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern int CGBitmapContextGetBytesPerRow(IntPtr context);

	[StructLayout(LayoutKind.Sequential)]
	private struct CGRect
	{
		public double X;
		public double Y;
		public double Width;
		public double Height;

		public CGRect(double x, double y, double width, double height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
	}
}
