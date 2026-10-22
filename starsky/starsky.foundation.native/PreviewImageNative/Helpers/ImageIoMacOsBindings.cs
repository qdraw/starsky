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
		var imageSource = IntPtr.Zero;
		var cgImage = IntPtr.Zero;
		try
		{
			imageSource = CGImageSourceCreateWithURL(cFStringUrl, IntPtr.Zero);
			cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, IntPtr.Zero);
			return CGImageGetHeight(cgImage);
		}
		finally
		{
			// Both objects follow the Create Rule: the caller owns them and must release
			// them, otherwise the decoded full-resolution CGImage leaks on every call.
			if ( cgImage != IntPtr.Zero )
			{
				CFRelease(cgImage);
			}

			if ( imageSource != IntPtr.Zero )
			{
				CFRelease(imageSource);
			}
		}
	}

	[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	private static extern int CGImageGetHeight(IntPtr image);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageSourceCreateImageAtIndex(IntPtr source, int index,
		IntPtr options);

	[DllImport("/System/Library/Frameworks/ImageIO.framework/ImageIO")]
	private static extern IntPtr CGImageSourceCreateWithURL(IntPtr url, IntPtr options);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRelease(IntPtr cf);
}
