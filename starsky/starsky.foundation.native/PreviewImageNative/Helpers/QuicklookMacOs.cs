using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class QuicklookMacOs(IWebLogger logger)
{
	public enum CfStringEncoding : uint
	{
		kCFStringEncodingUTF8 = 0x08000100
	}

	// Import the QuickLook framework
	[DllImport("/System/Library/Frameworks/QuickLook.framework/QuickLook",
		EntryPoint = "QLThumbnailImageCreate")]
	private static extern IntPtr QLThumbnailImageCreate(IntPtr alloc, IntPtr url, CGSize size,
		IntPtr options);

	public bool GenerateThumbnail(string filePath, string outputPath, int width, int height)
	{
		var cfStr = CFStringCreateWithCString(IntPtr.Zero, filePath,
			CfStringEncoding.kCFStringEncodingUTF8);
		var url = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfStr, CFURLPathStyle.POSIX, true);

		if ( url == IntPtr.Zero )
		{
			logger.LogInformation("[QuicklookMacOs] Error: Failed to create URL for {filePath}",
				filePath);
			return false;
		}

		// Define the thumbnail size
		var size = new CGSize(width, height);

		// Create options dictionary for QuickLook (currently empty)
		var options = IntPtr.Zero;

		// Generate the thumbnail (returns a CGImageRef, which is a pointer)
		var thumbnailRef = QLThumbnailImageCreate(IntPtr.Zero, url, size, options);

		if ( thumbnailRef != IntPtr.Zero )
		{
			// Handle the thumbnail (You could save or process the thumbnail here)
			return SaveCGImageAsFile(thumbnailRef, outputPath);
		}

		logger.LogInformation("[QuicklookMacOs] Failed to generate thumbnail");
		return false;
	}

	internal bool SaveCGImageAsFile(IntPtr cgImage, string outputPath,
		string uniformTypeIdentifier = "public.jpeg")
	{
		const uint kCFStringEncodingUTF8 = 0x08000100;

		// Create CFString for file path
		var cfStr = CFStringCreateWithCString(IntPtr.Zero, outputPath, kCFStringEncodingUTF8);

		// Create CFURL from path
		var url = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfStr, 0 /* POSIX */, false);

		// Create type identifier for image format
		var imageTypeIntPtr =
			CFStringCreateWithCString(IntPtr.Zero, uniformTypeIdentifier, kCFStringEncodingUTF8);

		// Create image destination
		var destination = CGImageDestinationCreateWithURL(url, imageTypeIntPtr, 1, IntPtr.Zero);

		if ( destination == IntPtr.Zero )
		{
			logger.LogInformation("[QuicklookMacOs] Failed to create image destination");
			return false;
		}

		// Add image and finalize
		CGImageDestinationAddImage(destination, cgImage, IntPtr.Zero);
		if ( !CGImageDestinationFinalize(destination) )
		{
			logger.LogInformation("[QuicklookMacOs] Failed to finalize image");
		}

		// Cleanup
		CFRelease(destination);
		CFRelease(url);
		CFRelease(cfStr);
		CFRelease(imageTypeIntPtr);
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

	[SuppressMessage("Usage",
		"S2342: Enumeration types should comply with a naming convention")]
	internal enum CFURLPathStyle
	{
		POSIX = 0,
		HFS = 1,
		Windows = 2
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
