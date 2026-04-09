using System;

namespace starsky.foundation.mountwatch.MountWatcher.Linux.Interfaces;

internal interface ILinuxMountWatcherSystem
{
	IntPtr UdevNew();
	void UdevUnref(IntPtr udev);
	IntPtr UdevMonitorNewFromNetlink(IntPtr udev, string name);

	int UdevMonitorFilterAddMatchSubsystemDevtype(IntPtr monitor, string subsystem,
		IntPtr devtype);

	int UdevMonitorEnableReceiving(IntPtr monitor);
	int UdevMonitorGetFd(IntPtr monitor);
	IntPtr UdevMonitorReceiveDevice(IntPtr monitor);
	void UdevDeviceUnref(IntPtr device);
	string? UdevDeviceGetDevnode(IntPtr device);
	void UdevMonitorUnref(IntPtr monitor);
	void Sleep(int milliseconds);
}
