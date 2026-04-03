using System;

namespace starsky.foundation.mountwatch.ServiceInstaller.Helpers;

public static class WatchServiceName
{
#if DEBUG
	private const string ReverseDnsServiceName = "nl.qdraw.mountwatcher.debug";
#else
	private const string ReverseDnsServiceName = "nl.qdraw.mountwatcher";
#endif
	public static string GetReverseDnsName()
	{
		if ( IsRunningTest() )
		{
			return ServiceDisplayName + "-test";
		}

		return ReverseDnsServiceName;
	}

#if DEBUG
	private const string SystemdServiceName = "starsky-mountwatcher-debug";
#else
	private const string SystemdServiceName = "starsky-mountwatcher";
#endif

	public static string GetSystemDName()
	{
		if ( IsRunningTest() )
		{
			return ServiceDisplayName + "-test";
		}

		return SystemdServiceName;
	}

#if DEBUG
	private const string ServiceDisplayName = "Starsky Mount Watcher (debug)";
#else
	private const string ServiceDisplayName = "Starsky Mount Watcher";
#endif

	public static string GetDisplayName()
	{
		return ServiceDisplayName;
	}

	/// <summary>
	///     Get the Linux log path hint
	/// </summary>
	internal static string GetLinuxLogHint()
	{
		return $"journalctl -u {SystemdServiceName}";
	}

	private static bool IsRunningTest()
	{
		return AppDomain.CurrentDomain.FriendlyName.Contains("test",
			StringComparison.OrdinalIgnoreCase);
	}
}
