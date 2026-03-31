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
public class MountWatcherFactory : IMountWatcherFactory
{
	private readonly IWebLogger? _logger;

	public MountWatcherFactory(IWebLogger? logger = null)
	{
		_logger = logger;
	}

	/// <summary>
	///     Create appropriate mount watcher for current OS
	/// </summary>
	public IMountWatcher CreateMountWatcher()
	{
		if ( OperatingSystem.IsMacOS() )
		{
			return new MacMountWatcher(_logger);
		}

		if ( OperatingSystem.IsWindows() )
		{
			return new WindowsMountWatcher(_logger);
		}

		if ( OperatingSystem.IsLinux() )
		{
			return new LinuxMountWatcher(_logger);
		}

		throw new NotSupportedException("Operating system is not supported");
	}
}
