using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using starsky.foundation.native.FileSystem.Interfaces;
using starsky.foundation.platform.Interfaces;

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
	private readonly IWebLogger _logger;
	private readonly IMacOsSecurityScopedBookmarkNative _native;

	/// <summary>Production constructor: uses the real macOS P/Invoke layer.</summary>
	public MacOsSecurityScopedBookmark(IWebLogger logger) : this(
		new MacOsSecurityScopedBookmarkNative(), logger)
	{
		_logger = logger;
	}

	/// <summary>Test constructor: inject a fake or mock native layer.</summary>
	internal MacOsSecurityScopedBookmark(IMacOsSecurityScopedBookmarkNative native,
		IWebLogger logger)
	{
		_native = native;
		_logger = logger;
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
				_logger?.LogError("NsData could not be parsed");
				return false;
			}

			// Resolve: NSURL(resolvingBookmarkData:options:relativeTo:bookmarkDataIsStale:error:)
			var nsUrl = _native.ResolveBookmarkData(nsData);

			// Release the NSData bookmark
			_native.ObjcRelease(nsData);

			if ( nsUrl == IntPtr.Zero )
			{
				_logger?.LogError("nsUrl could not be parsed");
				return false;
			}

			// Start accessing security-scoped resource
			if ( !_native.StartAccessingSecurityScopedResource(nsUrl) )
			{
				_logger?.LogError("StartAccessingSecurityScopedResource failed");

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

	/// <summary>
	///     Resolves a pre-existing security-scoped bookmark token that was created by a macOS
	///     folder-picker dialog in an Electron/Swift desktop process, and starts file access
	///     for the current .NET process.
	///     <para>
	///         Pattern:
	///         <list type="number">
	///             <item>Electron/Swift picks folder → creates bookmark → stores token in AppSettings</item>
	///             <item>.NET reads AppSettings → calls TryStartAccessFromToken → gains file access</item>
	///         </list>
	///     </para>
	///     <para>
	///         The <paramref name="bookmarkToken" /> may be raw base64 <em>or</em> the
	///         JSON-string-encoded form that Swift produces via
	///         <c>JSONEncoder().encode(base64EncodedString())</c>. Both formats are handled
	///         automatically.
	///     </para>
	///     <para>On non-macOS platforms the call is a safe no-op returning <c>false</c>.</para>
	/// </summary>
	/// <param name="storageFolder">
	///     Expected storage folder path. Included for call-site clarity; the resolved path
	///     comes from the bookmark itself.
	/// </param>
	/// <param name="bookmarkToken">
	///     Base64 or JSON-quoted base64 bookmark token from Swift/Electron.
	/// </param>
	/// <returns>true if access was successfully started; false otherwise.</returns>
	[SuppressMessage("ReSharper", "UnusedParameter.Global")]
	[SuppressMessage("Usage", "CA1801:Review unused parameters")]
	[SuppressMessage("Style", "IDE0060:Remove unused parameter")]
	public bool TryStartAccessFromToken(string storageFolder, string? bookmarkToken)
	{
		if ( string.IsNullOrEmpty(bookmarkToken) )
		{
			return false;
		}

		var rawBase64 = UnwrapJsonToken(bookmarkToken);
		return TryResolveAndStartAccess(rawBase64, out _);
	}

	/// <summary>
	///     Handles both raw base64 and the JSON-string-encoded form produced by Swift:
	///     <code>
	///         let base64 = bookmarkData.base64EncodedString()
	///         JSONEncoder().encode(base64) // produces bytes for: "base64..."
	///         // After String(data:encoding:) the Swift string VALUE contains surrounding quotes
	///     </code>
	///     When that value is stored in JSON settings and read back by System.Text.Json the
	///     outer JSON layer is stripped, leaving a .NET string whose value starts and ends
	///     with a literal <c>"</c> character. This helper peels off that extra layer.
	/// </summary>
	internal static string UnwrapJsonToken(string token)
	{
		if ( token.Length > 2 && token[0] == '"' && token[^1] == '"' )
		{
			try
			{
				var decoded = JsonSerializer.Deserialize<string>(token);
				if ( !string.IsNullOrEmpty(decoded) )
				{
					return decoded;
				}
			}
			catch
			{
				// ignored — fall through to simple quote-strip
			}

			return token[1..^1];
		}

		return token;
	}
}
