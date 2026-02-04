using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.native.Trash.Helpers
{

	/// <summary>
	/// Trash the file on Mac OS
	/// There is NO check if the file exists
	/// 
	/// @see: https://stackoverflow.com/a/44669560
	/// </summary>
	[SuppressMessage("Interoperability", "SYSLIB1054:Use \'LibraryImportAttribute\' instead of \'DllImportAttribute\' " +
										 "to generate P/Invoke marshalling code at compile time")]
	public static class MacOsTrashBindingHelper
	{
		/// <summary>
		/// Trash endpoint
		/// </summary>
		/// <param name="fullPath"></param>
		/// <param name="platform"></param>
		/// <returns></returns>
		internal static bool? Trash(string fullPath,
			OSPlatform platform)
		{
			return Trash(new List<string> { fullPath }, platform);
		}

		/// <summary>
		/// Trash endpoint
		/// </summary>
		/// <param name="filesFullPath">list of paths</param>
		/// <param name="platform">current os</param>
		/// <returns>operation succeed</returns>
		internal static bool? Trash(List<string> filesFullPath, OSPlatform platform)
		{
			if ( platform != OSPlatform.OSX )
			{
				return null;
			}

			TrashInternal(filesFullPath);
			return true;
		}

		internal static IntPtr[] GetUrls(List<string> filesFullPath)
		{
			var urls = new List<IntPtr>();
			foreach ( var filePath in filesFullPath )
			{
				var cfStrTestFile = CreateCfString(filePath);
				var nsUrl = objc_getClass("NSURL");
				var fileUrl = objc_msgSend_retIntPtr_IntPtr(nsUrl, GetSelector("fileURLWithPath:"), cfStrTestFile);
				CFRelease(cfStrTestFile);
				urls.Add(fileUrl);
			}
			return urls.ToArray();
		}

		internal static void TrashInternal(List<string> filesFullPath)
		{
			var urls = GetUrls(filesFullPath);

			var urlArray = CreateCfArray(urls);

			var nsWorkspace = objc_getClass("NSWorkspace");
			var sharedWorkspace = objc_msgSend_retIntPtr(nsWorkspace, GetSelector("sharedWorkspace"));
			var completionHandler = GetSelector("recycleURLs:completionHandler:");

			// https://developer.apple.com/documentation/appkit/nsworkspace/1530465-recycle
			objc_msgSend_retVoid_IntPtr_IntPtr(sharedWorkspace,
				completionHandler,
				urlArray, IntPtr.Zero);

			CFRelease(urlArray);
			// CFRelease the fileUrl, sharedWorkspace, nsWorkspace gives a crash (error 139)
		}

		/// <summary>
		/// Get Selector in the Objective-C runtime
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>Object</returns>
		internal static IntPtr GetSelector(string name)
		{
			var cfStrSelector = CreateCfString(name);
			var selector = NSSelectorFromString(cfStrSelector);
			CFRelease(cfStrSelector);
			return selector;
		}

		private const string FoundationFramework = "/System/Library/Frameworks/Foundation.framework/Foundation";
		private const string AppKitFramework = "/System/Library/Frameworks/AppKit.framework/AppKit";

		[SuppressMessage("Usage", "S6640: Make sure that using \"unsafe\" is safe here")]
		internal static unsafe IntPtr CreateCfString(string aString)
		{
			var bytes = Encoding.Unicode.GetBytes(aString);
			fixed ( byte* b = bytes )
			{
				var cfStr = CFStringCreateWithBytes(IntPtr.Zero, ( IntPtr ) b, bytes.Length,
					CfStringEncoding.UTF16, false);
				return cfStr;
			}
		}

		[SuppressMessage("Usage", "S6640: Make sure that using \"unsafe\" is safe here")]

		internal static unsafe IntPtr CreateCfArray(IntPtr[] objects)
		{
			// warning: this doesn't call retain/release on the elements in the array
			fixed ( IntPtr* values = objects )
			{
				return CFArrayCreate(IntPtr.Zero, ( IntPtr ) values, objects.Length, IntPtr.Zero);
			}
		}

		[DllImport(FoundationFramework)]
		private static extern IntPtr CFStringCreateWithBytes(IntPtr allocator, IntPtr buffer,
			long bufferLength, CfStringEncoding encoding, bool isExternalRepresentation);

		[DllImport(FoundationFramework)]
		private static extern IntPtr CFArrayCreate(IntPtr allocator, IntPtr values, long numValues, IntPtr callbackStruct);

		[DllImport(FoundationFramework)]
		private static extern void CFRelease(IntPtr handle);

		[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
		[DllImport(AppKitFramework, CharSet = CharSet.Ansi)]
		private static extern IntPtr objc_getClass(string name);

		[DllImport(AppKitFramework)]
		private static extern IntPtr NSSelectorFromString(IntPtr cfstr);

		[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_retIntPtr(IntPtr target, IntPtr selector);

		[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend_retVoid_IntPtr_IntPtr(IntPtr target, IntPtr selector, IntPtr param1, IntPtr param2);

		[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
		private static extern IntPtr objc_msgSend_retIntPtr_IntPtr(IntPtr target, IntPtr selector, IntPtr param);

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public enum CfStringEncoding : uint
		{
			UTF16 = 0x0100,
			UTF16BE = 0x10000100,
			UTF16LE = 0x14000100,
			ASCII = 0x0600
		}
	}
}
