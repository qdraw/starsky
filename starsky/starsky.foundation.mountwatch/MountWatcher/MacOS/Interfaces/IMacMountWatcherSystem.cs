using System;
using System.Diagnostics.CodeAnalysis;

namespace starsky.foundation.mountwatch.MountWatcher.MacOS.Interfaces;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal interface IMacMountWatcherSystem
{
	uint GetCfStringEncodingUtf8();
	string GetCfRunLoopDefaultMode();

	IntPtr DASessionCreateApi(IntPtr allocator);
	void DASessionScheduleWithRunLoopApi(IntPtr session, IntPtr runLoop, IntPtr runLoopMode);
	void DASessionUnscheduleWithRunLoopApi(IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

	void DARegisterDiskAppearedCallbackApi(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskAppearedCallback callback, IntPtr context);

	void DARegisterDiskDisappearedCallbackApi(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context);

	// CoreFoundation
	IntPtr CFRunLoopGetCurrentApi();
	void CFRunLoopRunApi();
	void CFRunLoopStopApi(IntPtr runLoop);
	IntPtr CFStringCreateWithCStringApi(IntPtr allocator, string cStr, uint encoding);
	void CFReleaseApi(IntPtr cf);
}
