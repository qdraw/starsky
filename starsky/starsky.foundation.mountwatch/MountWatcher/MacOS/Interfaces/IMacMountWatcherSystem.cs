using System;
using System.Diagnostics.CodeAnalysis;

namespace starsky.foundation.mountwatch.MountWatcher.MacOS.Interfaces;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal interface IMacMountWatcherSystem
{
	uint GetCfStringEncodingUtf8();
	string GetCfRunLoopDefaultMode();

	IntPtr DASessionCreate(IntPtr allocator);
	void DASessionScheduleWithRunLoop(IntPtr session, IntPtr runLoop, IntPtr runLoopMode);
	void DASessionUnscheduleWithRunLoop(IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

	void DARegisterDiskAppearedCallback(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskAppearedCallback callback, IntPtr context);

	void DARegisterDiskDisappearedCallback(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context);

	// CoreFoundation
	IntPtr CFRunLoopGetCurrent();
	void CFRunLoopRun();
	void CFRunLoopStop(IntPtr runLoop);
	IntPtr CFStringCreateWithCString(IntPtr allocator, string cStr, uint encoding);
	void CFRelease(IntPtr cf);
}
