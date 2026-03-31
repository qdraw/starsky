using System;
using starsky.foundation.injection;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Factory for creating OS-specific mount watchers
/// </summary>
[Service(typeof(IMountWatcherFactory), InjectionLifetime = InjectionLifetime.Singleton)]
public class MountWatcherFactory(IWebLogger logger) : IMountWatcherFactory
{
	/// <summary>
	///     Create appropriate mount watcher for current OS
	/// </summary>
	public IMountWatcher CreateMountWatcher()
	{
		if ( OperatingSystem.IsMacOS() )
		{
			return new MacMountWatcher(logger);
		}

		if ( OperatingSystem.IsWindows() )
		{
			return new WindowsMountWatcher(logger);
		}

		if ( OperatingSystem.IsLinux() )
		{
			return new LinuxMountWatcher(logger);
		}

		throw new NotSupportedException("Operating system is not supported");
	}
}
