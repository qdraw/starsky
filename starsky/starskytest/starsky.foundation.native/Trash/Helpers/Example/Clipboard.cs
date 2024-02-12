using System;
using System.Runtime.InteropServices;

namespace starskytest.starsky.foundation.native.Trash.Helpers.Example;

public static class Clipboard
{
	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr sel_registerName(string selectorName);

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_getClass(string className);

	public static string? GetText()
	{
		var nsString = objc_getClass("NSString");
		var nsPasteboard = objc_getClass("NSPasteboard");

		var nsStringPboardType = objc_msgSend(objc_msgSend(nsString, sel_registerName("alloc")),
			sel_registerName("initWithUTF8String:"), "NSStringPboardType");
		var generalPasteboard = objc_msgSend(nsPasteboard, sel_registerName("generalPasteboard"));
		var ptr = objc_msgSend(generalPasteboard, sel_registerName("stringForType:"),
			nsStringPboardType);
		var charArray = objc_msgSend(ptr, sel_registerName("UTF8String"));
		return Marshal.PtrToStringAnsi(charArray);
	}
}
