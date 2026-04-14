using starsky.foundation.native.FileSystem.Interfaces;

namespace starsky.foundation.native.FileSystem;

/// <summary>
///     Handles macOS security-scoped bookmark resolution and access.
///     Security-scoped access is associated with a process. If you start access in parent
///     and then exec into the backend binary (same process, replaced image), access stays.
///     If you spawn a brand-new child process, it does not benefit.
///     @see: https://developer.apple.com/documentation/foundation/nsurl/1417051-bookmarkdata
///     @see:
///     https://developer.apple.com/documentation/foundation/nsurl/1408010-startaccessingsecurityscopedreso
/// </summary>
public sealed class MacOsSecurityScopedBookmark
{
	private readonly IMacOsSecurityScopedBookmarkNative _native;

	/// <summary>Production constructor: uses the real macOS P/Invoke layer.</summary>
	public MacOsSecurityScopedBookmark() : this(new MacOsSecurityScopedBookmarkNative())
	{
	}

	/// <summary>Test constructor: inject a fake or mock native layer.</summary>
	internal MacOsSecurityScopedBookmark(IMacOsSecurityScopedBookmarkNative native)
	{
		_native = native;
	}

	/// <summary>
	///     Attempts to resolve a security-scoped bookmark and start access.
	///     When successful, the resource remains accessible to the current process
	///     and any child processes spawned via exec (same process image).
	/// </summary>
	/// <param name="bookmarkData">Base64-encoded bookmark data from macOS APIs</param>
	/// <param name="resolvedPath">Output parameter: the resolved file path, or null if resolution fails</param>
	/// <returns>true if bookmark was successfully resolved and access started; false otherwise</returns>
	public bool TryResolveAndStartAccess(string bookmarkData, out string? resolvedPath)
	{
		resolvedPath = null;

		try
		{
			// Convert base64-encoded bookmark back to NSData
			var bookmarkBytes = Convert.FromBase64String(bookmarkData);
			var nsData = _native.NsDataFromBytes(bookmarkBytes);

			if ( nsData == IntPtr.Zero )
			{
				return false;
			}

			// Resolve: NSURL(resolvingBookmarkData:options:relativeTo:bookmarkDataIsStale:error:)
			var nsUrl = _native.ResolveBookmarkData(nsData);

			// Release the NSData bookmark
			_native.ObjcRelease(nsData);

			if ( nsUrl == IntPtr.Zero )
			{
				return false;
			}

			// Start accessing security-scoped resource
			if ( !_native.StartAccessingSecurityScopedResource(nsUrl) )
			{
				// Release NSURL — access was not granted
				_native.CfRelease(nsUrl);
				return false;
			}

			// Extract path from NSURL
			resolvedPath = _native.GetPath(nsUrl);

			// Note: DO NOT release nsUrl here because startAccessingSecurityScopedResource
			// establishes a retain that will be balanced when
			// stopAccessingSecurityScopedResource is called (via StopAccess).

			return !string.IsNullOrEmpty(resolvedPath);
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	///     Stops accessing a security-scoped resource. This should be called when
	///     the resource is no longer needed to balance startAccessingSecurityScopedResource.
	/// </summary>
	/// <param name="path">The resolved path returned by TryResolveAndStartAccess</param>
	public void StopAccess(string path)
	{
		try
		{
			var fileUrl = _native.CreateFileUrl(path);

			if ( fileUrl == IntPtr.Zero )
			{
				return;
			}

			_native.StopAccessingSecurityScopedResource(fileUrl);
			_native.CfRelease(fileUrl);
		}
		catch
		{
			// Silently fail — stopping access is best-effort cleanup
		}
	}

	/// <summary>
	///     Creates a security-scoped bookmark from a file path.
	///     Returns base64-encoded bookmark data suitable for later resolution.
	/// </summary>
	/// <param name="filePath">Absolute path to file or directory</param>
	/// <param name="bookmarkBase64">Output parameter: base64-encoded bookmark data</param>
	/// <returns>true if bookmark was created successfully; false otherwise</returns>
	public bool TryCreateBookmark(string filePath, out string? bookmarkBase64)
	{
		bookmarkBase64 = null;

		try
		{
			var fileUrl = _native.CreateFileUrl(filePath);

			if ( fileUrl == IntPtr.Zero )
			{
				return false;
			}

			var bookmarkNsData = _native.CreateBookmarkData(fileUrl);
			_native.CfRelease(fileUrl);

			if ( bookmarkNsData == IntPtr.Zero )
			{
				return false;
			}

			var bytes = _native.NsDataGetBytes(bookmarkNsData);
			_native.ObjcRelease(bookmarkNsData);

			bookmarkBase64 = Convert.ToBase64String(bytes);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
