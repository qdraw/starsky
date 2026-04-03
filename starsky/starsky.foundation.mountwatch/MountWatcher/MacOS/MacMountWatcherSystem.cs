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
[SuppressMessage("Usage",
	"S4200: Make this wrapper for native method less trivial")]
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
	public IntPtr DASessionCreateApi(IntPtr allocator)
	{
		return DASessionCreate(allocator);
	}

	public void DASessionScheduleWithRunLoopApi(IntPtr session, IntPtr runLoop, IntPtr runLoopMode)
	{
		DASessionScheduleWithRunLoop(session, runLoop, runLoopMode);
	}

	public void DASessionUnscheduleWithRunLoopApi(IntPtr session, IntPtr runLoop,
		IntPtr runLoopMode)
	{
		DASessionUnscheduleWithRunLoop(session, runLoop, runLoopMode);
	}

	public void DARegisterDiskAppearedCallbackApi(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskAppearedCallback callback, IntPtr context)
	{
		DARegisterDiskAppearedCallback(session, match, callback, context);
	}

	public void DARegisterDiskDisappearedCallbackApi(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context)
	{
		DARegisterDiskDisappearedCallback(session, match, callback, context);
	}

	public IntPtr CFRunLoopGetCurrentApi()
	{
		return CFRunLoopGetCurrent();
	}

	public void CFRunLoopRunApi()
	{
		CFRunLoopRun();
	}

	public void CFRunLoopStopApi(IntPtr runLoop)
	{
		CFRunLoopStop(runLoop);
	}

	public IntPtr CFStringCreateWithCStringApi(IntPtr allocator, string cStr, uint encoding)
	{
		return CFStringCreateWithCString(allocator, cStr, encoding);
	}

	public void CFReleaseApi(IntPtr cf)
	{
		CFRelease(cf);
	}

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration",
		EntryPoint = "DASessionCreate")]
	private static extern IntPtr DASessionCreate(IntPtr allocator);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration",
		EntryPoint = "DASessionScheduleWithRunLoop")]
	private static extern void DASessionScheduleWithRunLoop(IntPtr session, IntPtr runLoop,
		IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration",
		EntryPoint = "DASessionUnscheduleWithRunLoop")]
	private static extern void DASessionUnscheduleWithRunLoop(IntPtr session, IntPtr runLoop,
		IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration",
		EntryPoint = "DARegisterDiskAppearedCallback")]
	private static extern void DARegisterDiskAppearedCallback(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskAppearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration",
		EntryPoint = "DARegisterDiskDisappearedCallback")]
	private static extern void DARegisterDiskDisappearedCallback(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation",
		EntryPoint = "CFRunLoopGetCurrent")]
	private static extern IntPtr CFRunLoopGetCurrent();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation",
		EntryPoint = "CFRunLoopRun")]
	private static extern void CFRunLoopRun();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation",
		EntryPoint = "CFRunLoopStop")]
	private static extern void CFRunLoopStop(IntPtr runLoop);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation",
		EntryPoint = "CFStringCreateWithCString")]
	private static extern IntPtr CFStringCreateWithCString(IntPtr allocator, string cStr,
		uint encoding);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation",
		EntryPoint = "CFRelease")]
	private static extern void CFRelease(IntPtr cf);
}

/// <summary>
///     DiskArbitration framework delegate types
/// </summary>
internal static class MacMountWatcherDelegate
{
	internal delegate void DiskAppearedCallback(IntPtr diskRef, IntPtr context);

	internal delegate void DiskDisappearedCallback(IntPtr diskRef, IntPtr context);
}
