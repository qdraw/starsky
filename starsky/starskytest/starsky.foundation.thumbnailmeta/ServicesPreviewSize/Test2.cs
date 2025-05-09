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
	// Import the QuickLook framework
	[DllImport("/System/Library/Frameworks/QuickLook.framework/QuickLook",
		EntryPoint = "QLThumbnailImageCreate")]
	public static extern IntPtr QLThumbnailImageCreate(IntPtr alloc, IntPtr url, CGSize size,
		IntPtr options);

	public static void GenerateThumbnail(string filePath)
	{
		IntPtr cfStr = CFStringCreateWithCString(IntPtr.Zero, filePath, CFStringEncoding.kCFStringEncodingUTF8);
		IntPtr url = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfStr, CFURLPathStyle.POSIX, true);
		
		if ( url == IntPtr.Zero )
		{
			Console.WriteLine($"Error: Failed to create URL for {filePath}");
			return;
		}

		// Define the thumbnail size
		var size = new CGSize(600, 800);

		// Create options dictionary for QuickLook (currently empty)
		var options = IntPtr.Zero;

		// Generate the thumbnail (returns a CGImageRef, which is a pointer)
		var thumbnailRef = QLThumbnailImageCreate(IntPtr.Zero, url, size, options);

		if ( thumbnailRef != IntPtr.Zero )
		{
			// Handle the thumbnail (You could save or process the thumbnail here)
			Console.WriteLine("Thumbnail generated successfully!");
		}
		else
		{
			Console.WriteLine("Failed to generate thumbnail.");
		}
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

	public enum CFURLPathStyle : int
	{
		POSIX = 0,
		HFS = 1,
		Windows = 2
	}

	public enum CFStringEncoding : uint
	{
		kCFStringEncodingUTF8 = 0x08000100
	}
	
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
