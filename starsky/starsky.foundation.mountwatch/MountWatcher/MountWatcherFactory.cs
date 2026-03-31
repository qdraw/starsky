using System;
using System.Runtime.InteropServices;
using starsky.foundation.injection;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Factory for creating OS-specific mount watchers
/// </summary>
[Service(typeof(IMountWatcherFactory), InjectionLifetime = InjectionLifetime.Singleton)]
public class MountWatcherFactory(IWebLogger logger) : IMountWatcherFactory
{
	private readonly Func<OSPlatform> _platformResolver = OperatingSystemHelper.GetPlatform;

	internal MountWatcherFactory(IWebLogger logger,
		Func<OSPlatform> platformResolver) : this(logger)
	{
		_platformResolver = platformResolver;
	}

	/// <summary>
	///     Create appropriate mount watcher for current OS
	/// </summary>
	public IMountWatcher CreateMountWatcher()
	{
		var platform = _platformResolver();

		if ( platform == OSPlatform.OSX )
		{
			return new MacMountWatcher(logger);
		}

		if ( platform == OSPlatform.Windows )
		{
			return new WindowsMountWatcher(logger);
		}

		if ( platform == OSPlatform.Linux )
		{
			return new LinuxMountWatcher(logger);
		}

		throw new NotSupportedException("Operating system is not supported");
	}
}
