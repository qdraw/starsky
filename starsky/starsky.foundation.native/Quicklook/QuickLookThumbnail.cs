using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.native.Quicklook;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class QuickLookThumbnail
{
	public enum CfStringEncoding : uint
	{
		kCFStringEncodingUTF8 = 0x08000100
	}

	[SuppressMessage("Usage",
		"S2342: Enumeration types should comply with a naming convention")]
	public enum CFURLPathStyle
	{
		POSIX = 0,
		HFS = 1,
		Windows = 2
	}

	// Import the QuickLook framework
	[DllImport("/System/Library/Frameworks/QuickLook.framework/QuickLook",
		EntryPoint = "QLThumbnailImageCreate")]
	private static extern IntPtr QLThumbnailImageCreate(IntPtr alloc, IntPtr url, CGSize size,
		IntPtr options);

	public static bool GenerateThumbnail(string filePath)
	{
		var cfStr = CFStringCreateWithCString(IntPtr.Zero, filePath,
			CfStringEncoding.kCFStringEncodingUTF8);
		var url = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfStr, CFURLPathStyle.POSIX, true);

		if ( url == IntPtr.Zero )
		{
			Console.WriteLine($"Error: Failed to create URL for {filePath}");
			return false;
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
			return SaveCGImageAsFile(thumbnailRef, "/tmp/thumbnail.webp");
		}

		Console.WriteLine("Failed to generate thumbnail.");
		return false;
	}

	public static bool SaveCGImageAsFile(IntPtr cgImage, string outputPath)
	{
		const uint kCFStringEncodingUTF8 = 0x08000100;

		// Create CFString for file path
		var cfStr = CFStringCreateWithCString(IntPtr.Zero, outputPath, kCFStringEncodingUTF8);

		// Create CFURL from path
		var url = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfStr, 0 /* POSIX */, false);

		// Create type identifier for PNG
		var pngType = CFStringCreateWithCString(IntPtr.Zero, "public.webp", kCFStringEncodingUTF8);

		// Create image destination
		var destination = CGImageDestinationCreateWithURL(url, pngType, 1, IntPtr.Zero);

		if ( destination == IntPtr.Zero )
		{
			Console.WriteLine("Failed to create image destination.");
			return false;
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
		return true;
	}

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFURLCreateWithFileSystemPath(
		IntPtr allocator,
		IntPtr filePath, // CFStringRef
		CFURLPathStyle pathStyle,
		[MarshalAs(UnmanagedType.I1)] bool isDirectory);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFURLCreateWithFileSystemPath(
		IntPtr allocator,
		IntPtr filePath,
		int pathStyle, // 0 = POSIX
		bool isDirectory);


	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageDestinationCreateWithURL(
		IntPtr url,
		IntPtr type,
		int count,
		IntPtr options);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern void CGImageDestinationAddImage(
		IntPtr destination,
		IntPtr image,
		IntPtr properties);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern bool CGImageDestinationFinalize(IntPtr destination);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFStringCreateWithCString(
		IntPtr allocator,
		string cStr,
		CfStringEncoding encoding);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFStringCreateWithCString(IntPtr alloc,
		string str, uint encoding);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRelease(IntPtr cf);

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
