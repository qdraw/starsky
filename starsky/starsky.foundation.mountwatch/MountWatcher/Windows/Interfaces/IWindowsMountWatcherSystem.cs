using System.Collections.Generic;
using System.IO;
using System.Management;

namespace starsky.foundation.mountwatch.MountWatcher.Windows.Interfaces;

internal interface IWindowsMountWatcherSystem
{
	// Create a management watcher for the given WQL query; returned as object to avoid
	// platform-specific type usages in cross-platform files.
	object? CreateManagementWatcher(string wqlQuery);

	void AddEventArrivedHandler(object watcher, EventArrivedEventHandler handler);

	void StartWatcher(object watcher);

	void StopWatcher(object watcher);

	void DisposeWatcher(object watcher);

	IEnumerable<DriveInfo> GetDrives();
}
