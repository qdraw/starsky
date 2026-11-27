using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.native.SecurityScoped.Helpers;

/// <summary>
/// macOS CoreFoundation P/Invoke bindings for security-scoped bookmark resolution.
/// Used by sandboxed MAS builds to regain access to user-selected directories
/// across process launches without re-presenting the NSOpenPanel.
/// </summary>
[SuppressMessage("Interoperability",
	"SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' " +
	"to generate P/Invoke marshalling code at compile time")]
public static class CoreFoundationSecurityBookmarkBindings
{
	// kCFURLBookmarkResolutionWithSecurityScope
	private const uint ResolutionWithSecurityScope = 0x400u;

	// kCFURLPOSIXPathStyle
	private const long PosixPathStyle = 0L;

	// kCFStringEncodingUTF8
	private const uint Utf8Encoding = 0x08000100u;

	private const string CoreFoundation =
		"/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

	/// <summary>
	/// Resolves a security-scoped bookmark and starts accessing the resource.
	/// Returns null when not running on macOS; returns (IntPtr.Zero, null) when
	/// the bookmark data is invalid or resolution fails.
	/// The caller is responsible for calling <see cref="StopAccessing"/> when done.
	/// </summary>
	internal static (IntPtr cfUrl, string? path)? ResolveAndStartAccessing(
		byte[] bookmarkBytes, OSPlatform platform)
	{
		if ( platform != OSPlatform.OSX )
		{
			return null;
		}

		var cfData = CfDataCreate(IntPtr.Zero, bookmarkBytes, bookmarkBytes.Length);
		if ( cfData == IntPtr.Zero )
		{
			return (IntPtr.Zero, null);
		}

		try
		{
			var cfUrl = CfUrlCreateByResolvingBookmarkData(
				IntPtr.Zero, cfData,
				ResolutionWithSecurityScope,
				IntPtr.Zero, IntPtr.Zero,
				out _, out _);

			if ( cfUrl == IntPtr.Zero )
			{
				return (IntPtr.Zero, null);
			}

			CfUrlStartAccessingSecurityScopedResource(cfUrl);
			var path = CopyPath(cfUrl);
			return (cfUrl, path);
		}
		finally
		{
			CfRelease(cfData);
		}
	}

	/// <summary>
	/// Stops accessing a security-scoped resource and releases the CF object.
	/// No-op when not on macOS or when cfUrl is IntPtr.Zero.
	/// </summary>
	internal static void StopAccessing(IntPtr cfUrl, OSPlatform platform)
	{
		if ( platform != OSPlatform.OSX || cfUrl == IntPtr.Zero )
		{
			return;
		}

		CfUrlStopAccessingSecurityScopedResource(cfUrl);
		CfRelease(cfUrl);
	}

	private static string? CopyPath(IntPtr cfUrl)
	{
		var cfPath = CfUrlCopyFileSystemPath(cfUrl, PosixPathStyle);
		if ( cfPath == IntPtr.Zero )
		{
			return null;
		}

		try
		{
			var buf = new byte[4096];
			if ( !CfStringGetCString(cfPath, buf, buf.Length, Utf8Encoding) )
			{
				return null;
			}

			return Encoding.UTF8.GetString(buf).TrimEnd('\0');
		}
		finally
		{
			CfRelease(cfPath);
		}
	}

	[DllImport(CoreFoundation, EntryPoint = "CFDataCreate")]
	internal static extern IntPtr CfDataCreate(IntPtr alloc, byte[] bytes, long length);

	[DllImport(CoreFoundation, EntryPoint = "CFURLCreateByResolvingBookmarkData")]
	internal static extern IntPtr CfUrlCreateByResolvingBookmarkData(
		IntPtr alloc,
		IntPtr bookmarkData,
		uint options,
		IntPtr relativeToUrl,
		IntPtr resourcePropertiesToInclude,
		[MarshalAs(UnmanagedType.I1)] out bool isStale,
		out IntPtr error);

	[DllImport(CoreFoundation, EntryPoint = "CFURLStartAccessingSecurityScopedResource")]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool CfUrlStartAccessingSecurityScopedResource(IntPtr url);

	[DllImport(CoreFoundation, EntryPoint = "CFURLStopAccessingSecurityScopedResource")]
	internal static extern void CfUrlStopAccessingSecurityScopedResource(IntPtr url);

	[DllImport(CoreFoundation, EntryPoint = "CFURLCopyFileSystemPath")]
	internal static extern IntPtr CfUrlCopyFileSystemPath(IntPtr url, long pathStyle);

	[DllImport(CoreFoundation, EntryPoint = "CFStringGetCString")]
	[return: MarshalAs(UnmanagedType.I1)]
	internal static extern bool CfStringGetCString(
		IntPtr cfString, byte[] buffer, long bufferSize, uint encoding);

	[DllImport(CoreFoundation, EntryPoint = "CFRelease")]
	internal static extern void CfRelease(IntPtr cf);
}
