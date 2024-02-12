using System.Runtime.InteropServices;

namespace starsky.foundation.native.Trash.Helpers;

// https://developer.apple.com/documentation/appkit/nsworkspace/3172700-openapplication
public class MacOsTest2
{
	private const string FoundationFramework = "/System/Library/Frameworks/Foundation.framework/Foundation";

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr(IntPtr target, IntPtr selector);
	
	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern void objc_msgSend_retVoid_IntPtr_IntPtr_IntPtr(
		IntPtr target, 
		IntPtr selector, 
		IntPtr param1, 
		IntPtr param2, 
		IntPtr param3);
	
	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr_IntPtr(IntPtr target, IntPtr selector, IntPtr param);

	public static void OpenApplicationAtURL(
		string applicationURL)
	{
		var cfStrTestFile = MacOsTrashBindingHelper.CreateCfString(applicationURL);
		var nsUrl = objc_getClass("NSURL");
		var fileUrl = objc_msgSend_retIntPtr_IntPtr(nsUrl, MacOsTrashBindingHelper.GetSelector("URLWithString:"), cfStrTestFile);
		
		var charArray = objc_msgSend(fileUrl, sel_registerName("absoluteURL"));
		var test =  Marshal.PtrToStringAnsi(charArray);
		Console.WriteLine(test);

		var nsWorkspace = objc_getClass("NSWorkspace");
		var sharedWorkspace = objc_msgSend_retIntPtr(nsWorkspace, MacOsTrashBindingHelper.GetSelector("sharedWorkspace"));
		var nsWorkspaceOpenConfiguration = objc_getClass("NSWorkspaceOpenConfiguration");
		var nsWorkspaceOpenConfigurationDefault = objc_msgSend_retIntPtr(nsWorkspaceOpenConfiguration, MacOsTrashBindingHelper.GetSelector("configuration"));

		objc_msgSend_retVoid_IntPtr_IntPtr_IntPtr(
			sharedWorkspace,
			MacOsTrashBindingHelper.GetSelector("openApplicationAtURL:configuration:completionHandler:"),
			fileUrl,
			nsWorkspaceOpenConfigurationDefault,
			IntPtr.Zero);
	}
	
	
	public static string? GetText()
	{
		var nsString = objc_getClass("NSString");
		var nsPasteboard = objc_getClass("NSPasteboard");

		var nsStringPboardType = objc_msgSend(objc_msgSend(nsString, sel_registerName("alloc")), sel_registerName("initWithUTF8String:"), "NSStringPboardType");
		var generalPasteboard = objc_msgSend(nsPasteboard, sel_registerName("generalPasteboard"));
		var ptr = objc_msgSend(generalPasteboard, sel_registerName("stringForType:"), nsStringPboardType);
		var charArray = objc_msgSend(ptr, sel_registerName("UTF8String"));
		return Marshal.PtrToStringAnsi(charArray);
	}

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_getClass(string className);

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr sel_registerName(string selectorName);

}
