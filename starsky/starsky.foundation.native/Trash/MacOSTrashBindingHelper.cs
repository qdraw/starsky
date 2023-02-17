using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace starsky.foundation.native.Trash
{
	
	public static class NSSearchPath {

		[Flags]
		public enum NSSearchPathDomain : ulong {
			None = 0,
			User = 1 << 0,
			Local = 1 << 1,
			Network = 1 << 2,
			System = 1 << 3,
			All = 0x0ffff,
		}
		
		/// <summary>
		/// @see: https://github.com/xamarin/xamarin-macios/blob/0f39b2579f0cd1a78a9ac9d1af1a12b46584b067/src/Foundation/Enum.cs
		/// see: https://developer.apple.com/documentation/foundation/nssearchpathdomainmask?language=objc
		/// see: https://gist.github.com/vfig/4d82406abeb3b0a0ec78
		/// </summary>
		public enum NSSearchPathDirectory : ulong
		{
			UserDirectory = 7,
			TrashDirectory = 102,
		}

		public static void GetDirectories (NSSearchPathDirectory directory, NSSearchPathDomain domainMask, bool expandTilde = true)
		{
			var result = NSSearchPathForDirectoriesInDomains(( nuint )( ulong )directory,
				( nuint )( ulong )domainMask, expandTilde);

			Console.WriteLine(result);
		}

		[DllImport ( "/System/Library/Frameworks/Foundation.framework/Foundation")]
		static extern IntPtr NSSearchPathForDirectoriesInDomains (nuint directory, nuint domainMask, [MarshalAs (UnmanagedType.I1)] bool expandTilde);
	}
	
	/// <summary>
	/// @see: https://stackoverflow.com/a/44669560
	/// </summary>
    public static class MacOsTrashBindingHelper
    {

	    public static bool? Trash(List<string> filesFullPath, OSPlatform platform)
	    {
		    if ( platform != OSPlatform.OSX )
		    {
			    return null;
		    }
		    
		    TrashInternal(filesFullPath);
		    return true;
	    }


	    internal static void TrashInternal(List<string> filesFullPath)
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

            var urlArray = CreateCfArray(urls.ToArray());

            var nsWorkspace = objc_getClass("NSWorkspace");
            var sharedWorkspace = objc_msgSend_retIntPtr(nsWorkspace, GetSelector("sharedWorkspace"));

            var completionHandler = GetSelector("recycleURLs:completionHandler:") ;
            
            // https://developer.apple.com/documentation/appkit/nsworkspace/1530465-recycle
            objc_msgSend_retVoid_IntPtr_IntPtr(sharedWorkspace, 
	            completionHandler, 
	            urlArray, IntPtr.Zero);
            
            CFRelease(urlArray);
            // CFRelease the fileUrl, sharedWorkspace, nsWorkspace gives a crash (error 139)
        }

        internal static IntPtr GetSelector(string name)
        {
            IntPtr cfstrSelector = CreateCfString(name);
            IntPtr selector = NSSelectorFromString(cfstrSelector);
            CFRelease(cfstrSelector);
            return selector;
        }

        private const string FoundationFramework = "/System/Library/Frameworks/Foundation.framework/Foundation";
        private const string AppKitFramework = "/System/Library/Frameworks/AppKit.framework/AppKit";

        internal unsafe static IntPtr CreateCfString(string aString)
        {
            var bytes = Encoding.Unicode.GetBytes(aString);
            fixed (byte* b = bytes) {
                var cfStr = CFStringCreateWithBytes(IntPtr.Zero, (IntPtr)b, bytes.Length, 
	                CfStringEncoding.UTF16, false);
                return cfStr;
            }
        }

        // warning: this doesn't call retain/release on the elements in the array
        internal unsafe static IntPtr CreateCfArray(IntPtr[] objectes)
        {
            fixed(IntPtr* vals = objectes) {
                 return CFArrayCreate(IntPtr.Zero, (IntPtr)vals, objectes.Length, IntPtr.Zero);
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

        [DllImport(FoundationFramework, EntryPoint="objc_msgSend")]
        private static extern IntPtr objc_msgSend_retIntPtr(IntPtr target, IntPtr selector);

        [DllImport(FoundationFramework, EntryPoint="objc_msgSend")]
        private static extern void objc_msgSend_retVoid_IntPtr_IntPtr(IntPtr target, IntPtr selector, IntPtr param1, IntPtr param2);

        [DllImport(FoundationFramework, EntryPoint="objc_msgSend")]
        private static extern IntPtr objc_msgSend_retIntPtr_IntPtr(IntPtr target, IntPtr selector, IntPtr param);

        public enum CfStringEncoding : uint
        {
            UTF16 = 0x0100,
            UTF16BE = 0x10000100,
            UTF16LE = 0x14000100,
            ASCII = 0x0600
        }
    }
}
