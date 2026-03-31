using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace starsky.foundation.mountwatch.ServiceInstaller.Helpers;

/// <summary>
///     Shared helper methods for service installation
/// </summary>
internal static class ServiceInstallerHelper
{
	// /// <summary>
	// ///     Check if the application has Full Disk Access on macOS
	// /// </summary>
	// /// <returns>
	// ///     true if Full Disk Access is granted, false if not, null if unable to determine
	// /// </returns>
	// internal static async Task<bool?> CheckMacOsFullDiskAccessAsync()
	// {
	// 	if ( !OperatingSystem.IsMacOS() )
	// 	{
	// 		return null; // Not applicable on non-macOS platforms
	// 	}
	//
	// 	return await Task.Run<bool?>(() =>
	// 	{
	// 		try
	// 		{
	// 			// Try to access a directory that requires Full Disk Access
	// 			// ~/Library/Mail is a good candidate as it's commonly protected
	// 			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	// 			var protectedPath = Path.Combine(home, "Library", "Mail");
	//
	// 			// If the directory doesn't exist, try another protected location
	// 			if ( !Directory.Exists(protectedPath) )
	// 			{
	// 				protectedPath = Path.Combine(home, "Library", "Preferences");
	// 			}
	//
	// 			// Attempt to enumerate the directory
	// 			try
	// 			{
	// 				using ( var enumerator = Directory.EnumerateDirectories(protectedPath).GetEnumerator() )
	// 				{
	// 					_ = enumerator.MoveNext();
	// 				}
	//
	// 				return true; // We have access
	// 			}
	// 			catch ( UnauthorizedAccessException )
	// 			{
	// 				return false; // Access denied
	// 			}
	// 		}
	// 		catch ( Exception )
	// 		{
	// 			// If we can't determine, return null
	// 			return null;
	// 		}
	// 	});
	// }
	//
	// /// <summary>
	// ///     Get a user-friendly message about Full Disk Access setup
	// /// </summary>
	// internal static string GetFullDiskAccessSetupInstructions()
	// {
	// 	return """
	// 	       ⚠️  Full Disk Access Required
	// 	       
	// 	       The Mount Watcher needs Full Disk Access to monitor camera mounts.
	// 	       
	// 	       Steps to grant Full Disk Access:
	// 	       1. Go to System Preferences → Security & Privacy → Privacy
	// 	       2. Select "Full Disk Access" from the left sidebar
	// 	       3. Click the lock icon to make changes (enter your password)
	// 	       4. Click the "+" button and select the Mount Watcher executable
	// 	       5. Restart the application
	// 	       
	// 	       Alternatively, you can run this command to open the preferences:
	// 	         open x-apple.systempreferences:com.apple.preference.security?Privacy_AllFiles
	// 	       """;
	// }
	//
	// /// <summary>
	// ///     Open Full Disk Access settings in System Preferences on macOS
	// /// </summary>
	// internal static void OpenFullDiskAccessSettings()
	// {
	// 	if ( !OperatingSystem.IsMacOS() )
	// 	{
	// 		return; // Not applicable on non-macOS platforms
	// 	}
	//
	// 	Process.Start(new ProcessStartInfo
	// 	{
	// 		FileName = "open",
	// 		Arguments = "x-apple.systempreferences:com.apple.preference.security?Privacy_AllFiles",
	// 		UseShellExecute = false
	// 	});
	// }

	/// <summary>
	///     Generate macOS launchd plist XML
	/// </summary>
	internal static string GenerateMacOsPlist(string executablePath, string serviceName)
	{
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
	///     Get the Linux log path hint
	/// </summary>
	internal static string GetLinuxLogHint()
	{
		return "journalctl -u starsky-mountwatcher.service";
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
