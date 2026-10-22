using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Interoperability", "SYSLIB1054:Use \'LibraryImportAttribute\' " +
                                     "instead of \'DllImportAttribute\' to generate P/Invoke " +
                                     "marshalling code at compile time")]
[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
public static class WhiteImageDetectorMacOsBindings
{
	// ImageIO
	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageSourceCreateWithURL(IntPtr url, IntPtr options);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageSourceCreateImageAtIndex(IntPtr source, int index,
		IntPtr options);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern int CGImageGetWidth(IntPtr image);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern int CGImageGetHeight(IntPtr image);

	public static bool IsImageWhite(string imagePath)
	{
		var url = IntPtr.Zero;
		var imageSource = IntPtr.Zero;
		var cgImage = IntPtr.Zero;
		var colorSpace = IntPtr.Zero;
		var context = IntPtr.Zero;

		try
		{
			url = CoreFoundationMacOsBindings.CreateCFStringCreateWithCString(imagePath);

			imageSource = CGImageSourceCreateWithURL(url, IntPtr.Zero);
			if ( imageSource == IntPtr.Zero )
			{
				return false;
			}

			cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, IntPtr.Zero);
			if ( cgImage == IntPtr.Zero )
			{
				return false;
			}

			// Ensure the image is decoded into a standard RGB color space
			colorSpace = CGColorSpaceCreateDeviceRGB();
			context = CGBitmapContextCreate(IntPtr.Zero, CGImageGetWidth(cgImage),
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

			return IsPixelDataWhite(width, height, bytesPerRow, bytesPerPixel, pixelData);
		}
		finally
		{
			// Every Create/Copy-rule CoreFoundation/CoreGraphics object above is owned by us
			// and must be released, or its backing memory (a full width*height*4 pixel buffer
			// for the bitmap context, plus the decoded CGImage) is leaked permanently.
			if ( context != IntPtr.Zero )
			{
				CGContextRelease(context);
			}

			if ( colorSpace != IntPtr.Zero )
			{
				CGColorSpaceRelease(colorSpace);
			}

			if ( cgImage != IntPtr.Zero )
			{
				CFRelease(cgImage);
			}

			if ( imageSource != IntPtr.Zero )
			{
				CFRelease(imageSource);
			}

			if ( url != IntPtr.Zero )
			{
				CFRelease(url);
			}
		}
	}

	internal static bool IsPixelDataWhite(int width, int height,
		int bytesPerRow, int bytesPerPixel,
		byte[] pixelData)
	{
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
	private static extern void CGColorSpaceRelease(IntPtr colorSpace);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern IntPtr CGBitmapContextCreate(IntPtr data, int width, int height,
		int bitsPerComponent, int bytesPerRow, IntPtr colorSpace, uint bitmapInfo);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern void CGContextRelease(IntPtr context);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRelease(IntPtr cf);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern void CGContextDrawImage(IntPtr context, CGRect rect, IntPtr image);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern IntPtr CGBitmapContextGetData(IntPtr context);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern int CGBitmapContextGetBytesPerRow(IntPtr context);

	[StructLayout(LayoutKind.Sequential)]
	private struct CGRect(double x, double y, double width, double height)
	{
		public double X = x;
		public double Y = y;
		public double Width = width;
		public double Height = height;
	}
}
