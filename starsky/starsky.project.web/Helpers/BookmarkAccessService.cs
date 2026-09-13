using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace starsky.project.web.Helpers;

/// <summary>
/// Resolves macOS security-scoped bookmarks stored in the App Group container by the
/// host app and calls CFURLStartAccessingSecurityScopedResource for each one.
/// This gives the sandboxed backend process access to user-selected photo directories
/// without requiring the directories to be inside the sandbox container.
///
/// Only active on macOS when the STARSKY_APP_GROUP environment variable is set.
/// </summary>
public sealed class BookmarkAccessService : IHostedService, IDisposable
{
	private readonly ILogger<BookmarkAccessService> _logger;
	private readonly List<IntPtr> _accessedUrls = new();

	// kCFURLBookmarkResolutionWithSecurityScope = 0x400
	private const uint ResolutionWithSecurityScope = 0x400u;
	private const long PosixPathStyle = 0L;
	private const uint Utf8Encoding = 0x08000100u;

	public BookmarkAccessService(ILogger<BookmarkAccessService> logger)
	{
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		if ( !OperatingSystem.IsMacOS() )
		{
			return Task.CompletedTask;
		}

		var appGroup = Environment.GetEnvironmentVariable("STARSKY_APP_GROUP");
		if ( string.IsNullOrEmpty(appGroup) )
		{
			return Task.CompletedTask;
		}

		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var bookmarksDir = Path.Combine(
			home, "Library", "Group Containers", appGroup, "bookmarks");

		if ( !Directory.Exists(bookmarksDir) )
		{
			return Task.CompletedTask;
		}

		foreach ( var bookmarkFile in Directory.EnumerateFiles(bookmarksDir, "*.bookmark") )
		{
			try
			{
				var bytes = File.ReadAllBytes(bookmarkFile);
				var (cfUrl, path) = ResolveAndStart(bytes);
				if ( cfUrl != IntPtr.Zero )
				{
					_accessedUrls.Add(cfUrl);
					_logger.LogInformation("Security-scoped access started for {Path}", path);
				}
				else
				{
					_logger.LogWarning("Failed to resolve bookmark {File}", Path.GetFileName(bookmarkFile));
				}
			}
			catch ( Exception ex )
			{
				_logger.LogWarning(ex, "Error resolving bookmark {File}", Path.GetFileName(bookmarkFile));
			}
		}

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		ReleaseAll();
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		ReleaseAll();
	}

	private void ReleaseAll()
	{
		foreach ( var cfUrl in _accessedUrls )
		{
			try
			{
				CfUrlStopAccessingSecurityScopedResource(cfUrl);
			}
			catch
			{
				// best-effort
			}

			try
			{
				CfRelease(cfUrl);
			}
			catch
			{
				// best-effort
			}
		}

		_accessedUrls.Clear();
	}

	private static (IntPtr cfUrl, string? path) ResolveAndStart(byte[] bookmarkBytes)
	{
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

			var cfPath = CfUrlCopyFileSystemPath(cfUrl, PosixPathStyle);
			string? path = null;
			if ( cfPath != IntPtr.Zero )
			{
				var buf = new byte[4096];
				if ( CfStringGetCString(cfPath, buf, buf.Length, Utf8Encoding) )
				{
					path = Encoding.UTF8.GetString(buf).TrimEnd('\0');
				}

				CfRelease(cfPath);
			}

			return (cfUrl, path);
		}
		finally
		{
			CfRelease(cfData);
		}
	}

	private const string CoreFoundation =
		"/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

	[DllImport(CoreFoundation, EntryPoint = "CFDataCreate")]
	private static extern IntPtr CfDataCreate(IntPtr alloc, byte[] bytes, long length);

	[DllImport(CoreFoundation, EntryPoint = "CFURLCreateByResolvingBookmarkData")]
	private static extern IntPtr CfUrlCreateByResolvingBookmarkData(
		IntPtr alloc,
		IntPtr bookmarkData,
		uint options,
		IntPtr relativeToUrl,
		IntPtr resourcePropertiesToInclude,
		[MarshalAs(UnmanagedType.I1)] out bool isStale,
		out IntPtr error);

	[DllImport(CoreFoundation, EntryPoint = "CFURLStartAccessingSecurityScopedResource")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool CfUrlStartAccessingSecurityScopedResource(IntPtr url);

	[DllImport(CoreFoundation, EntryPoint = "CFURLStopAccessingSecurityScopedResource")]
	private static extern void CfUrlStopAccessingSecurityScopedResource(IntPtr url);

	[DllImport(CoreFoundation, EntryPoint = "CFURLCopyFileSystemPath")]
	private static extern IntPtr CfUrlCopyFileSystemPath(IntPtr url, long pathStyle);

	[DllImport(CoreFoundation, EntryPoint = "CFStringGetCString")]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool CfStringGetCString(
		IntPtr cfString, byte[] buffer, long bufferSize, uint encoding);

	[DllImport(CoreFoundation, EntryPoint = "CFRelease")]
	private static extern void CfRelease(IntPtr cf);
}
