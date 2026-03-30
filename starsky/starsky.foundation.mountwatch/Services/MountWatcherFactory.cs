using System;
using starsky.foundation.injection;
using starsky.foundation.mountwatch.Interfaces;

namespace starsky.foundation.mountwatch.Services;

/// <summary>
///     Factory for creating OS-specific mount watchers
/// </summary>
[Service(typeof(IMountWatcherFactory), InjectionLifetime = InjectionLifetime.Singleton)]
public class MountWatcherFactory : IMountWatcherFactory
{
	/// <summary>
	///     Create appropriate mount watcher for current OS
	/// </summary>
	public IMountWatcher CreateMountWatcher()
	{
		if ( OperatingSystem.IsMacOS() )
		{
			return new MacMountWatcher();
		}

		if ( OperatingSystem.IsWindows() )
		{
			return new WindowsMountWatcher();
		}

		if ( OperatingSystem.IsLinux() )
		{
			return new LinuxMountWatcher();
		}

		throw new NotSupportedException("Operating system is not supported");
	}
}

