using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using starsky.foundation.native.Trash.Helpers;

namespace starsky.foundation.native.OpenApplicationNative.Helpers;

[SuppressMessage("Interoperability",
	"SYSLIB1054:Use \'LibraryImportAttribute\' instead of \'DllImportAttribute\' to " +
	"generate P/Invoke marshalling code at compile time")]
public static class MacOsOpenUrl
{
	/// <summary>
	/// Add check if not Mac OS X
	/// </summary>
	/// <param name="fileUrls"></param>
	/// <param name="platform"></param>
	/// <returns></returns>
	internal static bool? OpenDefault(
		List<string> fileUrls, OSPlatform platform)
	{
		return platform != OSPlatform.OSX ? null : OpenDefault(fileUrls);
	}

	/// <summary>
	/// Does NOT check if file exists
	/// </summary>
	/// <param name="fileUrls">Absolute Path of file</param>
	/// <returns></returns>
	public static bool OpenDefault(
		List<string> fileUrls)
	{
		if ( fileUrls.Count == 0 )
		{
			return false;
		}

		var fileUrlsIntPtr = MacOsTrashBindingHelper.GetUrls(fileUrls);

		var result = new List<bool>();
		foreach ( var fileUrlIntPtr in fileUrlsIntPtr )
		{
			result.Add(InvokeOpenUrl(fileUrlIntPtr));
		}

		return result.TrueForAll(p => p);
	}

	internal static bool? OpenApplicationAtUrl(
		List<string> fileUrls,
		string applicationUrl, OSPlatform platform)
	{
		return platform != OSPlatform.OSX ? null : OpenApplicationAtUrl(fileUrls, applicationUrl);
	}

	/// <summary>
	/// Does NOT check if a file exists
	/// No Fallback if NOT Mac OS X
	/// </summary>
	/// <param name="fileUrls">Absolute Paths</param>
	/// <param name="applicationUrl">Open with .app folder</param>
	/// <exception cref="DllNotFoundException">When not Mac OS</exception>
	internal static bool? OpenApplicationAtUrl(
		List<string> fileUrls,
		string applicationUrl)
	{
		if ( fileUrls.Count == 0 )
		{
			return false;
		}

		var filesUrlIntPtr = MacOsTrashBindingHelper.GetUrls(fileUrls);
		var fileUrlIntPtrUrlArray = MacOsTrashBindingHelper.CreateCfArray(filesUrlIntPtr);

		var applicationUrlIntPtr =
			MacOsTrashBindingHelper.GetUrls([applicationUrl]).FirstOrDefault();

		var nsWorkspaceOpenConfiguration = objc_getClass("NSWorkspaceOpenConfiguration");
		var nsWorkspaceOpenConfigurationDefault = objc_msgSend_retIntPtr(
			nsWorkspaceOpenConfiguration, MacOsTrashBindingHelper.GetSelector("configuration"));

		// https://developer.apple.com/documentation/appkit/nsworkspace/3172702-openurls?language=objc
		OpenUrLsWithApplicationAtUrl(fileUrlIntPtrUrlArray, applicationUrlIntPtr,
			nsWorkspaceOpenConfigurationDefault);
		return true;
	}

	/// <summary>
	/// Open Default Url
	/// </summary>
	/// <param name="fileUrlIntPtr">Pointer for urls</param>
	/// <returns>Is Success</returns>
	internal static bool InvokeOpenUrl(IntPtr fileUrlIntPtr)
	{
		return objc_msgSend_retBool_IntPtr_IntPtr(
			NsWorkspaceSharedWorkSpace(),
			MacOsTrashBindingHelper.GetSelector("openURL:"),
			fileUrlIntPtr);
	}


	/// <summary>
	/// @see: https://developer.apple.com/documentation/appkit/nsworkspace/3172702-openurls?language=objc
	/// </summary>
	internal static void OpenUrLsWithApplicationAtUrl(nint fileUrlIntPtrUrlArray,
		nint applicationUrlIntPtr, nint nsWorkspaceOpenConfigurationDefault)
	{
		objc_msgSend_retVoid_IntPtr_IntPtr_IntPtr_IntPtr(
			NsWorkspaceSharedWorkSpace(),
			MacOsTrashBindingHelper.GetSelector(
				"openURLs:withApplicationAtURL:configuration:completionHandler:"),
			fileUrlIntPtrUrlArray,
			applicationUrlIntPtr,
			nsWorkspaceOpenConfigurationDefault,
			IntPtr.Zero);
	}

	private const string FoundationFramework =
		"/System/Library/Frameworks/Foundation.framework/Foundation";

	private const string AppKitFramework =
		"/System/Library/Frameworks/AppKit.framework/AppKit";

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

	[DllImport(AppKitFramework)]
	[SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments")]
	static extern IntPtr objc_getClass(string className);

	[DllImport(FoundationFramework, EntryPoint = "objc_msgSend")]
	private static extern bool objc_msgSend_retBool_IntPtr_IntPtr(IntPtr target, IntPtr selector,
		IntPtr param);

	internal static IntPtr NsWorkspaceSharedWorkSpace()
	{
		// Namespace
		var nsWorkspace = objc_getClass("NSWorkspace");
		return objc_msgSend_retIntPtr(nsWorkspace,
			MacOsTrashBindingHelper.GetSelector("sharedWorkspace"));
	}
}
