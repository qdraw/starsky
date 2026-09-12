using System;
using System.IO;

namespace starsky.foundation.mountwatch.ServiceInstaller.Helpers;

/// <summary>
///     Shared helper methods for service installation
/// </summary>
internal static class ServiceInstallerHelper
{
	/// <summary>
	///     Generate macOS launchd plist XML
	/// </summary>
	internal static string GenerateMacOsPlist(string executablePath, string serviceName)
	{
		var appSupport = GetMacOsAppSupportPath();
		var caches = GetMacOsCachesPath();
		return $"""
		        <?xml version="1.0" encoding="UTF-8"?>
		        <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
		        <plist version="1.0">
		        <dict>
		            <key>Label</key>
		            <string>{serviceName}</string>
		            <key>ProgramArguments</key>
		            <array>
		                <string>{executablePath}</string>
		                <string></string>
		            </array>
		            <key>RunAtLoad</key>
		            <true/>
		            <key>KeepAlive</key>
		            <true/>
		            <key>StandardOutPath</key>
		            <string>{GetMacOsLogPath("log")}</string>
		            <key>StandardErrorPath</key>
		            <string>{GetMacOsLogPath("error.log")}</string>
		            <key>EnvironmentVariables</key>
		            <dict>
		                <key>app__appsettingspath</key>
		                <string>{appSupport}/appsettings.json</string>
		                <key>app__appsettingslocalpath</key>
		                <string>{appSupport}/appsettings.local.json</string>
		                <key>app__databaseConnection</key>
		                <string>Data Source={appSupport}/starsky.db</string>
		                <key>app__tempFolder</key>
		                <string>{caches}/tempFolder/</string>
		                <key>app__thumbnailTempFolder</key>
		                <string>{appSupport}/thumbnailTempFolder/</string>
		            </dict>
		        </dict>
		        </plist>
		        """;
	}

	/// <summary>
	///     Generate Linux systemd unit file
	/// </summary>
	internal static string GenerateLinuxSystemdUnit(string executablePath)
	{
		return $"""
		        [Unit]
		        Description=Starsky Mount Watcher
		        After=network.target

		        [Service]
		        Type=simple
		        ExecStart={executablePath} --verbose
		        Restart=on-failure
		        RestartSec=10

		        [Install]
		        WantedBy=multi-user.target
		        """;
	}

	/// <summary>
	///     Get the macOS log file path
	/// </summary>
	private static string GetMacOsLogPath(string suffix)
	{
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, "Library", "Logs", "starsky", $"mountwatcher.{suffix}");
	}

	private static string GetMacOsAppSupportPath()
	{
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, "Library", "Application Support", "starsky");
	}

	private static string GetMacOsCachesPath()
	{
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, "Library", "Caches", "starsky");
	}
}
