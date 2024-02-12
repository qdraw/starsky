using System.Runtime.InteropServices;

namespace starsky.foundation.native.Trash.Helpers;

public static class MacOsOpenUrl
{
	/// <summary>
	/// Does NOT check if file exists
	/// </summary>
	/// <param name="fileUrl">Absolute Path of file</param>
	/// <returns></returns>
	public static bool OpenDefault(
		string fileUrl)
	{
		var fileUrlIntPtr =
			MacOsTrashBindingHelper.GetUrls([fileUrl]).FirstOrDefault();
		
		return objc_msgSend_retBool_IntPtr_IntPtr(
			NsWorkspaceSharedWorksPace(),
			MacOsTrashBindingHelper.GetSelector("openURL:"),
			fileUrlIntPtr);
	}

	public static void OpenApplicationAtUrl(
		string fileUrl,
		string applicationUrl)
	{
		var fileUrlIntPtr = MacOsTrashBindingHelper.GetUrls([fileUrl]);
		var fileUrlIntPtrUrlArray = MacOsTrashBindingHelper.CreateCfArray(fileUrlIntPtr);

		var applicationUrlIntPtr =
			MacOsTrashBindingHelper.GetUrls([applicationUrl]).FirstOrDefault();

		var nsWorkspaceOpenConfiguration = objc_getClass("NSWorkspaceOpenConfiguration");
		var nsWorkspaceOpenConfigurationDefault = objc_msgSend_retIntPtr(
			nsWorkspaceOpenConfiguration, MacOsTrashBindingHelper.GetSelector("configuration"));
		
		// https://developer.apple.com/documentation/appkit/nsworkspace/3172702-openurls?language=objc
		objc_msgSend_retVoid_IntPtr_IntPtr_IntPtr_IntPtr(
			NsWorkspaceSharedWorksPace(),
			MacOsTrashBindingHelper.GetSelector("openURLs:withApplicationAtURL:configuration:completionHandler:"),
			fileUrlIntPtrUrlArray,
			applicationUrlIntPtr,
			nsWorkspaceOpenConfigurationDefault,
			IntPtr.Zero);
	}
	
	private const string FoundationFramework =
		"/System/Library/Frameworks/Foundation.framework/Foundation";

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retIntPtr(IntPtr target, IntPtr selector);

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retVoid_IntPtr_IntPtr_IntPtr_IntPtr(
		IntPtr target,
		IntPtr selector,
		IntPtr param1,
		IntPtr param2,
		IntPtr param3,
		IntPtr param4);
	
	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern IntPtr objc_msgSend_retVoid_IntPtr(
		IntPtr target,
		IntPtr selector,
		IntPtr param1);
	
	[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
	static extern IntPtr objc_getClass(string className);
	
	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern bool objc_msgSend_retBool_IntPtr_IntPtr(IntPtr target, IntPtr selector, IntPtr param);


	
	private static IntPtr NsWorkspaceSharedWorksPace()
	{
		// Namespace
		var nsWorkspace = objc_getClass("NSWorkspace");
		return objc_msgSend_retIntPtr(nsWorkspace,
			MacOsTrashBindingHelper.GetSelector("sharedWorkspace"));
	}

}
