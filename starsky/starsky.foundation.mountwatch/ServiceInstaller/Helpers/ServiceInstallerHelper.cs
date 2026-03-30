using System;
using System.IO;

namespace starsky.foundation.mountwatch.Services;

/// <summary>
///     Shared helper methods for service installation
/// </summary>
internal static class ServiceInstallerHelper
{
	/// <summary>
	///     Generate macOS launchd plist XML
	/// </summary>
	internal static string GenerateMacOsPlist(string executablePath)
	{
		return $"""
		        <?xml version="1.0" encoding="UTF-8"?>
		        <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
		        <plist version="1.0">
		        <dict>
		            <key>Label</key>
		            <string>com.starsky.mountwatcher</string>
		            <key>ProgramArguments</key>
		            <array>
		                <string>{executablePath}</string>
		                <string>--verbose</string>
		            </array>
		            <key>RunAtLoad</key>
		            <true/>
		            <key>KeepAlive</key>
		            <true/>
		            <key>StandardOutPath</key>
		            <string>{GetMacOsLogPath("log")}</string>
		            <key>StandardErrorPath</key>
		            <string>{GetMacOsLogPath("error.log")}</string>
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
}

