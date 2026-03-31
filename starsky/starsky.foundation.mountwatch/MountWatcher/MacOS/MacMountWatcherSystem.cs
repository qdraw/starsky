using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace starsky.foundation.mountwatch.MountWatcher.MacOS;

/// <summary>
///     macOS DiskArbitration and CoreFoundation P/Invoke wrapper
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MacMountWatcherSystem
{
	private const uint CfStringEncodingUtf8 = 0x08000100;
	private const string CfRunLoopDefaultMode = "kCFRunLoopDefaultMode";

	internal uint GetCfStringEncodingUtf8() => CfStringEncodingUtf8;

	internal string GetCfRunLoopDefaultMode() => CfRunLoopDefaultMode;

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	internal static extern IntPtr DASessionCreate(IntPtr allocator);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	internal static extern void DASessionScheduleWithRunLoop(
		IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	internal static extern void DASessionUnscheduleWithRunLoop(
		IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	internal static extern void DARegisterDiskAppearedCallback(
		IntPtr session, IntPtr match, MacMountWatcherDelegate.DiskAppearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	internal static extern void DARegisterDiskDisappearedCallback(
		IntPtr session, IntPtr match, MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	internal static extern IntPtr CFRunLoopGetCurrent();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	internal static extern void CFRunLoopRun();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	internal static extern void CFRunLoopStop(IntPtr runLoop);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	internal static extern IntPtr CFStringCreateWithCString(
		IntPtr allocator, string cStr, uint encoding);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	internal static extern void CFRelease(IntPtr cf);
}

/// <summary>
///     DiskArbitration framework delegate types
/// </summary>
internal static class MacMountWatcherDelegate
{
	internal delegate void DiskAppearedCallback(IntPtr diskRef, IntPtr context);
	internal delegate void DiskDisappearedCallback(IntPtr diskRef, IntPtr context);
}

