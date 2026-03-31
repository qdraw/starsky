namespace starsky.foundation.mountwatch.ServiceInstaller.Helpers;

public static class WatchServiceName
{
#if DEBUG
	private const string ReverseDnsServiceName = "nl.qdraw.mountwatcher.debug";
#else
	private const string ServiceName = "nl.qdraw.mountwatcher";
#endif
	public static string GetReverseDnsName()
	{
		return ReverseDnsServiceName;
	}
	
#if DEBUG
	private const string SystemdServiceName = "starsky-mountwatcher-debug";
#else
	private const string SystemdServiceName = "starsky-mountwatcher";
#endif

	public static string GetSystemDName()
	{
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
}
