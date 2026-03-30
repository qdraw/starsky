using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using starsky.foundation.mountwatch.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     macOS mount watcher using DiskArbitration framework for event-driven notifications
/// </summary>
internal class MacMountWatcher : BaseMountWatcher
{
	private IntPtr _runLoop;

	private IntPtr _session;

	/// <summary>
	///     Start watching for mount events using DiskArbitration
	/// </summary>
	public override void Start()
	{
		if ( IsRunning )
		{
			return;
		}

		IsRunning = true;
		WatchThread = new Thread(RunWatcher) { IsBackground = true };
		WatchThread.Start();
	}

	/// <summary>
	///     Stop watching for mount events
	/// </summary>
	public override void Stop()
	{
		IsRunning = false;

		try
		{
			if ( _runLoop != IntPtr.Zero )
			{
				CFRunLoopStop(_runLoop);
			}
		}
		catch
		{
			// Ignore cleanup errors
		}

		WatchThread?.Join(TimeSpan.FromSeconds(5));
	}

	/// <summary>
	///     Get the list of currently mounted volumes
	/// </summary>
	public override List<string> GetMountedVolumes()
	{
		var mounts = new List<string>();

		try
		{
			const string volumesPath = "/Volumes";
			if ( Directory.Exists(volumesPath) )
			{
				var volumeInfo = new DirectoryInfo(volumesPath);
				mounts.AddRange(volumeInfo
					.GetDirectories()
					.Where(d => !d.Name.StartsWith("."))
					.Select(d => d.FullName));
			}

			if ( Directory.Exists("/") )
			{
				mounts.Add("/");
			}
		}
		catch
		{
			// Ignore errors
		}

		return mounts;
	}

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern IntPtr DASessionCreate(IntPtr allocator);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DASessionScheduleWithRunLoop(
		IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DARegisterDiskAppearedCallback(
		IntPtr session, IntPtr match, DiskAppearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DARegisterDiskDisappearedCallback(
		IntPtr session, IntPtr match, DiskDisappearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFRunLoopGetCurrent();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRunLoopRun();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRunLoopStop(IntPtr runLoop);

	/// <summary>
	///     Run the DiskArbitration event watcher
	/// </summary>
	private void RunWatcher()
	{
		try
		{
			_session = DASessionCreate(IntPtr.Zero);
			if ( _session == IntPtr.Zero )
			{
				// Fallback to polling if DiskArbitration is unavailable
				RunPollingFallback();
				return;
			}

			_runLoop = CFRunLoopGetCurrent();
			DASessionScheduleWithRunLoop(_session, _runLoop, IntPtr.Zero);

			// Register callbacks
			var diskAppearedCallback = new DiskAppearedCallback(OnDiskAppeared);
			var diskDisappearedCallback = new DiskDisappearedCallback(OnDiskDisappeared);

			DARegisterDiskAppearedCallback(_session, IntPtr.Zero, diskAppearedCallback,
				IntPtr.Zero);
			DARegisterDiskDisappearedCallback(_session, IntPtr.Zero, diskDisappearedCallback,
				IntPtr.Zero);

			// Keep the callback delegates alive
			GC.KeepAlive(diskAppearedCallback);
			GC.KeepAlive(diskDisappearedCallback);

			// Run the event loop
			CFRunLoopRun();
		}
		catch
		{
			// Fallback to polling on error
			RunPollingFallback();
		}
	}

	/// <summary>
	///     Called when a disk appears
	/// </summary>
	private void OnDiskAppeared(IntPtr diskRef, IntPtr context)
	{
		try
		{
			var mounts = GetMountedVolumes();
			if ( mounts.Any() )
			{
				OnMountDetected(mounts.First());
			}
		}
		catch
		{
			// Ignore errors
		}
	}

	/// <summary>
	///     Called when a disk disappears
	/// </summary>
	private void OnDiskDisappeared(IntPtr diskRef, IntPtr context)
	{
		// Disk disappeared - could trigger cleanup logic if needed
	}


	// DiskArbitration framework P/Invoke declarations
	private delegate void DiskAppearedCallback(IntPtr diskRef, IntPtr context);

	private delegate void DiskDisappearedCallback(IntPtr diskRef, IntPtr context);
}
