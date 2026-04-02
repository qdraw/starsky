using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using starsky.foundation.mountwatch.MountWatcher.MacOS.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher.MacOS;

/// <summary>
///     macOS DiskArbitration and CoreFoundation P/Invoke wrapper
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Interoperability",
	"SYSLIB1054:Use \'LibraryImportAttribute\' " +
	"instead of \'DllImportAttribute\' to " +
	"generate P/Invoke marshalling code at compile time")]
[SuppressMessage("Globalization", "CA2101:Specify marshaling " +
                                  "for P/Invoke string arguments")]
internal sealed class MacMountWatcherSystem : IMacMountWatcherSystem
{
	private const uint CfStringEncodingUtf8 = 0x08000100;
	private const string CfRunLoopDefaultMode = "kCFRunLoopDefaultMode";

	public uint GetCfStringEncodingUtf8()
	{
		return CfStringEncodingUtf8;
	}

	public string GetCfRunLoopDefaultMode()
	{
		return CfRunLoopDefaultMode;
	}

	// Instance wrappers that forward to static P/Invoke methods
	public IntPtr DASessionCreate(IntPtr allocator)
	{
		return DASessionCreateNative(allocator);
	}

	public void DASessionScheduleWithRunLoop(IntPtr session, IntPtr runLoop, IntPtr runLoopMode)
	{
		DASessionScheduleWithRunLoopNative(session, runLoop, runLoopMode);
	}

	public void DASessionUnscheduleWithRunLoop(IntPtr session, IntPtr runLoop, IntPtr runLoopMode)
	{
		DASessionUnscheduleWithRunLoopNative(session, runLoop, runLoopMode);
	}

	public void DARegisterDiskAppearedCallback(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskAppearedCallback callback, IntPtr context)
	{
		DARegisterDiskAppearedCallbackNative(session, match, callback, context);
	}

	public void DARegisterDiskDisappearedCallback(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context)
	{
		DARegisterDiskDisappearedCallbackNative(session, match, callback, context);
	}

	public IntPtr CFRunLoopGetCurrent()
	{
		return CFRunLoopGetCurrentNative();
	}

	public void CFRunLoopRun()
	{
		CFRunLoopRunNative();
	}

	public void CFRunLoopStop(IntPtr runLoop)
	{
		CFRunLoopStopNative(runLoop);
	}

	public IntPtr CFStringCreateWithCString(IntPtr allocator, string cStr, uint encoding)
	{
		return CFStringCreateWithCStringNative(allocator, cStr, encoding);
	}

	public void CFRelease(IntPtr cf)
	{
		CFReleaseNative(cf);
	}

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern IntPtr DASessionCreateNative(IntPtr allocator);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DASessionScheduleWithRunLoopNative(IntPtr session, IntPtr runLoop,
		IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DASessionUnscheduleWithRunLoopNative(IntPtr session, IntPtr runLoop,
		IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DARegisterDiskAppearedCallbackNative(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskAppearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DARegisterDiskDisappearedCallbackNative(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFRunLoopGetCurrentNative();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRunLoopRunNative();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRunLoopStopNative(IntPtr runLoop);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFStringCreateWithCStringNative(IntPtr allocator, string cStr,
		uint encoding);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFReleaseNative(IntPtr cf);
}

/// <summary>
///     DiskArbitration framework delegate types
/// </summary>
internal static class MacMountWatcherDelegate
{
	internal delegate void DiskAppearedCallback(IntPtr diskRef, IntPtr context);

	internal delegate void DiskDisappearedCallback(IntPtr diskRef, IntPtr context);
}
