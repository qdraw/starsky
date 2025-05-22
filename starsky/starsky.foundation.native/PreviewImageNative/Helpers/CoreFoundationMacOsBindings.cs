using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Interoperability", "SYSLIB1054:Use \'LibraryImportAttribute\' " +
                                     "instead of \'DllImportAttribute\' to generate P/Invoke " +
                                     "marshalling code at compile time")]
[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
public static class CoreFoundationMacOsBindings
{
	[SuppressMessage("Usage",
		"S2342: Enumeration types should comply with a naming convention")]
	public enum CFURLPathStyle
	{
		POSIX = 0,
		HFS = 1,
		Windows = 2
	}

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFStringCreateWithCString(
		IntPtr allocator,
		string cStr,
		CfStringEncoding encoding);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFURLCreateWithFileSystemPath(
		IntPtr allocator,
		IntPtr filePath, // CFStringRef
		CFURLPathStyle pathStyle,
		[MarshalAs(UnmanagedType.I1)] bool isDirectory);

	internal static IntPtr CreateCFStringCreateWithCString(string filePath)
	{
		try
		{
			var cfStr = CFStringCreateWithCString(IntPtr.Zero, filePath,
				CfStringEncoding.kCFStringEncodingUTF8);
			return CFURLCreateWithFileSystemPath(IntPtr.Zero,
				cfStr, CFURLPathStyle.POSIX, false);
		}
		catch ( DllNotFoundException )
		{
			return IntPtr.Zero;
		}
	}

	internal enum CfStringEncoding : uint
	{
		kCFStringEncodingUTF8 = 0x08000100
	}
}
