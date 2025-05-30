using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Interoperability", "SYSLIB1054:Use \'LibraryImportAttribute\' " +
                                     "instead of \'DllImportAttribute\' to generate P/Invoke " +
                                     "marshalling code at compile time")]
[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
public class QuicklookMacOs(IWebLogger logger)
{
	/// <summary>
	///     Generate Images on Native macOS
	/// </summary>
	/// <param name="filePath">input path</param>
	/// <param name="outputPath">output jpg</param>
	/// <param name="width">should contain value</param>
	/// <param name="height">optional value</param>
	/// <returns>true if successful</returns>
	public bool GenerateThumbnail(string filePath, string outputPath, int width, int height)
	{
		filePath = filePath.Replace("//", "/");

		var url = CoreFoundationMacOsBindings.CreateCFStringCreateWithCString(filePath);
		if ( url == IntPtr.Zero )
		{
			logger.LogInformation("[QuicklookMacOs] Error: Failed to create URL for {filePath}",
				filePath);
			return false;
		}

		if ( height <= 0 )
		{
			var sourceHeight = ImageIoMacOsBindings.GetSourceHeight(url);
			height = ( int ) Math.Round(( double ) sourceHeight / width * width);
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

		logger.LogInformation("[QuicklookMacOs] Failed to generate thumbnail" +
		                      $" for F: {filePath} O: {outputPath}");
		return false;
	}

	// Import the QuickLook framework
	[DllImport("/System/Library/Frameworks/QuickLook.framework/QuickLook",
		EntryPoint = "QLThumbnailImageCreate")]
	private static extern IntPtr QLThumbnailImageCreate(IntPtr alloc, IntPtr url, CGSize size,
		IntPtr options);


	internal bool SaveCGImageAsFile(IntPtr cgImage, string outputPath,
		string uniformTypeIdentifier = "public.jpeg")
	{
		const uint kCFStringEncodingUTF8 = 0x08000100;

		// Create CFString for file path
		var cfStr = CFStringCreateWithCString(IntPtr.Zero, outputPath, kCFStringEncodingUTF8);

		// Create CFURL from path
		var url = CFURLCreateWithFileSystemPath(IntPtr.Zero, cfStr,
			0 /* POSIX */, false);

		// Create type identifier for image format
		var imageTypeIntPtr =
			CFStringCreateWithCString(IntPtr.Zero, uniformTypeIdentifier, kCFStringEncodingUTF8);

		// Create image destination
		var destination = CGImageDestinationCreateWithURL(url, imageTypeIntPtr, 1, IntPtr.Zero);

		if ( destination == IntPtr.Zero )
		{
			logger.LogInformation("[QuicklookMacOs] Failed to create image destination"
			                      + $" for F: {outputPath} U: {uniformTypeIdentifier}");
			return false;
		}

		// Add image and finalize
		ImageDestinationAddImageFinalize(destination, cgImage, outputPath);

		// Cleanup
		CFRelease(destination);
		CFRelease(url);
		CFRelease(cfStr);
		CFRelease(imageTypeIntPtr);

		if ( !WhiteImageDetectorMacOsBindings.IsImageWhite(outputPath) )
		{
			return true;
		}

		// MacOS has a bug where it sometimes creates a white image
		File.Delete(outputPath);
		return false;
	}

	internal void ImageDestinationAddImageFinalize(IntPtr destination, IntPtr cgImage,
		string? reference = "")
	{
		CGImageDestinationAddImage(destination, cgImage, IntPtr.Zero);
		if ( !CGImageDestinationFinalize(destination) )
		{
			logger.LogInformation("[QuicklookMacOs] Failed to finalize image" +
			                      $" destination for R: {reference}");
		}
	}

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFStringCreateWithCString(IntPtr alloc,
		string str, uint encoding);

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
	private static extern void CFRelease(IntPtr cf);


	// Define a structure for CGSize (used for image size)
	[StructLayout(LayoutKind.Sequential)]
	internal struct CGSize(double width, double height)
	{
		public double Width = width;
		public double Height = height;
	}
}
