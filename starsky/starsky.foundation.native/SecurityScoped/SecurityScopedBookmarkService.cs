using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using starsky.foundation.injection;
using starsky.foundation.native.SecurityScoped.Helpers;

namespace starsky.foundation.native.SecurityScoped;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class SecurityScopedBookmarkService : IHostedService, IDisposable
{
	internal delegate bool IsOsPlatformDelegate(OSPlatform platform);

	private readonly ILogger<SecurityScopedBookmarkService> _logger;
	private readonly List<IntPtr> _accessedUrls = [];

	public SecurityScopedBookmarkService(ILogger<SecurityScopedBookmarkService> logger)
	{
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		return StartAccessingInternal(RuntimeInformation.IsOSPlatform, GetBookmarksDirectory());
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

	/// <summary>
	/// Returns the bookmarks directory from the App Group container, or null
	/// when the STARSKY_APP_GROUP environment variable is not set.
	/// </summary>
	internal static string? GetBookmarksDirectory()
	{
		var appGroup = Environment.GetEnvironmentVariable("STARSKY_APP_GROUP");
		if ( string.IsNullOrEmpty(appGroup) )
		{
			return null;
		}

		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, "Library", "Group Containers", appGroup, "bookmarks");
	}

	internal Task StartAccessingInternal(IsOsPlatformDelegate isOsPlatform,
		string? bookmarksDirectory)
	{
		if ( !isOsPlatform(OSPlatform.OSX) || bookmarksDirectory == null ||
		     !Directory.Exists(bookmarksDirectory) )
		{
			return Task.CompletedTask;
		}

		foreach ( var bookmarkFile in Directory.EnumerateFiles(bookmarksDirectory, "*.bookmark") )
		{
			try
			{
				var bytes = File.ReadAllBytes(bookmarkFile);
				var result = CoreFoundationSecurityBookmarkBindings
					.ResolveAndStartAccessing(bytes, OSPlatform.OSX);

				if ( result is { cfUrl: var cfUrl, path: var path } && cfUrl != IntPtr.Zero )
				{
					_accessedUrls.Add(cfUrl);
					_logger.LogInformation("Security-scoped access started for {Path}", path);
				}
				else
				{
					_logger.LogWarning("Failed to resolve bookmark {File}",
						Path.GetFileName(bookmarkFile));
				}
			}
			catch ( Exception ex )
			{
				_logger.LogWarning(ex, "Error resolving bookmark {File}",
					Path.GetFileName(bookmarkFile));
			}
		}

		return Task.CompletedTask;
	}

	private void ReleaseAll()
	{
		foreach ( var cfUrl in _accessedUrls )
		{
			CoreFoundationSecurityBookmarkBindings.StopAccessing(cfUrl, OSPlatform.OSX);
		}

		_accessedUrls.Clear();
	}
}
