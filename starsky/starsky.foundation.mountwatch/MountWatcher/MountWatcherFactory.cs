using System;
using System.Runtime.InteropServices;
using starsky.foundation.injection;
using starsky.foundation.mountwatch.MountWatcher.Interfaces;
using starsky.foundation.mountwatch.Services;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Factory for creating OS-specific mount watchers
/// </summary>
[Service(typeof(IMountWatcherFactory), InjectionLifetime = InjectionLifetime.Scoped)]
public class MountWatcherFactory(ISelectorStorage selectorStorage, IWebLogger logger)
	: IMountWatcherFactory
{
	private readonly Func<OSPlatform> _platformResolver =
		OperatingSystemHelper.GetPlatform;

	private readonly int _pollIntervalMs = 2000;

	internal MountWatcherFactory(ISelectorStorage selectorStorage, IWebLogger logger,
		Func<OSPlatform> platformResolver, int pollIntervalMs) : this(selectorStorage, logger)
	{
		_platformResolver = platformResolver;
		_pollIntervalMs = pollIntervalMs;
	}

	/// <summary>
	///     Create appropriate mount watcher for current OS
	/// </summary>
	public IMountWatcher CreateMountWatcher()
	{
		var platform = _platformResolver();

		if ( platform == OSPlatform.OSX )
		{
			return new MacMountWatcher(logger, _pollIntervalMs);
		}

		if ( platform == OSPlatform.Windows )
		{
			return new WindowsMountWatcher(logger, _pollIntervalMs);
		}

		if ( platform == OSPlatform.Linux )
		{
			return new LinuxMountWatcher(selectorStorage, logger, _pollIntervalMs);
		}

		throw new NotSupportedException("Operating system is not supported");
	}
}
