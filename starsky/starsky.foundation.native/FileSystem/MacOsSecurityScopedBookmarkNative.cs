using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using starsky.foundation.native.FileSystem.Interfaces;

namespace starsky.foundation.native.FileSystem;

/// <summary>
///     Concrete P/Invoke wrapper for macOS Foundation framework APIs used in security-scoped
///     bookmarks.
///     Thin wrappers only — excluded from code coverage.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Interoperability",
	"SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to " +
	"generate P/Invoke marshalling code at compile time")]
[SuppressMessage("Usage", "S4200: Make this wrapper for native method less trivial")]
internal sealed class MacOsSecurityScopedBookmarkNative : IMacOsSecurityScopedBookmarkNative
{
	private const string FoundationFramework =
		"/System/Library/Frameworks/Foundation.framework/Foundation";

	private const string AppKitFramework =
		"/System/Library/Frameworks/AppKit.framework/AppKit";

	private const int NsUrlBookmarkCreationWithSecurityScope = 1 << 10;
	private const int NsUrlBookmarkResolutionWithSecurityScope = 1 << 8;

	public IntPtr CreateFileUrl(string path)
	{
		var cfStrPath = CreateCfStringInternal(path);
		var nsUrlClass = objc_getClass("NSURL");
		var result = objc_msgSend_retIntPtr_IntPtr(
			nsUrlClass,
			GetSelectorInternal("fileURLWithPath:"),
			cfStrPath);
		CFRelease(cfStrPath);
		return result;
	}

	public IntPtr CreateBookmarkData(IntPtr fileUrl)
	{
		return objc_msgSend_retIntPtr_Int(
			fileUrl,
			GetSelectorInternal(
				"bookmarkDataWithOptions:includingResourceValuesForKeys:relativeToURL:error:"),
			NsUrlBookmarkCreationWithSecurityScope,
			IntPtr.Zero,
			IntPtr.Zero,
			IntPtr.Zero);
	}

	public IntPtr ResolveBookmarkData(IntPtr nsData)
	{
		var nsUrlClass = objc_getClass("NSURL");
		return objc_msgSend_retIntPtr_Int_IntPtr_IntPtr_IntPtr_IntPtr(
			nsUrlClass,
			GetSelectorInternal(
				"URLByResolvingBookmarkData:options:relativeTo:bookmarkDataIsStale:error:"),
			nsData,
			NsUrlBookmarkResolutionWithSecurityScope,
			IntPtr.Zero,
			IntPtr.Zero,
			IntPtr.Zero);
	}

	public bool StartAccessingSecurityScopedResource(IntPtr url)
	{
		return objc_msgSend_retBool(url,
			GetSelectorInternal("startAccessingSecurityScopedResource"));
	}

	public void StopAccessingSecurityScopedResource(IntPtr url)
	{
		objc_msgSend_retVoid(url, GetSelectorInternal("stopAccessingSecurityScopedResource"));
	}

	public string GetPath(IntPtr url)
	{
		var pathCfStr = objc_msgSend_retIntPtr(url, GetSelectorInternal("path"));
		var result = CfStringToStringInternal(pathCfStr);
		CFRelease(pathCfStr);
		return result;
	}

	public unsafe IntPtr NsDataFromBytes(byte[] bytes)
	{
		fixed ( byte* ptr = bytes )
		{
			var nsDataClass = objc_getClass("NSData");
			return objc_msgSend_retIntPtr_IntPtr_Int(
				nsDataClass,
				GetSelectorInternal("dataWithBytes:length:"),
				( IntPtr ) ptr,
				bytes.Length);
		}
	}

	public byte[] NsDataGetBytes(IntPtr nsData)
	{
		var lengthIntPtr = objc_msgSend_retIntPtr(nsData, GetSelectorInternal("length"));
		var length = ( int ) lengthIntPtr.ToInt64();
		var bytesPtr = objc_msgSend_retIntPtr(nsData, GetSelectorInternal("bytes"));
		var result = new byte[length];
		Marshal.Copy(bytesPtr, result, 0, length);
		return result;
	}

	public void CfRelease(IntPtr handle)
	{
		CFRelease(handle);
	}

	public void ObjcRelease(IntPtr obj)
	{
		objc_msgSend_retVoid(obj, GetSelectorInternal("release"));
	}

	// ========== Private helpers ==========

	private static unsafe IntPtr CreateCfStringInternal(string aString)
	{
		var bytes = Encoding.Unicode.GetBytes(aString);
		fixed ( byte* b = bytes )
		{
			return CFStringCreateWithBytes(IntPtr.Zero, ( IntPtr ) b, bytes.Length,
				CfStringEncoding.UTF16, false);
		}
	}

	private static string CfStringToStringInternal(IntPtr cfStr)
	{
		if ( cfStr == IntPtr.Zero )
		{
			return string.Empty;
		}

		var buf = new StringBuilder(1024);
		var ok = CFStringGetCString(cfStr, buf, buf.Capacity, 0x08000100); // kCFStringEncodingUTF8
		return ok ? buf.ToString() : string.Empty;
	}

	private static IntPtr GetSelectorInternal(string name)
	{
		var cfStr = CreateCfStringInternal(name);
		var sel = NSSelectorFromString(cfStr);
		CFRelease(cfStr);
		return sel;
	}

	// ========== P/Invoke declarations ==========

	[DllImport(FoundationFramework)]
	private static extern IntPtr CFStringCreateWithBytes(IntPtr allocator, IntPtr buffer,
		long bufferLength, CfStringEncoding encoding, bool isExternalRepresentation);

	[DllImport(FoundationFramework)]
	private static extern bool CFStringGetCString(IntPtr theString, StringBuilder buffer,
		long bufferSize, uint encoding);

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
	private static extern void objc_msgSend_retVoid(IntPtr target, IntPtr selector);

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern bool objc_msgSend_retBool(IntPtr target, IntPtr selector);

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr_IntPtr(IntPtr target, IntPtr selector,
		IntPtr param);

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr_Int(IntPtr target, IntPtr selector,
		int param1, IntPtr param2, IntPtr param3, IntPtr param4);

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr_IntPtr_Int(IntPtr target, IntPtr selector,
		IntPtr param1, int param2);

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr_Int_IntPtr_IntPtr_IntPtr_IntPtr(
		IntPtr target, IntPtr selector,
		IntPtr param1, int param2, IntPtr param3, IntPtr param4, IntPtr param5);

	private enum CfStringEncoding : uint
	{
		UTF16 = 0x0100
	}
}
