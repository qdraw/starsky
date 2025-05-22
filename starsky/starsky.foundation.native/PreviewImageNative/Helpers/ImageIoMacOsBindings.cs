using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Interoperability", "SYSLIB1054:Use \'LibraryImportAttribute\' " +
                                     "instead of \'DllImportAttribute\' to generate P/Invoke " +
                                     "marshalling code at compile time")]
[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
public static class ImageIoMacOsBindings
{
	internal static int GetSourceHeight(IntPtr cFStringUrl)
	{
		var imageSource = CGImageSourceCreateWithURL(cFStringUrl, IntPtr.Zero);
		var cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, IntPtr.Zero);
		return CGImageGetHeight(cgImage);
	}

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern int CGImageGetHeight(IntPtr image);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageSourceCreateImageAtIndex(IntPtr source, int index,
		IntPtr options);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageSourceCreateWithURL(IntPtr url, IntPtr options);
}
