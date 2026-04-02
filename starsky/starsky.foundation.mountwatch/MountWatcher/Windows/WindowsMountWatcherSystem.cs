using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management;
using starsky.foundation.mountwatch.MountWatcher.Windows.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher.Windows;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
[ExcludeFromCodeCoverage]
internal sealed class WindowsMountWatcherSystem : IWindowsMountWatcherSystem
{
	public object? CreateManagementWatcher(string wqlQuery)
	{
		var query = new WqlEventQuery(wqlQuery);
		var watcher = new ManagementEventWatcher(query);
		return watcher;
	}

	public void AddEventArrivedHandler(object watcher, EventArrivedEventHandler handler)
	{
		if ( watcher is ManagementEventWatcher mgmt )
		{
			mgmt.EventArrived += handler;
		}
	}

	public void StartWatcher(object watcher)
	{
		if ( watcher is ManagementEventWatcher mgmt )
		{
			mgmt.Start();
		}
	}

	public void StopWatcher(object watcher)
	{
		if ( watcher is ManagementEventWatcher mgmt )
		{
			mgmt.Stop();
		}
	}

	public void DisposeWatcher(object watcher)
	{
		if ( watcher is ManagementEventWatcher mgmt )
		{
			mgmt.Dispose();
		}
	}

	public IEnumerable<DriveInfo> GetDrives()
	{
		return DriveInfo.GetDrives();
	}
}
