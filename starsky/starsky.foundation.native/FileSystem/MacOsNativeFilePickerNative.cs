using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using starsky.foundation.native.FileSystem.Interfaces;

namespace starsky.foundation.native.FileSystem;

[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Interoperability",
	"SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to " +
	"generate P/Invoke marshalling code at compile time")]
internal sealed class MacOsNativeFilePickerNative : IMacOsNativeFilePickerNative
{
	private const string FoundationFramework =
		"/System/Library/Frameworks/Foundation.framework/Foundation";
	private const string ObjcFramework = "/usr/lib/libobjc.A.dylib";

	private const nint NsModalResponseOk = 1;
	private const nuint NsUrlBookmarkCreationWithSecurityScope = 1u << 10;

	public IntPtr CreateOpenPanel()
	{
		var panelClass = objc_getClass("NSOpenPanel");
		return objc_msgSend_retIntPtr(panelClass, GetSelectorInternal("openPanel"));
	}

	public void ConfigureOpenPanel(IntPtr panel, bool includeFiles = false)
	{
		objc_msgSend_retVoid_Bool(panel, GetSelectorInternal("setCanChooseFiles:"), includeFiles);
		objc_msgSend_retVoid_Bool(panel, GetSelectorInternal("setCanChooseDirectories:"), true);
		objc_msgSend_retVoid_Bool(panel, GetSelectorInternal("setAllowsMultipleSelection:"), false);
	}

	public bool RunModal(IntPtr panel)
	{
		var result = objc_msgSend_retNInt(panel, GetSelectorInternal("runModal"));
		return result == NsModalResponseOk;
	}

	public IntPtr GetSelectedUrl(IntPtr panel)
	{
		return objc_msgSend_retIntPtr(panel, GetSelectorInternal("URL"));
	}

	public string GetPath(IntPtr url)
	{
		var pathCfStr = objc_msgSend_retIntPtr(url, GetSelectorInternal("path"));
		return CfStringToStringInternal(pathCfStr);
	}

	public IntPtr CreateBookmarkData(IntPtr url)
	{
		return objc_msgSend_retIntPtr_NUInt_IntPtr_IntPtr_IntPtr(
			url,
			GetSelectorInternal(
				"bookmarkDataWithOptions:includingResourceValuesForKeys:relativeToURL:error:"),
			NsUrlBookmarkCreationWithSecurityScope,
			IntPtr.Zero,
			IntPtr.Zero,
			IntPtr.Zero);
	}

	public byte[] NsDataGetBytes(IntPtr nsData)
	{
		var lengthIntPtr = objc_msgSend_retIntPtr(nsData, GetSelectorInternal("length"));
		var length = ( int ) lengthIntPtr.ToInt64();
		if ( length <= 0 )
		{
			return [];
		}

		var bytesPtr = objc_msgSend_retIntPtr(nsData, GetSelectorInternal("bytes"));
		var result = new byte[length];
		Marshal.Copy(bytesPtr, result, 0, length);
		return result;
	}

	private static string CfStringToStringInternal(IntPtr cfStr)
	{
		if ( cfStr == IntPtr.Zero )
		{
			return string.Empty;
		}

		var buf = new StringBuilder(1024);
		var ok = CFStringGetCString(cfStr, buf, buf.Capacity, 0x08000100);
		return ok ? buf.ToString() : string.Empty;
	}

	private static IntPtr GetSelectorInternal(string name)
	{
		return sel_registerName(name);
	}

	[DllImport(FoundationFramework)]
	private static extern bool CFStringGetCString(IntPtr theString, StringBuilder buffer,
		long bufferSize, uint encoding);

	[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
	[DllImport(ObjcFramework, CharSet = CharSet.Ansi)]
	private static extern IntPtr objc_getClass(string name);

	[SuppressMessage("Usage", "CA2101: Specify marshaling for P/Invoke string arguments")]
	[DllImport(ObjcFramework, CharSet = CharSet.Ansi)]
	private static extern IntPtr sel_registerName(string name);

	[DllImport(ObjcFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr(IntPtr target, IntPtr selector);

	[DllImport(ObjcFramework, EntryPoint = "objc_msgSend")]
	private static extern nint objc_msgSend_retNInt(IntPtr target, IntPtr selector);

	[DllImport(ObjcFramework, EntryPoint = "objc_msgSend")]
	private static extern void objc_msgSend_retVoid_Bool(IntPtr target, IntPtr selector,
		[MarshalAs(UnmanagedType.I1)] bool value);

	[DllImport(ObjcFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr_NUInt_IntPtr_IntPtr_IntPtr(IntPtr target,
		IntPtr selector,
		nuint param1, IntPtr param2, IntPtr param3, IntPtr param4);
}

