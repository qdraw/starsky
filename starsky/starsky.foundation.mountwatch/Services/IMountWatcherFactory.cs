using starsky.foundation.mountwatch.Interfaces;

namespace starsky.foundation.mountwatch.Services;

/// <summary>
///     Factory interface for creating mount watchers
/// </summary>
public interface IMountWatcherFactory
{
	/// <summary>
	///     Create OS-specific mount watcher
	/// </summary>
	IMountWatcher CreateMountWatcher();
}

