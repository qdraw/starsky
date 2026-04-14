namespace starsky.foundation.native.FileSystem.Interfaces;

/// <summary>
///     Abstraction over macOS Foundation framework operations for security-scoped bookmarks.
///     The real implementation is <c>MacOsSecurityScopedBookmarkNative</c>,
///     which is marked <see cref="System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute" />
///     .
///     Tests inject a fake via the internal constructor of <see cref="MacOsSecurityScopedBookmark" />.
/// </summary>
internal interface IMacOsSecurityScopedBookmarkNative
{
	/// <summary>Creates an NSURL pointing at the given file-system path.</summary>
	IntPtr CreateFileUrl(string path);

	/// <summary>
	///     Creates NSData holding a security-scoped bookmark for the given file NSURL.
	///     Equivalent to <c>[url bookmarkDataWithOptions:NSURLBookmarkCreationWithSecurityScope ...]</c>.
	/// </summary>
	IntPtr CreateBookmarkData(IntPtr fileUrl);

	/// <summary>
	///     Resolves bookmark NSData to an NSURL.
	///     Equivalent to
	///     <c>[NSURL URLByResolvingBookmarkData:options:NSURLBookmarkResolutionWithSecurityScope ...]</c>.
	/// </summary>
	IntPtr ResolveBookmarkData(IntPtr nsData);

	/// <summary>Calls <c>-startAccessingSecurityScopedResource</c> on the URL.</summary>
	bool StartAccessingSecurityScopedResource(IntPtr url);

	/// <summary>Calls <c>-stopAccessingSecurityScopedResource</c> on the URL.</summary>
	void StopAccessingSecurityScopedResource(IntPtr url);

	/// <summary>Extracts the file-system path string from an NSURL via <c>-path</c>.</summary>
	string GetPath(IntPtr url);

	/// <summary>Creates an NSData from a managed byte array via <c>[NSData dataWithBytes:length:]</c>.</summary>
	IntPtr NsDataFromBytes(byte[] bytes);

	/// <summary>Copies the bytes of an NSData into a managed byte array.</summary>
	byte[] NsDataGetBytes(IntPtr nsData);

	/// <summary>Calls Core Foundation <c>CFRelease</c>.</summary>
	void CfRelease(IntPtr handle);

	/// <summary>Sends Objective-C <c>-release</c> to an object.</summary>
	void ObjcRelease(IntPtr obj);
}
