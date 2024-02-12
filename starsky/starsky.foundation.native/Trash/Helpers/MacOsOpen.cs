using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace starsky.foundation.native.Trash.Helpers;

public class MacOsOpen
{
	private const string AppKitFramework = "/System/Library/Frameworks/AppKit.framework/AppKit";

	private const string FoundationFramework =
		"/System/Library/Frameworks/Foundation.framework/Foundation";


	private enum CFStringEncoding : uint
	{
		UTF16 = 0x0100,
		UTF16BE = 0x10000100,
		UTF16LE = 0x14000100,
		ASCII = 0x0600
	}

	/// <summary>
	/// Native methods we can call on the Mac.
	/// </summary>
	private static class NativeMethods
	{
		private const string FoundationFramework =
			"/System/Library/Frameworks/Foundation.framework/Foundation";

		private const string AppKitFramework = "/System/Library/Frameworks/AppKit.framework/AppKit";

		[DllImport(AppKitFramework, CharSet = CharSet.Ansi)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
			"CA2101:Specify marshaling for P/Invoke string arguments",
			Justification = "objc_getClass method requires CharSet.Ansi to work.")]
		public static extern IntPtr objc_getClass(string name);

		[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
		public static extern IntPtr objc_msgSend_retIntPtr(IntPtr target, IntPtr selector);

		[DllImport(AppKitFramework)]
		public static extern IntPtr NSSelectorFromString(IntPtr cfstr);

		[DllImport(FoundationFramework)]
		public static extern void CFRelease(IntPtr handle);

		[DllImport(FoundationFramework)]
		public static extern IntPtr CFStringCreateWithBytes(IntPtr allocator, IntPtr buffer,
			long bufferLength, CFStringEncoding encoding, bool isExternalRepresentation);
	}

	[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
	[DllImport(AppKitFramework, CharSet = CharSet.Ansi)]
	private static extern IntPtr objc_getClass(string name);

	[DllImport(FoundationFramework)]
	private static extern IntPtr CFStringCreateWithBytes(IntPtr allocator, IntPtr buffer,
		long bufferLength, MacOsTrashBindingHelper.CfStringEncoding encoding,
		bool isExternalRepresentation);

	[SuppressMessage("Usage", "S6640: Make sure that using \"unsafe\" is safe here")]
	internal static unsafe IntPtr CreateCfString(string aString)
	{
		var bytes = Encoding.Unicode.GetBytes(aString);
		fixed ( byte* b = bytes )
		{
			var cfStr = CFStringCreateWithBytes(IntPtr.Zero, ( IntPtr )b, bytes.Length,
				MacOsTrashBindingHelper.CfStringEncoding.UTF16, false);
			return cfStr;
		}
	}

	// let url = NSURL(fileURLWithPath: "/System/Applications/Utilities/Terminal.app", isDirectory: true) as URL
	//
	// let path = "/bin"
	// let configuration = NSWorkspace.OpenConfiguration()
	// configuration.arguments = [path]
	// NSWorkspace.shared.openApplication(at: url,
	// configuration: configuration,
	// completionHandler: nil)
	// NSWorkspace.shared.openFile

	private static IntPtr GetSelector(string name)
	{
		IntPtr cfstrSelector = CreateCFString(name);
		IntPtr selector = NativeMethods.NSSelectorFromString(cfstrSelector);
		NativeMethods.CFRelease(cfstrSelector);
		return selector;
	}
	
	private static unsafe IntPtr CreateCFString(string aString)
	{
		var bytes = Encoding.Unicode.GetBytes(aString);
		fixed (byte* b = bytes)
		{
			var cfStr = NativeMethods.CFStringCreateWithBytes(IntPtr.Zero, (IntPtr)b, bytes.Length, CFStringEncoding.UTF16, false);
			return cfStr;
		}
	}
	
	public static void Open()
	{
		var nsWorkspace = objc_getClass("NSWorkspace");
		var sharedWorkspace =
			NativeMethods.objc_msgSend_retIntPtr(nsWorkspace, GetSelector("sharedWorkspace"));

		Console.WriteLine();
	}

	// static void Main(string[] args)
	// {
	// 	var cfStrTestFile = CreateCfString("/System/Applications/Utilities/Terminal.app");
	// 	var nsUrl = objc_getClass("NSURL");
	// 	var fileUrl = objc_msgSend_retIntPtr_IntPtr(nsUrl, GetSelector("fileURLWithPath:"), cfStrTestFile);
	// 	CFRelease(cfStrTestFile);
	// 	
	// 	
	// 	var url = NSUrl.FromFilename("/System/Applications/Utilities/Terminal.app");
	// 	var path = "/bin";
	//
	// 	var configuration = new NSWorkspace.OpenConfiguration();
	// 	configuration.Arguments = new[] { path };
	//
	// 	NSWorkspace.SharedWorkspace.OpenApplication(url,
	// 		configuration,
	// 		(success, error) =>
	// 		{
	// 			if (error != null)
	// 			{
	// 				Console.WriteLine($"Error: {error.LocalizedDescription}");
	// 			}
	// 			else if (!success)
	// 			{
	// 				Console.WriteLine("Failed to open application");
	// 			}
	// 			else
	// 			{
	// 				Console.WriteLine("Application opened successfully");
	// 			}
	// 		});
}
