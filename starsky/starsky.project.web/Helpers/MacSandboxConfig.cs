using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.project.web.Helpers;

/// <summary>
/// Applies configuration when running as a sandboxed MAS Login Item.
/// Derives all paths from the STARSKY_APP_GROUP environment variable (set via
/// LSEnvironment in the login item's Info.plist) and writes them as environment
/// variables before the ASP.NET host is built.
/// Returns the appsettings.json path inside the App Group container so that
/// callers can pass it to PortProgramHelper.
/// </summary>
public static class MacSandboxConfig
{
	private const string AppGroupEnvKey = "STARSKY_APP_GROUP";

	/// <summary>
	/// Sets backend environment variables from the App Group container when
	/// STARSKY_APP_GROUP is present. Returns the appsettings.json path when
	/// applied, or null when not running as a MAS login item.
	/// </summary>
	public static string? TryApply()
	{
		if ( !OperatingSystem.IsMacOS() )
		{
			return null;
		}

		var appGroup = Environment.GetEnvironmentVariable(AppGroupEnvKey);
		if ( string.IsNullOrEmpty(appGroup) )
		{
			return null;
		}

		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var groupContainer = Path.Combine(home, "Library", "Group Containers", appGroup);

		ApplyPaths(groupContainer);
		return Path.Combine(groupContainer, "appsettings.json");
	}

	internal static void ApplyPaths(string groupContainer)
	{
		Set("app__appsettingspath", Path.Combine(groupContainer, "appsettings.json"));
		Set("app__appsettingslocalpath", Path.Combine(groupContainer, "appsettings.local.json"));
		Set("app__databaseConnection", "Data Source=" + Path.Combine(groupContainer, "starsky.db"));
		Set("app__tempFolder", Path.Combine(groupContainer, "tmp") + Path.DirectorySeparatorChar);
		Set("app__thumbnailTempFolder", Path.Combine(groupContainer, "thumbnailTempFolder") + Path.DirectorySeparatorChar);
		Set("app__NoAccountLocalhost", "true");
		Set("app__UseLocalDesktop", "true");
		Set("app__AccountRegisterDefaultRole", "Administrator");
		Set("app__ThumbnailGenerationIntervalInMinutes", "300");
		Set("app__Verbose", "false");
	}

	private static void Set(string key, string value)
	{
		Environment.SetEnvironmentVariable(key, value);
	}
}
