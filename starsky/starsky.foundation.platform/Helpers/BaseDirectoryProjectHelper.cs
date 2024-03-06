using System;

namespace starsky.foundation.platform.Helpers;

public static class BaseDirectoryProjectHelper
{
	private const string Starsky = "starsky";

	/// <summary>
	/// Root of the project with replaced value
	/// </summary>
	public static string BaseDirectoryProject => AppDomain.CurrentDomain
		.BaseDirectory
		.Replace("starskyadmincli", Starsky)
		.Replace("starskysynchronizecli", Starsky)
		.Replace("starskythumbnailcli", Starsky)
		.Replace("starskythumbnailmetacli", Starsky)
		.Replace("starskysynccli", Starsky)
		.Replace("starsky.foundation.database", Starsky)
		.Replace("starskyimportercli", Starsky)
		.Replace("starskywebftpcli", Starsky)
		.Replace("starskywebhtmlcli", Starsky)
		.Replace("starskygeocli", Starsky)
		.Replace("starskytest", Starsky)
		.Replace("starskydiskwatcherworkerservice", Starsky)
		.Replace("starskydemoseedcli", Starsky);
}
