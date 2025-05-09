using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace starskytest.starsky.foundation.thumbnailmeta.ServicesPreviewSize;

[TestClass]
public class Test2
{
	[TestMethod]
	[Timeout(1000)] // Timeout set to avoid the endless loop
	public void TestThumbnailSize()
	{
		const string filePath =
			"/Users/dion/data/fotobieb/2024/11/2024_11_11_d glow eindhoven/20241111_185241_DSC00914.jpg"; // Update to your file path
		QuickLookThumbnail.GenerateThumbnail(filePath);
	}
}

public class QuickLookThumbnail
{
	public enum CFStringEncoding : uint
	{
		kCFStringEncodingUTF8 = 0x08000100
	}

	public enum CFURLPathStyle
	{
		POSIX = 0,
		HFS = 1,
		Windows = 2
	}

	// Import the QuickLook framework
	[DllImport("/System/Library/Frameworks/QuickLook.framework/QuickLook",
		EntryPoint = "QLThumbnailImageCreate")]
	public static extern IntPtr QLThumbnailImageCreate(IntPtr alloc, IntPtr url, CGSize size,
		IntPtr options);

	public static void GenerateThumbnail(string filePath)
	{
		var cfStr = CFStringCreateWithCString(IntPtr.Zero, filePath,
			CFStringEncoding.kCFStringEncodingUTF8);
		var url = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfStr, CFURLPathStyle.POSIX, true);

		if ( url == IntPtr.Zero )
		{
			Console.WriteLine($"Error: Failed to create URL for {filePath}");
			return;
		}

		// Define the thumbnail size
		var size = new CGSize(1024, 2048);

		// Create options dictionary for QuickLook (currently empty)
		var options = IntPtr.Zero;

		// Generate the thumbnail (returns a CGImageRef, which is a pointer)
		var thumbnailRef = QLThumbnailImageCreate(IntPtr.Zero, url, size, options);

		if ( thumbnailRef != IntPtr.Zero )
		{
			// Handle the thumbnail (You could save or process the thumbnail here)
			Console.WriteLine("Thumbnail generated successfully!");
			SaveCGImageAsPng(thumbnailRef, "/tmp/thumbnail.jpg");
		}
		else
		{
			Console.WriteLine("Failed to generate thumbnail.");
		}
	}

	public static void SaveCGImageAsPng(IntPtr cgImage, string outputPath)
	{
		const uint kCFStringEncodingUTF8 = 0x08000100;

		// Create CFString for file path
		var cfStr = CFStringCreateWithCString(IntPtr.Zero, outputPath, kCFStringEncodingUTF8);

		// Create CFURL from path
		var url = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfStr, 0 /* POSIX */, false);

		// Create type identifier for PNG
		var pngType = CFStringCreateWithCString(IntPtr.Zero, "public.jpeg", kCFStringEncodingUTF8);

		// Create image destination
		var destination = CGImageDestinationCreateWithURL(url, pngType, 1, IntPtr.Zero);

		if ( destination == IntPtr.Zero )
		{
			Console.WriteLine("Failed to create image destination.");
			return;
		}

		// Add image and finalize
		CGImageDestinationAddImage(destination, cgImage, IntPtr.Zero);
		if ( !CGImageDestinationFinalize(destination) )
		{
			Console.WriteLine("Failed to finalize image.");
		}
		else
		{
			Console.WriteLine($"Image written to {outputPath}");
		}

		// Cleanup
		CFRelease(destination);
		CFRelease(url);
		CFRelease(cfStr);
		CFRelease(pngType);
	}

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	public static extern IntPtr CFURLCreateWithFileSystemPath(
		IntPtr allocator,
		IntPtr filePath, // CFStringRef
		CFURLPathStyle pathStyle,
		[MarshalAs(UnmanagedType.I1)] bool isDirectory);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	public static extern IntPtr CFStringCreateWithCString(
		IntPtr allocator,
		string cStr,
		CFStringEncoding encoding);

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	public static extern IntPtr CGDataProviderCreateWithCFData(IntPtr data);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	public static extern IntPtr CGImageDestinationCreateWithURL(
		IntPtr url,
		IntPtr type,
		int count,
		IntPtr options);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	public static extern void CGImageDestinationAddImage(
		IntPtr destination,
		IntPtr image,
		IntPtr properties);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	public static extern bool CGImageDestinationFinalize(IntPtr destination);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	public static extern IntPtr CFStringCreateWithCString(IntPtr alloc, string str, uint encoding);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	public static extern void CFRelease(IntPtr cf);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	public static extern IntPtr CFURLCreateWithFileSystemPath(
		IntPtr allocator,
		IntPtr filePath,
		int pathStyle, // 0 = POSIX
		bool isDirectory);

	// Define a structure for CGSize (used for image size)
	[StructLayout(LayoutKind.Sequential)]
	public struct CGSize
	{
		public double Width;
		public double Height;

		public CGSize(double width, double height)
		{
			Width = width;
			Height = height;
		}
	}
}
